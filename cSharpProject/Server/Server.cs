using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Server
    {
        private static Socket? serverSocket;
        private static List<Server> serverlist;

        private int _port;
        
        struct Client
        {
            public Socket socket { get; }
            public int Id { get; }
            public Client(Socket socket, int id)
            {
                this.socket = socket;
                this.Id = id;
            }
        }

        public Server(int port)
        {
            _port = port;

            serverSocket
        }
    }
}
