using System.IO;
using System.Security.Cryptography;

namespace duplicatus
{
	public class FileInformation
	{
		public string Filename { get; set; }
		public long Length{get; set;}
		public byte[] MD5Hash { get; set;}
		public string AbsolutePath {get; set;}

		private static MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
		public static FileInformation FromFile(FileInfo info)
		{
			var retFileInformation = new FileInformation
			{
				Filename=info.Name,
				Length=info.Length,
				AbsolutePath=info.FullName,
			};
			using(var fs= info.OpenRead())
			{
				retFileInformation.MD5Hash = MD5.ComputeHash(fs);
			}
			return retFileInformation;
		}
	}
}