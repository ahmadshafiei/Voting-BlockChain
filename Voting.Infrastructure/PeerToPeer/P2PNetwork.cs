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

namespace Voting.Infrastructure.PeerToPeer
{
    public class P2PNetwork
    {
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent messageManualReset = new ManualResetEvent(false);
        private readonly IPAddress localhost = IPAddress.Parse("127.0.0.1");

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

        public P2PNetwork(IConfiguration configuration)
        {
            _p2pPort = Environment.GetEnvironmentVariable("P2P_PORT") != null ?
                Convert.ToInt32(Environment.GetEnvironmentVariable("P2P_PORT")) :
                Convert.ToInt32(configuration.GetSection("P2P").GetSection("DEFAULT_PORT").Value);

            Console.WriteLine($"Current P2P_Port : {_p2pPort}");

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
                Console.WriteLine($"Connected to initial peer {peerAddress}");
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

            Console.WriteLine("Peer Connected");

            allDone.Set();
        }

        private void SendMessage(Socket socket)
        {
            byte[] data = Encoding.UTF8.GetBytes("SUCK IT");
            byte[] dataLength = BitConverter.GetBytes(data.Length);
            byte[] packet = dataLength.Concat(data).ToArray();

            Console.WriteLine("Begin Message Send");

            socket.Send(packet);

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

                    //byte[] bufferSize = new byte[4];
                    //socket.Receive(bufferSize);

                    //state.BufferSize = Convert.ToInt32(bufferSize);

                    socket.BeginReceive(state.buffer, 0, 1000, SocketFlags.None, HandleData, state);

                    messageManualReset.WaitOne();
                }

            });
        }

        private void HandleData(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            var s = Encoding.UTF8.GetString(state.buffer);

            state.workSocket.EndAccept(ar);

            Console.WriteLine(state.data);

            messageManualReset.Set();
        }
    }

    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;

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
        public byte[] buffer = new byte[1000];
        // Received data string.  
        public string data
        {
            get
            {
                return Convert.ToString(buffer);
            }
        }
    }
}