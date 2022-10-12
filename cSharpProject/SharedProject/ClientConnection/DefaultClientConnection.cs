using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using Shared.Log;

namespace Shared;

public class DefaultClientConnection
{
    #region ClientConnection
    private TcpClient client;
    private NetworkStream stream;
    public readonly Dictionary<string, Action<JObject>> SerialCallbacks = new();
    private Action<JObject, bool> commandHandlerMethod;
    
    public RSA Rsa = new RSACryptoServiceProvider();
    #endregion
    
    

    public DefaultClientConnection(string hostname, int port, Action<JObject, bool> commandHandlerMethod)
    {
        Init(hostname, port, commandHandlerMethod);
        
    }
    public DefaultClientConnection()
    {
        
    }
    
    public void Init(string hostname, int port, Action<JObject, bool> commandHandlerMethod, bool setup = true)
    {
        OnMessage += (_, json) => HandleMessage(json);

        this.commandHandlerMethod = commandHandlerMethod;
        
        client = new(hostname, port);
        stream = client.GetStream();
        stream.BeginRead(_buffer, 0, 1024, OnRead, null);
        if (setup)
        {
            SetupClient();
        }
    }

   
    public void SetupClient()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        AddSerialCallback(serial, ob =>
        {
            PublicKey = ob["data"]!.Value<JArray>("key")!.Values<byte>().ToArray();
            Logger.LogMessage(LogImportance.Information, 
                $"Received PublicKey from Server: {LogColor.Gray}\n{Util.ByteArrayToString(PublicKey)}");
        });
        
        SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolderShared.Json.Path));
    }

    #region Sending and retrieving data
    private byte[] _totalBuffer = Array.Empty<byte>();
    private readonly byte[] _buffer = new byte[1024];
    public event EventHandler<JObject> OnMessage;
    
    
   
    private void OnRead(IAsyncResult readResult)
    {
        try
        {
            var numberOfBytes = stream.EndRead(readResult);
            _totalBuffer = Concat(_totalBuffer, _buffer, numberOfBytes);
        }
        catch
        {
            return;
        }

        while (_totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(_totalBuffer, 0);

            if (_totalBuffer.Length >= packetSize + 4) 
            {
                var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);

                OnMessage?.Invoke(this, JObject.Parse(json));

                var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                _totalBuffer = newBuffer;
            }

            else
                break;
        }

        stream.BeginRead(_buffer, 0, 1024, OnRead, null);
    }
    
    public void SendData(string message)
    {
        try
        {
            var ob = JObject.Parse(message);
            if (ob.ContainsKey("serial"))
            {
                if (ob["serial"]!.ToObject<string>()!.Equals("_serial_"))
                {
                    ob.Remove("serial");
                    message = ob.ToString();
                }
            }

            if (ob["data"]?["error"]?.ToObject<string>() != null)
            {
                if (ob["data"]!["error"]!.ToObject<string>()!.Equals("_error_"))
                {
                    ob["data"]!["error"]!.Remove();
                    message = ob.ToString();
                }
            }

            if (!ob["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
            {
                Logger.LogMessage(LogImportance.Information, 
                    $"Sending message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
            }
        }
        catch(JsonReaderException)
        {
           
        }
                
        Byte[] data = BitConverter.GetBytes(message.Length);
        Byte[] comman = System.Text.Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
        stream.Write(comman, 0, comman.Length);
    }
    
  
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
    
   
    #endregion

   
    {
        stream.Close();
        client.Close();
    }
}