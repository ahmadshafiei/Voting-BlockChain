﻿using Microsoft.AspNetCore.SignalR;
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
using Voting.Infrastructure.Services;

namespace Voting.Infrastructure.PeerToPeer
{
    public class P2PNetwork
    {
        private delegate void MessageHandler(IAsyncResult ar);
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent messageManualReset = new ManualResetEvent(false);
        private readonly IPAddress localhost = IPAddress.Parse("127.0.0.1");

        private BlockChainService _blockChainServie;
        private TransactionPoolService _transactionPoolService;

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

        public P2PNetwork(IConfiguration configuration, BlockChainService blockChainService, IServiceProvider serviceProvider)
        {
            _blockChainServie = blockChainService;
            _p2pPort = Environment.GetEnvironmentVariable("P2P_PORT") != null ?
                Convert.ToInt32(Environment.GetEnvironmentVariable("P2P_PORT")) :
                Convert.ToInt32(configuration.GetSection("P2P").GetSection("DEFAULT_PORT").Value);

            _transactionPoolService = serviceProvider.GetService<TransactionPoolService>();

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
                BlockchainMessageHandler(socket);
                SendChainToPeers(socket);
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
            BlockchainMessageHandler(clientSocket);
            SendChainToPeers(clientSocket);

            Console.WriteLine("Peer Connected");

            allDone.Set();
        }

        private void SendChainToPeers(Socket socket)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(BlockChain.Chain));
            byte[] dataType = new byte[] { Convert.ToByte((int)MessageType.Blockchain) };
            byte[] dataLength = data.Length.To4Byte();
            byte[] packet = dataType.Concat(dataLength).Concat(data).ToArray();

            try
            {
                int s = socket.Send(packet);
            }
            catch (Exception e)
            {

                throw;
            }

            Console.WriteLine("Blockchain Data Sent");
        }

        private void BroadcastTransactionToPeers(Socket socket, Transaction transaction)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transaction));
            byte[] dataType = new byte[] { Convert.ToByte((int)MessageType.Transaction) };
            byte[] dataLength = data.Length.To4Byte();
            byte[] packet = dataType.Concat(dataLength).Concat(data).ToArray();

            try
            {
                int s = socket.Send(packet);
            }
            catch (Exception e)
            {
                throw;
            }

            Console.WriteLine("Transaction Data Sent");

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

                    if (messageType.First() == 1)
                        handler = HandleBlockchainData;
                    else if (messageType.First() == 2)
                        handler = HandleTransactionData;

                    byte[] bufferSize = new byte[4];
                    socket.Receive(bufferSize);

                    //Little Endian
                    state.BufferSize = BitConverter.ToInt32(bufferSize.Reverse().ToArray());

                    Console.WriteLine();
                    Console.WriteLine("WAITING FOR MESSAGE");
                    Console.WriteLine();

                    try
                    {
                        socket.BeginReceive(state.buffer, 0, state.BufferSize, SocketFlags.None, handler, state);
                    }
                    catch (Exception e)
                    {

                        throw;
                    }

                    messageManualReset.WaitOne();
                }

            });
        }

        private void HandleBlockchainData(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            try
            {
                List<Block> incomingChain = state.BLockchain;

                _blockChainServie.ReplaceChain(incomingChain);

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
            var state = (StateObject)ar.AsyncState;

            try
            {
                Transaction transaction = state.Transaction;

                _transactionPoolService.UpdateOrAddTransaction(transaction);
                //_blockChainServie.ReplaceChain(incomingChain);

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
        public List<Block> BLockchain
        {
            get
            {
                return JsonConvert.DeserializeObject<List<Block>>(Encoding.UTF8.GetString(buffer));
            }
        }
        public Transaction Transaction
        {
            get
            {
                return JsonConvert.DeserializeObject<Transaction>(Encoding.UTF8.GetString(buffer));
            }
        }
        public MessageType MessageType { get; set; }
    }

    public enum MessageType : byte
    {
        Blockchain = 1,
        Transaction = 2
    }
}