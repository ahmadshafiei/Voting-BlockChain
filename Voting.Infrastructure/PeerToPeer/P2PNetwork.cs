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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Voting.Model.Entities;
using Voting.Infrastructure.Services.BlockChainServices;
using Voting.Infrastructure.Services;
using Voting.Model.Context;

namespace Voting.Infrastructure.PeerToPeer
{
    public class P2PNetwork
    {
        private delegate void MessageHandler(IAsyncResult ar);

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent messageManualReset = new ManualResetEvent(false);
        private readonly IPAddress localhost = IPAddress.Parse("127.0.0.1");

        private BlockChainService _blockChainService;
        private TransactionPoolService _transactionPoolService;

        /// <summary>
        /// Exposed with this port
        /// </summary>
        public int _p2pPort;

        /// <summary>
        /// Initial peers
        /// </summary>
        public List<string> _peers = Environment.GetEnvironmentVariable("PEERS") != null
            ? Environment.GetEnvironmentVariable("PEERS").Split(',').ToList()
            : new List<string>();

        /// <summary>
        /// All of peers
        /// </summary>
        private List<Socket> _sockets = new List<Socket>();

        private readonly IServiceProvider _serviceProvider;

        public P2PNetwork(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _p2pPort = Environment.GetEnvironmentVariable("P2P_PORT") != null
                ? Convert.ToInt32(Environment.GetEnvironmentVariable("P2P_PORT"))
                : Convert.ToInt32(configuration.GetSection("P2P").GetSection("DEFAULT_PORT").Value);

            Console.WriteLine($"Current P2P_Port : {_p2pPort}");
            Console.WriteLine($"Initial Peers : {JsonConvert.SerializeObject(_peers)}");
        }

        public async Task InitialNetwrok()
        {
            await InitialBlockchain();
            ConnectToPeers();
            ListenForPeers();
        }

        private async Task InitialBlockchain()
        {
            var db = _serviceProvider.GetService<BlockchainContext>();

            List<Block> blockchain = await db.Blocks.ToListAsync();

            BlockChain.Chain = blockchain;
        }

        private void ConnectToPeers()
        {
            _peers.ForEach(p => AddPeer(p));
        }

        private void AddPeer(string peerAddress)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress socketIP = null;
            int socketPort = 0;

            try
            {
                socketIP = IPAddress.Parse(peerAddress.Split(':')[1].Substring(2));
                socketPort = Convert.ToInt32(peerAddress.Split(':')[2]);
            }
            catch (Exception e)
            {
                Console.WriteLine("---------- Invalid socket address of peer ----------");
                Console.WriteLine("---------- Valid form of socket address : ws://X.X.X.X:PORT ----------");
                return;
            }

            try
            {
                socket.Connect(socketIP, socketPort);
                _sockets.Add(socket);
                BlockchainMessageHandler(socket);
                SendChainToPeers(socket);
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

                    Console.WriteLine($"Waiting For Connection on port {_p2pPort} ...");
                    server.BeginAcceptSocket(AddSocket, server);

                    allDone.WaitOne();
                }
            });
        }

        private void AddSocket(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener) ar.AsyncState;

            var clientSocket = listener.EndAcceptSocket(ar);

            _sockets.Add(clientSocket);
            BlockchainMessageHandler(clientSocket);
            SendChainToPeers(clientSocket);

            Console.WriteLine("Peer Connected");

            allDone.Set();
        }

        private void SendChainToPeers(Socket socket)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(BlockChain.Chain));
            byte[] dataType = new byte[] {Convert.ToByte((int) MessageType.Blockchain)};
            byte[] dataLength = data.Length.To4Byte();
            byte[] packet = dataType.Concat(dataLength).Concat(data).ToArray();

            try
            {
                int s = socket.Send(packet);
                Console.WriteLine("Blockchain Broadcasted to peers");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void BroadcastTransactionToPeers(Socket socket, Transaction transaction)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transaction));
            byte[] dataType = new byte[] {Convert.ToByte((int) MessageType.Transaction)};
            byte[] dataLength = data.Length.To4Byte();
            byte[] packet = dataType.Concat(dataLength).Concat(data).ToArray();

            try
            {
                int s = socket.Send(packet);
                Console.WriteLine("Transaction Broadcasted to peers");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void BroadcastClearTransactionPoolToPeers(Socket socket)
        {
            byte[] dataType = new byte[] {Convert.ToByte(value: (int) MessageType.ClearTransaction)};
            byte[] packet = dataType.ToArray();

            try
            {
                int s = socket.Send(packet);
                Console.WriteLine("TransactionPool is clear");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void BlockchainMessageHandler(Socket socket)
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

                    byte[] messageType = new byte[1];
                    socket.Receive(messageType);

                    AsyncCallback handler = HandleTransactionData;

                    if ((MessageType) messageType.First() == MessageType.Blockchain)
                        handler = HandleBlockchainData;
                    else if ((MessageType) messageType.First() == MessageType.Transaction)
                        handler = HandleTransactionData;
                    else if ((MessageType) messageType.First() == MessageType.ClearTransaction)
                    {
                        _transactionPoolService = _serviceProvider.GetService<TransactionPoolService>();
                        _transactionPoolService.ClearPool();
                        messageManualReset.Set();
                        continue;
                    }

                    byte[] bufferSize = new byte[4];
                    socket.Receive(bufferSize);

                    //Little Endian
                    state.BufferSize = BitConverter.ToInt32(bufferSize.Reverse().ToArray());

                    try
                    {
                        socket.BeginReceive(state.buffer, 0, state.BufferSize, SocketFlags.None, handler, state);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        messageManualReset.WaitOne();
                    }
                }
            });
        }

        private void HandleBlockchainData(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;

            try
            {
                List<Block> incomingChain = state.BLockchain;

                _blockChainService = _serviceProvider.GetService<BlockChainService>();
                _blockChainService.ReplaceChain(incomingChain);

                Console.WriteLine("Received Blockchain : ");
                Console.WriteLine(Encoding.UTF8.GetString(state.buffer));
            }
            finally
            {
                messageManualReset.Set();
                state.workSocket.EndReceive(ar);
            }
        }

        private void HandleTransactionData(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;

            try
            {
                Transaction transaction = state.Transaction;

                _transactionPoolService = _serviceProvider.GetService<TransactionPoolService>();
                _transactionPoolService.UpdateOrAddTransaction(transaction);

                Console.WriteLine("Received Transaction : ");
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
            _sockets.ForEach(s => SendChainToPeers(s));
        }

        public void BroadcastTransaction(Transaction transaction)
        {
            _sockets.ForEach(s => BroadcastTransactionToPeers(s, transaction));
        }

        public void BroadcastClearTransactionPool()
        {
            _sockets.ForEach(s => BroadcastClearTransactionPoolToPeers(s));
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
            get { return _bufferSize; }

            set
            {
                _bufferSize = value;
                buffer = new byte[value];
            }
        }

        // Receive buffer.  
        public byte[] buffer;

        // Received data string.  
        public List<Block> BLockchain
        {
            get { return JsonConvert.DeserializeObject<List<Block>>(Encoding.UTF8.GetString(buffer)); }
        }

        public Transaction Transaction
        {
            get { return JsonConvert.DeserializeObject<Transaction>(Encoding.UTF8.GetString(buffer)); }
        }

        public MessageType MessageType { get; set; }
    }

    public enum MessageType : byte
    {
        Blockchain = 1,
        Transaction = 2,
        ClearTransaction = 3
    }
}