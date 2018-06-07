using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks; 

namespace duplicatus
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Requires two args.  [PathToCheck] [FileToSave]");
                return -1;
            }
            var pathToCheck=args[0];

            Console.WriteLine($"Checking path: '{args[0]}' and saving catalog to '{args[1]}'.  Press Y to continue.");
            if(Console.ReadKey().KeyChar != 'y')
            {
                return -1;
            }
            Console.WriteLine("\nStarting catalog...");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var co = new CatalogOperation(args[0]);
            var files=co.Run().ToList();
            Console.WriteLine("Time to index: {0} files.  {1}ms", files.Count, sw.ElapsedMilliseconds);
            var duplicates = files.GroupBy(zz=>zz.MD5Hash, new ByteArrayComparer()).Where(zz=>zz.Count() > 1).ToList();
            Console.WriteLine($"{duplicates.Count()} duplicates.");
            foreach(var f in duplicates)
            {
                var md5String = BitConverter.ToString(f.First().MD5Hash).Replace("-", "");
                Console.WriteLine($"{md5String} - {f.Count()} matching files.");
                Console.WriteLine("\t{0}", string.Join("\r\n\t", f.Select(zz=>zz.AbsolutePath)));
            }
            return 0;
        }
    }
}
