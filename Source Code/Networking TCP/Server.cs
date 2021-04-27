using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Networking.TCP
{
    /// <summary>
    /// A class that connects clients together
    /// and handles data transfer between clients
    /// using a TCP protocol
    /// </summary>
    public class Server
    {
        #region Server Properties

        TcpListener listener;

        #region Clients

        List<Client> ConnectedClients = new List<Client>();

        /// <summary>
        /// How many clients are connected to the server
        /// </summary>
        public int Connections 
        { 
            get 
            { 
                UpdateClientList();
                return ConnectedClients.Count;
            } 
        }

        /// <summary>
        /// Max no. of connected clients at a time.
        /// Default is 10
        /// </summary>
        public int MaxConnections = 10;

        #endregion

        #region Server Info

        readonly IPEndPoint IpAddress;
        public string IP { get { return IpAddress.Address.ToString(); } }
        public int Port { get { return IpAddress.Port; } }

        #endregion

        #region Events

        public delegate void GeneralEvent();

        event GeneralEvent ServerStarted;
        event GeneralEvent ServerClosed;
        event GeneralEvent ClientConnected;
        event GeneralEvent ServerFull;

        #endregion

        #endregion

        /// <summary>
        /// Once the server has started, it will only accept clients
        /// connecting from a specified port no.
        /// </summary>
        public Server(int PortNo)
        {
            IpAddress = new IPEndPoint(ShowMyIP(), PortNo);
        }

        /// <summary>
        /// Sets up event methods to a class inheriting from
        /// IServer, to its designated methods
        /// </summary>
        /// <param name="server">An instance of a class
        /// using the 'IServer' interface</param>
        public void AssignEvents(IServer server)
        {
            ServerStarted += server.OnServerStarted;
            ServerClosed += server.OnServerClosed;
            ClientConnected += server.OnClientConnected;
            ServerFull += server.OnServerFilled;
        }

        IPAddress ShowMyIP()
        {
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());

            for (int i = localIP.Length - 1; i >= 0; i--)
            {
                if (localIP[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return localIP[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Opens the server at the specified port and listens for connections
        /// </summary>
        public void Start()
        {
            // Opens up port and accepts any pending connection requests
            listener = new TcpListener(IPAddress.Any, IpAddress.Port);
            listener.Start();

            ListenForConnections();

            ServerStarted?.Invoke();
        }

        /// <summary>
        /// Closes the server and releases all connected clients
        /// </summary>
        public void Close()
        {
            if (listener != null && listener.Server.IsBound)
            {
                listener.Stop();
                ServerClosed?.Invoke();
            }
        }

        async void ListenForConnections()
        {
            while (listener.Server.IsBound)
            {
                await Task.Run(() =>
                {
                    var tcpClient = listener.AcceptTcpClient();

                    if (ConnectedClients.Count < MaxConnections)
                    {
                        Client client = new Client()
                        {
                            SR = new StreamReader(tcpClient.GetStream()),
                            SW = new StreamWriter(tcpClient.GetStream())
                        };
                        ConnectedClients.Add(client);

                        // Listen for any received data from that client
                        new Task(() => ReadStream(client.SR)).Start();
                    }
                    else
                    {
                        tcpClient.Close();
                    }
                });

                if (ConnectedClients.Count < MaxConnections)
                    ClientConnected?.Invoke();
                else
                    ServerFull?.Invoke();
            }
        }

        void ReadStream(StreamReader stream)
        {
            while (true)
            {
                var data = stream.ReadLine().Split('|');
                SendData(data);
            }
        }

        void SendData(string[] data)
        {
            if (data[0] == "-1")
            {
                foreach (var client in ConnectedClients)
                {
                    client.SW.WriteLine(data[1]);
                    client.SW.Flush();
                }
            }
            else
            {
                var client = ConnectedClients[int.Parse(data[0])];

                client.SW.WriteLine(data[1]);
                client.SW.Flush();
            }
        }

        void UpdateClientList()
        {
            var clients = new List<Client>();

            foreach (var client in ConnectedClients)
            {
                try
                {
                    // Stream has not been closed
                    var len = client.SR.BaseStream.Length;
                    clients.Add(client);
                }
                catch (Exception) { }
            }

            ConnectedClients = clients;
        }
    }
}
