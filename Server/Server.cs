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
        private Dictionary<int, TcpClient> dictionary;
        int UserIDNumber;
        private Object messageLock = new Object();
        public Server()
        {
            server = new TcpListener(IPAddress.Parse("192.168.0.131"), 9999);
            myQ = new Queue<Message>();
            dictionary = new Dictionary<int, TcpClient>();
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
            Task<string> userName = Task.Run(() => client.GetUserName());
            Task.WaitAll(userName);
            AddClientToDictionary(clientSocket);
            NotifyClientOfNewClient(clientSocket);
            while (true)
            {
                Task<string> message = Task.Run(() => client.Recieve());
                Task<string>[] messages = new Task<string>[] { message };
                string currentMessage = messages[0].Result;
                AddToQueue(currentMessage);
                Task.Run(() => Broadcast(currentMessage));
            }


        }
        private void Broadcast(string message)
        {
            lock (messageLock)
            {
                client.Send(message);
            }
        }
        private void Respond(string body)
        {
             client.Send(body);
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
        private void AddClientToDictionary(TcpClient clientSocket)
        {
            dictionary.Add(UserIDNumber, clientSocket);
            UserIDNumber++;
        }
        private void NotifyClientOfNewClient(TcpClient clientSocket)
        {
            foreach(KeyValuePair<int, TcpClient> clients in dictionary)
            {
                string words = "{0} added to the chatroom";
                client.Send(words);
            }
        }
    }
}
