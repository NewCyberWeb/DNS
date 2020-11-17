using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    internal static class Extensions
    {
		/// <summary>
		/// Extension method that can devide Arrays the same way the SubString Method would.
		/// </summary>
		/// <typeparam name="Tarray">The array of any type</typeparam>
		/// <param name="data">The array to perform the action on</param>
		/// <param name="index">Where to start</param>
		/// <param name="length">how much items to retrieve</param>
		/// <returns>The new shorter Array</returns>
		public static Tarray[] SubArray<Tarray>(this Tarray[] data, int index, int length)
		{
			Tarray[] result = new Tarray[length];
			if (length > data.Length - index)
			{
				Array.Copy(data, index, result, 0, data.Length - index);
			}
			else
			{
				Array.Copy(data, index, result, 0, length);
			}
			return result;
		}

		/// <summary>
		/// Checks if the given string is a valid ip address by checking the segments
		/// </summary>
		/// <param name="ip">ip address in string form</param>
		/// <returns></returns>
		public static bool IsValidIpAddress(this string ip)
        {
			string[] ipsegments = ip.Split('.');
			if (ipsegments.Length != 4) return false;

			foreach(string segment in ipsegments)
            {
				if (int.Parse(segment) < 0 || int.Parse(segment) > 255) return false;
			}

			return true;
        }
	}
}
