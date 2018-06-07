using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace duplicatus
{
    public class FileInformation
    {
        public string Filename { get; set; }
        public long Length { get; set; }
        public string MD5Hash { get; set; }
        public string AbsolutePath { get; set; }

        public static FileInformation FromFile(FileInfo info)
        {
            var retFileInformation = new FileInformation
            {
                Filename = info.Name,
                Length = info.Length,
                AbsolutePath = info.FullName,
            };
            using (var fs = info.OpenRead())
            {
                using (MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider())
                {
                    var byteHash = MD5.ComputeHash(fs);
                    retFileInformation.MD5Hash = BitConverter.ToString(byteHash).Replace("-", "");
                }
            }
            return retFileInformation;
        }

        public static void SerializeToDisk(string filename, IEnumerable<FileInformation> contents)
        {
            using (var sw = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                var orderedContent = contents.OrderByDescending(zz => zz.Length).ThenBy(zz => zz.MD5Hash).ToList();
                sw.WriteLine(JsonConvert.SerializeObject(orderedContent, Formatting.Indented));
            }
        }

        public static List<FileInformation> ReadFromDisk(string filename)
        {
            using (var sr = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                List<FileInformation> retList = JsonConvert.DeserializeObject<List<FileInformation>>(sr.ReadToEnd());
                return retList;
            }
        }
    }
}
