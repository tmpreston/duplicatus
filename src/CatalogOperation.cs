using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace duplicatus
{
    public class CatalogOperation
    {
        private string _basePath;
        private Action<FileInformation> _operation;

        public CatalogOperation(string basePath, Action<FileInformation> operation = null)
        {
            _basePath = basePath;
            if (operation == null)
            {
                operation = (file) => { };
            }
            _operation = operation;
        }

        public FileInformation[] Run()
        {
            var directory = new DirectoryInfo(_basePath);
            var fileInfos = from file in directory.EnumerateFiles("*.*", SearchOption.AllDirectories)
                            select file;
            var convertedInfos = from info in fileInfos.AsParallel()
                                 select FileInformation.FromFile(info);
            foreach (var fileInformation in convertedInfos)
            {
                _operation(fileInformation);
            }
            return convertedInfos.ToArray();
        }
    }
}