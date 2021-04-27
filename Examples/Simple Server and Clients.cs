using System;
using Networking.TCP;

namespace LANTest
{
    class Program
    {
        public static Client myClient;

        static void Main()
        {
            // Start server at port 1234
            var server = new Server(1234);
            server.AssignEvents(new ServerMethods());
            server.Start();

            // Connect client to server ip and port (essentially a host)
            myClient = new Client();
            myClient.AssignEvents(new ClientMethods());
            myClient.ConnectTo(server.IP, server.Port);

            // Connect another client to server ip and port (may be from another device)
            var otherClient = new Client();
            otherClient.AssignEvents(new ClientMethods());
            otherClient.ConnectTo(server.IP, server.Port);

            // Once client is connected, it will send a
            // data packet, which is read by all connected
            // clients and output in the console

            Console.ReadKey();
        }
    }

    class ServerMethods : IServer
    {
        public void OnClientConnected()
        {
            Console.WriteLine("Client connected");
        }

        public void OnServerClosed()
        {
            Console.WriteLine("Server closed");
        }

        public void OnServerFilled()
        {
            Console.WriteLine("Server filled");
        }

        public void OnServerStarted()
        {
            Console.WriteLine("Server started");
        }
    }

    class ClientMethods : IClient
    {
        public void OnDataReceived(string data)
        {
            // Deserializes the received data string back to its original form
            var myObj = Packet.GetObject<MyObject>(data);

            Console.WriteLine(myObj.Name + ": " + myObj.Text);
        }

        public void OnDisconnected()
        {
            Console.WriteLine("Disconnected");
        }

        public void OnFailedConnection()
        {
            Console.WriteLine("Failed to connect");
        }

        public void OnSuccessfulConnection()
        {
            Console.WriteLine("Connected");
            SendMyObject();
        }

        void SendMyObject()
        {
            MyObject obj = new MyObject("MyName", "Hello universe");

            // Serializes the object and prepares it for data transfer
            var packet = new Packet<MyObject>(obj);

            // Sends the object to all clients
            Program.myClient.SendData(packet);
        }
    }

    [Serializable]
    struct MyObject
    {
        public string Name;
        public string Text;

        public MyObject(string name, string text)
        {
            Name = name;
            Text = text;
        }
    }
}