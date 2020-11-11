using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNS_Server
{
	public class BConsole
	{
		private static object _MessageLock = new object();

		/// <summary>
		/// Overload of the writeline function that displays current time in front of the text to write
		/// </summary>
		/// <param name="line"></param>
		/// <param name="color"></param>
		public static void WriteLine(string line, ConsoleColor color = ConsoleColor.White)
		{
			lock (_MessageLock)
			{
				string timeFormat = $"[{DateTime.Now:HH:mm:ss}] ";
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(timeFormat);
				Console.ForegroundColor = color;
				Console.WriteLine(line);
				Console.ResetColor();
			}
		}
	}
}
