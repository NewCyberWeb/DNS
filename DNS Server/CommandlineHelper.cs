using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNS_Server
{
    internal sealed class CommandlineHelper
    {
        public const string COMMAND_EXIT = "exit";
        public const string COMMAND_HELP = "help";
        public const string COMMAND_NEW_ENTRY = "dnsnew";
        public const string COMMAND_EDIT_ENTRY = "dnsedit";
        public const string COMMAND_DELETE_ENTRY = "dnsremove";
        public const string COMMAND_SHOW_ENTRY = "dnsshow";


        public static string ParseCommand()
        {
            Console.Write("Enter command: ");
            string command = Console.ReadLine();
            switch (command.Split(' ')[0].ToLower())
            {
                case COMMAND_EXIT:
                    return COMMAND_EXIT;
                case COMMAND_HELP:
                case COMMAND_NEW_ENTRY:
                case COMMAND_EDIT_ENTRY:
                case COMMAND_DELETE_ENTRY:
                case COMMAND_SHOW_ENTRY:
                    return command;
                default:
                    return null;
            }
        }

        public static void Execute(string command)
        {
            if (command == null) return;
            switch (command.Split(' ')[0].ToLower())
            {
                case COMMAND_HELP:
                    string helpText = @"Retrieving help information
There are 4 commands:

- Exit
- DNSShow          usage: dnsshow -[domain,ip] [text]
- DNSNew           usage: dnsnew [domain] [ip]
- DNSEdit          usage: dnsedit -[domain,ip] [domain] [ip]
- DNSRemove        usage: dnsremove [domain] [ip]

Commands that have a - in the usage interpret these as flags.";
                    BConsole.WriteLine(helpText, ConsoleColor.Yellow);
                    break;
                case COMMAND_NEW_ENTRY:
                case COMMAND_EDIT_ENTRY:
                case COMMAND_DELETE_ENTRY:
                case COMMAND_SHOW_ENTRY:
                    break;
            }
        }
    }
}
