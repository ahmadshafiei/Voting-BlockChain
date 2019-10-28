using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Voting.Infrastructure.Utility;
using System.Diagnostics;
using Newtonsoft.Json;
using Votin.Model.Entities;
using Voting.Infrastructure.Services.BlockChainServices;

namespace Voting.Infrastructure.PeerToPeer
{
    public class P2PNetwork
    {
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent messageManualReset = new ManualResetEvent(false);
        private readonly IPAddress localhost = IPAddress.Parse("127.0.0.1");

        private BlockChainService _blockChainServie;

        /// <summary>
        /// Exposed with this port
        /// </summary>
        public int _p2pPort;

        /// <summary>
        /// Initial peers
        /// </summary>
        public List<string> _peers = Environment.GetEnvironmentVariable("PEERS") != null ?
            Environment.GetEnvironmentVariable("PEERS").Split(',').ToList() :
            new List<string>();

        /// <summary>
        /// All of peers
        /// </summary>
        private List<Socket> _sockets = new List<Socket>();

        public P2PNetwork(IConfiguration configuration, BlockChainService blockChainService)
        {
            _blockChainServie = blockChainService;
            _p2pPort = Environment.GetEnvironmentVariable("P2P_PORT") != null ?
                Convert.ToInt32(Environment.GetEnvironmentVariable("P2P_PORT")) :
                Convert.ToInt32(configuration.GetSection("P2P").GetSection("DEFAULT_PORT").Value);

            Console.WriteLine($"Current P2P_Port : {_p2pPort}");
        }

        public void InitialNetwrok()
        {
            ConnectToPeers();
            ListenForPeers();
        }

        private void ConnectToPeers()
        {
            _peers.ForEach(p => AddPeer(p));
        }

        private void AddPeer(string peerAddress)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(IPAddress.Parse(peerAddress.Split(':')[1].Substring(2)), Convert.ToInt32(peerAddress.Split(':')[2]));
                _sockets.Add(socket);
                MessageHandler(socket);
                SendMessage(socket);
                Debug.WriteLine($"Connected to initial peer {peerAddress}");
            }
            catch (Exception e)
            {
                Console.WriteLine("---------------------- Error ---------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------------------------------");
            }
        }

        private void ListenForPeers()
        {
            Task.Run(() =>
            {
                var server = new TcpListener(localhost, _p2pPort);
                server.Start();

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting For Connection ...");
                    server.BeginAcceptSocket(AddSocket, server);

                    allDone.WaitOne();
                }
            });
        }

        private void AddSocket(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            var clientSocket = listener.EndAcceptSocket(ar);

            _sockets.Add(clientSocket);
            MessageHandler(clientSocket);
            SendMessage(clientSocket);

            Console.WriteLine("Peer Connected");

            allDone.Set();
        }

        private void SendMessage(Socket socket)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(BlockChain.Chain));
            byte[] dataLength = data.Length.To4Byte();
            byte[] packet = dataLength.Concat(data).ToArray();

            try
            {
                int s = socket.Send(packet);
            }
            catch (Exception e)
            {

                throw;
            }

            Console.WriteLine("Data Sent");
        }

        private void MessageHandler(Socket socket)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    messageManualReset.Reset();

                    StateObject state = new StateObject
                    {
                        workSocket = socket
                    };

                    byte[] bufferSize = new byte[4];
                    socket.Receive(bufferSize);

                    //Little Endian
                    state.BufferSize = BitConverter.ToInt32(bufferSize.Reverse().ToArray());

                    Console.WriteLine();
                    Console.WriteLine("WAITING FOR MESSAGE");
                    Console.WriteLine();

                    socket.BeginReceive(state.buffer, 0, state.BufferSize, SocketFlags.None, HandleData, state);

                    messageManualReset.WaitOne();
                }

            });
        }

        private void HandleData(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            try
            {
                List<Block> incomingChain = state.data;

                _blockChainServie.ReplaceChain(incomingChain);

                Console.WriteLine("Received Data : ");
                Console.WriteLine(Encoding.UTF8.GetString(state.buffer));

            }
            finally
            {
                messageManualReset.Set();
                state.workSocket.EndReceive(ar);
            }
        }

        public void SyncChains()
        {
            _sockets.ForEach(s => SendMessage(s));
        }
    }

    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket;

        // Size of receive buffer.  
        private int _bufferSize;
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }

            set
            {
                _bufferSize = value;
                buffer = new byte[value];
            }
        }

        // Receive buffer.  
        public byte[] buffer;
        // Received data string.  
        public List<Block> data
        {
            get
            {
                return JsonConvert.DeserializeObject<List<Block>>(Encoding.UTF8.GetString(buffer));
            }
        }
    }
}