using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    class Server
    {
        public static Client client;
        TcpListener server;
        private Queue<Message> myQ;
        private Dictionary<int, Client> dictionary;
        int UserIDNumber;
        public Server()
        {
            server = new TcpListener(IPAddress.Parse("192.168.0.131"), 9999);
            myQ = new Queue<Message>();
            dictionary = new Dictionary<int, Client>();
            UserIDNumber = 0;
            server.Start();
        }
        public void Run()
        {
            Task.Run(() => AcceptClient());
            
          
        }
        private void AcceptClient()
        {
            TcpClient clientSocket = default(TcpClient);
            clientSocket = server.AcceptTcpClient();
            Console.WriteLine("Connected");
            NetworkStream stream = clientSocket.GetStream();
            client = new Client(stream, clientSocket);
            AddClientToDictionary(client);
            NotifyClientOfNewClient(client);
            string message = client.Recieve();
            AddToQueue(message);
            Task.Run(() => Broadcast(message));


        }
        private void Broadcast(string message)
        {
            client.Send(message);
        }
        private void Respond(string body)
        {
             client.Send(body);
            //queue should be type message
        }
        private void AddToQueue(string message)
        {
            Message currentMessage = new Message(client, message);
            myQ.Enqueue(currentMessage);
        }

        private void RemoveFromQueue(string message)
        {
            myQ.Dequeue();
        }
        private void AddClientToDictionary(Client client)
        {
            dictionary.Add(UserIDNumber, client);
            UserIDNumber++;
        }
        private void NotifyClientOfNewClient(Client client)
        {
            foreach(KeyValuePair<int, Client> clients in dictionary)
            {
                string words = "blank added to the chatroom";
                client.Send(words);
            }
        }
    }
}
