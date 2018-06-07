using System;
using System.Collections.Generic;

namespace duplicatus
{
	public class ByteArrayComparer : IEqualityComparer<byte[]>
	{
		public bool Equals(byte[] x, byte[] y)
		{
			if (x == null || y == null) return false;
			if (x.Length != y.Length) return false;
			for(int ii=0; ii < x.Length; ii++)
			{
				var compareIndex = x[ii].CompareTo(y[ii]);
				if(compareIndex != 0) return false;
			}
			return true;
		}

		public int GetHashCode(byte[] obj)
		{
			return BitConverter.ToString(obj).Replace("-", "").GetHashCode();
		}
	}
}