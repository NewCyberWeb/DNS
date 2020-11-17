using DNSLib.Server;
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

        public static void Execute(DNSServer s, string command)
        {
            if (command == null) return;
            switch (command.Split(' ')[0].ToLower())
            {
                case COMMAND_HELP:
                    string helpText = @"Retrieving help information
There are 4 commands:

- Exit
- DNSShow          Gets a list of records                                       usage: dnsshow -[domain|ip] [searchtext]
- DNSNew           Creates a new DNS entry                                      usage: dnsnew [domain] [ip]
- DNSEdit          Edits the DNS entry, the flag decides what to search for.    usage: dnsedit -[domain|ip] [current domain] [new ip/domain]
- DNSRemove        Removes the DNS entry, AND all of it's Sub Domains.          usage: dnsremove [domain]

Commands that have a - in the usage clause interpret these as flags.";
                    BConsole.WriteLine(helpText, ConsoleColor.Yellow);
                    break;
                case COMMAND_NEW_ENTRY:
                    AddNewEntry(s, command);
                    break;
                case COMMAND_EDIT_ENTRY:
                    EditEntry(s, command);
                    break;
                case COMMAND_DELETE_ENTRY:
                    DeleteEntry(s, command);
                    break;
                case COMMAND_SHOW_ENTRY:
                    ShowEntry(s, command);
                    break;
            }
        }

        private static void AddNewEntry(DNSServer s, string command)
        {
            string[] commandList = command.Split(' ');
            if (commandList.Length != 3) return;

            string domain = commandList[1];
            string ip = commandList[2];

            //add the new entry
            BConsole.WriteLine(s.Processor.AddDNSEntry(domain, ip), ConsoleColor.Yellow);
        }

        private static void EditEntry(DNSServer s, string command)
        {
            string[] commandList = command.Split(' ');
            if (commandList.Length != 4) return;

            string flag = commandList[1];
            string currentDomain = commandList[2];
            string newValue = commandList[3];
            switch (flag)
            {
                case "-domain":
                    BConsole.WriteLine(s.Processor.EditDNSEntry(true, currentDomain, newValue), ConsoleColor.Yellow);
                    break;
                case "-ip":
                    BConsole.WriteLine(s.Processor.EditDNSEntry(false, currentDomain, newValue), ConsoleColor.Yellow);
                    break;
            }
        }

        private static void DeleteEntry(DNSServer s, string command)
        {
            string[] commandList = command.Split(' ');
            if (commandList.Length != 2) return;

            string domain = commandList[1];

            //add the new entry
            BConsole.WriteLine(s.Processor.DeleteDNSEntry(domain), ConsoleColor.Yellow);
        }

        private static void ShowEntry(DNSServer s, string command)
        {
            string[] commandList = command.Split(' ');
            if (commandList.Length != 3) return;

            string flag = commandList[1];
            string value = commandList[2];

            switch (flag)
            {
                case "-domain":
                    BConsole.WriteLine(s.Processor.ShowDNSEntry(true, value), ConsoleColor.Yellow);
                    break;
                case "-ip":
                    BConsole.WriteLine(s.Processor.ShowDNSEntry(false, value), ConsoleColor.Yellow);
                    break;
            }
        }
    }
}
