using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TextLogger: ILog
    {
        private Object logLock = new object();
        string path = "log.txt";
        public void LogMessage(string message)
        {
            lock (logLock)
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(message + " was recieved.");
                }
            }
        }

        public void LogPerson(string userName)
        {
            lock (logLock)
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(userName + " added to the server.");
                }
            }
        }

        public void LogError(Exception e)
        {
            lock (logLock)
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(e + " was thrown.");
                }
            }
        }
        public void LogPersonLeft(Client client)
        {
            lock (logLock)
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(client.userName + " has left the chatroom.");
                }
            }
        }

        public void ServerClosed()
        {
            lock (logLock)
            {
                using(StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("The server has closed");
                }
            }
        }
    }
}
