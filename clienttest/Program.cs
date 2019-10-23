using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


namespace clienttest
{
    class Program
    {

        static void Connect(String server, String message)
        {
            {
                while (true)
                {


                    var client = new TcpClient();
                    client.Connect(IPAddress.Loopback, 5000);

                    var stream = client.GetStream();


                    Console.WriteLine("Send message:");
                    var msg = Console.ReadLine();

                    Console.WriteLine($"Message: {msg}");
                    var buffer = Encoding.UTF8.GetBytes(msg);

                    stream.Write(buffer, 0, buffer.Length);

                    if (msg == "exit") break;

                    buffer = new byte[client.ReceiveBufferSize];
                    var rcnt = stream.Read(buffer, 0, buffer.Length);

                    msg = Encoding.UTF8.GetString(buffer, 0, rcnt);

                    Console.WriteLine($"Message: {msg}");

                    stream.Close();
                }
            }
        }
    



        static void Main(string[] args)
        {
            Connect("127.0.0.1", "Hello from client ");
            
        }
    }
}
