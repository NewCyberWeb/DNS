using DNSLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNS_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            DNSServer s = new DNSServer();
            s.VerboseLog += VerboseLog;
            s.ErrorLog += ErrorLog;
            s.Start();
            for (string command; (command = CommandlineHelper.ParseCommand()) != CommandlineHelper.COMMAND_EXIT;)
                CommandlineHelper.Execute(s, command);
            s.Stop();
        }

        private static void ErrorLog(string message)
        {
            BConsole.WriteLine(message, ConsoleColor.Red);
        }

        private static void VerboseLog(string message)
        {
            BConsole.WriteLine(message, ConsoleColor.White);
        }
    }
}
