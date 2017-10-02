using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Server
{
    public class Client
    {
        NetworkStream stream;
        TcpClient client;
        public int UserId;
        public string userName;
        public Client(NetworkStream Stream, TcpClient Client)
        {
            stream = Stream;
            client = Client;
            UserId = 1;
        }
        public bool IsConnected
        {
            get { return client.Connected; }
        }

        public void Send(string Message)
        {
            byte[] message = Encoding.ASCII.GetBytes(Message);
            stream.Write(message, 0, message.Count());
        }
        public string Recieve()
        {
            try
            {
                byte[] recievedMessage = new byte[256];
                stream.Read(recievedMessage, 0, recievedMessage.Length);
                string recievedMessageString = Encoding.ASCII.GetString(recievedMessage);
                Console.WriteLine(recievedMessageString);
                return recievedMessageString;
            }
            catch(Exception e)
            {
                string message = "The server has crashed." + e;
                return message;
                    
            }
        }

        public string GetUserName()
        {
           Send("What is your username?");
           userName = Recieve();
           return userName;
        }
    }
}
