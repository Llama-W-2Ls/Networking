using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking.TCP
{
    /// <summary>
    /// A class used to establish a connection to an
    /// available server and receive/transfer data
    /// using a TCP protocol
    /// </summary>
    public class Client
    {
        TcpClient client;

        public StreamReader SR;
        public StreamWriter SW;

        public bool IsConnected { get { return client.Connected; } }

        #region Events

        public delegate void DataEvent(string data);
        public delegate void GeneralEvent();
        public delegate void ErrorEvent();

        event DataEvent DataReceived;
        event GeneralEvent Connected;
        event ErrorEvent ConnectionFailed;
        event GeneralEvent Disconnected;

        #endregion

        /// <summary>
        /// Sets up event methods to a class inheriting from
        /// IClient, to its designated methods
        /// </summary>
        /// <param name="Class">An instance of a class
        /// using the 'IServer' interface</param>
        public void AssignEvents(IClient client)
        {
            Connected += client.OnSuccessfulConnection;
            ConnectionFailed += client.OnFailedConnection;
            Disconnected += client.OnDisconnected;
            DataReceived += client.OnDataReceived;
        }

        /// <summary>
        /// Connects asynchronously to an open server
        /// </summary>
        /// <param name="IP">The server IP</param>
        /// <param name="Port">The server Port</param>
        public async void ConnectTo(string IP, int Port)
        {
            var IpAddress = new IPEndPoint(IPAddress.Parse(IP), Port);
            client = new TcpClient();

            try
            {
                await Task.Run(() =>
                {
                    client.Connect(IpAddress);
                });

                if (client.Connected)
                {
                    SR = new StreamReader(client.GetStream());
                    SW = new StreamWriter(client.GetStream());

                    Connected?.Invoke();

                    ReceiveData();
                }
            }
            catch (Exception)
            {
                ConnectionFailed?.Invoke();
            }
        }

        async void ReceiveData()
        {
            while (client.Connected)
            {
                string dataReceived = "";

                await Task.Run(() =>
                {
                    try { dataReceived = SR.ReadLine(); }

                    // Lost connection
                    catch (Exception) { Disconnect(); }
                });

                try { DataReceived?.Invoke(dataReceived); }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Sends a serialized object using a stream
        /// to all clients
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="packet">The object</param>
        /// <param name="clientID">The ID of the client to send to.
        /// (0 being the host, and 1 being the next client etc).
        /// If null, it will send to all clients</param>
        public void SendData<T>(Packet<T> packet, int clientID = -1)
        {
            if (client.Connected)
            {
                SW.WriteLine(clientID + "|" + packet.Data);
                SW.Flush();
            }
        }

        public void Disconnect()
        {
            if (client != null && client.Connected)
            {
                client.Close();
            }

            Disconnected?.Invoke();
        }
    }
}
