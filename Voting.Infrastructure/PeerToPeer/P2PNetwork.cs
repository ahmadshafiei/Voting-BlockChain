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

            ListenForPeers();
            ConnectToPeers();
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

                    Thread.Sleep(1000);

                    server.BeginAcceptSocket(AddSocket, server);

                    Console.WriteLine("Waiting For Connection ...");
                    allDone.WaitOne();
                }
            });
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
                Console.WriteLine($"Connected to initial peer {peerAddress}");
            }
            catch (Exception e)
            {
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine($"Error : Invalid initial peer address {peerAddress}");
                Console.WriteLine("-------------------------------------------------");
            }
        }

        private void AddSocket(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            var clientSocket = listener.EndAcceptSocket(ar);

            _sockets.Add(clientSocket);

            Console.WriteLine("Peer Connected");

            allDone.Set();
        }
    }
}