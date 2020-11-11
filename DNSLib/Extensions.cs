using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    internal static class Extensions
    {
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
	}
}
