using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientApplication;

class Client
{
    public Client()
    {
        
        string hostName = Dns.GetHostName();
        string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
        TcpClient client = new TcpClient(IPAddress.Parse(myIP).ToString(), 3370);
        bool done = false;
        Console.WriteLine("Type 'Bye' to end connection");
        while (!done)
        {
            //Console.WriteLine("Enter a message to the server: ");
            string message = Console.ReadLine();

            WriteMessage(client, message);

            string response = ReadTextMessage(client);
            Console.WriteLine($"Response: {response}");
            done = response.Equals("Bye");
        }
    }

    private static string ReadTextMessage(TcpClient client)
    {
        var stream = new StreamReader(client.GetStream(), Encoding.ASCII);
        {
            return stream.ReadLine();
        }
    }

    private static void WriteMessage(TcpClient client, string message)
    {
        var stream = new StreamWriter(client.GetStream(), Encoding.ASCII);
        {
            stream.WriteLine(message);
            stream.Flush();
        }
    }
}