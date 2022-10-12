using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerApplication;

public class Server
{
    public Server(int port = 3370)
    {
        string hostName = Dns.GetHostName();
        string MyIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
        IPAddress localhost = IPAddress.Parse(MyIP);
        // TcpListener listener = new TcpListener(IPAddress.Any, port);
        TcpListener listener = new TcpListener(localhost, port);
        listener.Start();

        while (true)
        {
            Console.WriteLine("waiting for connection");
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine(client.Connected);
            Thread thread = new Thread(HandleClientThread);
            thread.Start(client);
            Thread.Sleep(100);
        }
    }

    private void HandleClientThread(object? obj)
    {
        Console.WriteLine("Connected");
        TcpClient client = obj as TcpClient;
    
        bool done = false;
        while (!done)
        {
            string received = ReadTextMessage(client);
            Console.WriteLine($"Received: {received}");
    
            done = received.Equals("Bye");
            if (done)
            {
                WriteTextMessage(client, "Bye");
            }
            else
            {
                string response = Console.ReadLine();
                WriteTextMessage(client, response);
            }
        }
        client.Close();
        Console.WriteLine("connection closed");
    }
    
    private static void WriteTextMessage(TcpClient client, string v)
    {
        var stream = new StreamWriter(client.GetStream(), Encoding.ASCII);
        {
            stream.WriteLine(v);
            stream.Flush();
        }
    }
    
    private static string ReadTextMessage(TcpClient client)
    {
        var stream = new StreamReader(client.GetStream(), Encoding.ASCII);
        {
                    
            return stream.ReadLine();
               
        }
    }
}