using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.TCP
{
    public interface IServer
    {
        void OnServerStarted();

        void OnServerClosed();

        void OnClientConnected();

        void OnServerFilled();
    }

    public interface IClient
    {
        void OnSuccessfulConnection();

        void OnFailedConnection();

        void OnDataReceived(string data);

        void OnDisconnected();
    }
}
