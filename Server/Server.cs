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
        TcpListener server;
        private Queue<Message> myQ;
        private Dictionary<int, Client> dictionary;
        int UserIDNumber;
        private Object messageLock = new Object();
        public Server()
        {
            server = new TcpListener(IPAddress.Any, 9999);
            myQ = new Queue<Message>();
            dictionary = new Dictionary<int, Client>();
            UserIDNumber = 0;
            server.Start();
        }
        public void Run()
        {
            Task.Run(() => AcceptClient());
            Task.Run(() => Broadcast());
        }
        private void AcceptClient()
        {
            while (true)
            {
                TcpClient clientSocket = default(TcpClient);
                clientSocket = server.AcceptTcpClient();
                Console.WriteLine("Connected");
                NetworkStream stream = clientSocket.GetStream();
                Client client = new Client(stream, clientSocket);
                AddClientToDictionary(client);
                Task username = Task.Run(() => GetInformationForNotification(client));
                username.Wait();
                Task.Run(() => CreateNewClientChat(clientSocket, client));
            }
        }
        private void GetInformationForNotification(Client client)
        {
            Task<string> userName = Task.Run(() => client.GetUserName());
            userName.Wait();
            string name = userName.Result.Trim('\0');
            NotifyClientOfNewClient(name, client);
        }

        private void CreateNewClientChat(TcpClient clientSocket, Client client)
        {
            while (true)
            {
                try
                {
                    Task<string> message = Task.Run(() => client.Recieve());
                    Task<string>[] messages = new Task<string>[] { message };
                    string currentMessage = messages[0].Result;
                    AddToQueue(currentMessage, client);
                }
                catch (Exception e)
                {

                }
            }
        }
        private void Broadcast()
        {
            while (true)
            {
                if (myQ.Count() > 0)
                {
                    string message = RemoveFromQueue();
                    lock (messageLock)
                    {
                        foreach (KeyValuePair<int, Client> clients in dictionary)
                        {
                            clients.Value.Send(message);
                        }
                    }
                }
            }
        }
        private void Respond(string body, Client client)
        {
             client.Send(body);
        }
        private void AddToQueue(string message, Client client)
        {
            Message currentMessage = new Message(client, message);
            myQ.Enqueue(currentMessage);
        }

        private string RemoveFromQueue()
        {
            return myQ.Dequeue().Body;
        }
        private void AddClientToDictionary(Client client)
        {
            dictionary.Add(UserIDNumber, client);
            UserIDNumber++;
        }
        private void NotifyClientOfNewClient(string userName, Client client)
        {
             string words = userName + " added to the chatroom.";
             AddToQueue(words, client);
        }
    }
}
