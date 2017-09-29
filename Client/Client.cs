using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Client
{
    class Client
    {
        TcpClient clientSocket;
        NetworkStream stream;
        public Client(string IP, int port)
        {
            clientSocket = new TcpClient();
            clientSocket.Connect(IPAddress.Parse(IP), port);
            stream = clientSocket.GetStream();
        }
        public void Start()
        {
            Task.Run(() => Send());
            Task value = Task.Run(() => Recieve());
            value.Wait();
        }
        public void Send()
        {
            while (true)
            {
                try
                {
                    string messageString = UI.GetInput();
                    byte[] message = Encoding.ASCII.GetBytes(messageString);
                    stream.Write(message, 0, message.Count());
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                    break;
                    // log each e.Message in log
                }
            }
        }
        public void Recieve()
        {
            while (true)
            {
                try
                {
                    byte[] recievedMessage = new byte[256];
                    stream.Read(recievedMessage, 0, recievedMessage.Length);
                    UI.DisplayMessage(Encoding.ASCII.GetString(recievedMessage));
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                    break;
                    // you will want to log each e.Message in log
                }
            }
        }
    }
}
