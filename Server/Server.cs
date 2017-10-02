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
        private Queue<Message> messages;
        private Dictionary<int, Client> people;
        int UserIDNumber;
        private Object messageLock = new Object();
        ILog logger;
        public Server(ILog logger)
        {
            server = new TcpListener(IPAddress.Any, 9999);
            messages = new Queue<Message>();
            people = new Dictionary<int, Client>();
            UserIDNumber = 0;
            this.logger = logger;
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
                try
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
                catch(Exception e)
                {
                    logger.ServerClosed();
                }
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
                    message.Wait();
                    Task<string>[] messages = new Task<string>[] { message };
                    string currentMessage = messages[0].Result;
                    AddToQueue(currentMessage, client);
                }
                catch (Exception e)
                {
                    string error = "You have left the chatroom. " + e;
                    Console.Write(error);
                    logger.LogError(e);
                }

            }
        }
        private void Broadcast()
        {
            while (true)
            {
                if (messages.Count() > 0)
                {
                    Message message = RemoveFromQueue();
                    lock (messageLock)
                    {
                        Client removedPerson = null;
                        foreach (KeyValuePair<int, Client> clients in people)
                        {
                            if (message.sender.IsConnected == true)
                            {
                                if (!(message.sender.userName == clients.Value.userName))
                                { 
                                    clients.Value.Send(message.Body);
                                }
                            }
                            else
                            {
                                logger.LogPersonLeft(message.sender);
                                removedPerson = message.sender;
                            }
                        }
                      RemoveClientFromDictionary(removedPerson);
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
            messages.Enqueue(currentMessage);
            logger.LogMessage(message);
        }
        private Message RemoveFromQueue()
        {
            return messages.Dequeue();
        }
        private void AddClientToDictionary(Client client)
        {
            people.Add(UserIDNumber, client);
            client.UserId = UserIDNumber;
            UserIDNumber++;
        }
        private void RemoveClientFromDictionary(Client client)
        {
            if (!(client == null))
            {
                people.Remove(client.UserId);
                CheckForPeople(client);
            }
        }
        private void CheckForPeople(Client client)
        {
            if(people.Count <= 0)
            {
                logger.ServerClosed();
                Environment.Exit(0);
            }
        }
        private void NotifyClientOfNewClient(string userName, Client client)
        {
            string words = userName + " added to the chatroom.";
            client.userName = userName;
            AddToQueue(words, client);
            logger.LogPerson(userName);
        }
    }
}
