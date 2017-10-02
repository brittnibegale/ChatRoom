using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface ILog
    {
        void LogPersonLeft(Client client);

        void LogMessage(string message);

        void LogPerson(string userName);

        void LogError(Exception e);

        void ServerClosed();
    }
}
