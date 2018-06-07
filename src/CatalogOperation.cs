using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace duplicatus
{
	public class CatalogOperation
	{
		private string _basePath;
		public CatalogOperation(string basePath)
		{
			_basePath=basePath;
		}

		public IEnumerable<FileInformation> Run()
		{
			var directory = new DirectoryInfo(_basePath);
			foreach(var file in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
			{
				yield return FileInformation.FromFile(file);
			}
		}
	}
}