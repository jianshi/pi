using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        private const string SERVER_IP = @"192.168.0.7";
        private const int PORT_NO = 5556; 
        static void Main(string[] args)
        {
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine("Krenuo sa radom...");
            listener.Start();

            while (true)
            {

                TcpClient client = listener.AcceptTcpClient();


                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                bool bTerminateConnection = false;

                do
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);


                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    dataReceived = dataReceived.Trim();
                    Console.WriteLine("Primljeno : " + dataReceived);


                    Console.WriteLine("Dobijena poruka na serveru : " + dataReceived);
                    nwStream.Write(buffer, 0, bytesRead);

                    Console.WriteLine("\n");

                    if (string.Compare(dataReceived, "END", true) == 0)
                    {
                        bTerminateConnection = true;
                    }
                } while (!bTerminateConnection);
                client.Close();
            }
            listener.Stop();
        }
    }
}
