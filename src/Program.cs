using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace duplicatus
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Requires two args.  [PathToCheck] [FileToSave]");
                return -1;
            }
            var pathToCheck = args[0];

            Console.WriteLine($"Checking path: '{args[0]}' and saving catalog to '{args[1]}'.");
            Console.WriteLine("\nStarting catalog...");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int filesProcessed = 0;
            string lastFile = null;
            bool running = true;
            var progressTask = Task.Run(() =>
            {
                do
                {
                    if (!running) break;
                    Console.Clear();
                    Console.WriteLine($"Files processed: {filesProcessed}");
                    Console.WriteLine($"Current file: {lastFile}");
                    System.Threading.Thread.Sleep(500);
                } while (running);
            });
            Action<FileInformation> trackProgress = (info) =>
            {
                System.Threading.Interlocked.Increment(ref filesProcessed);
                lastFile = info.AbsolutePath;
            };
            var co = new CatalogOperation(args[0], trackProgress);
            var files = co.Run().ToList();
            running = false;
            var timeToIndex = sw.ElapsedMilliseconds;
            await progressTask;
            Console.WriteLine("Time to index: {0} files.  {1}ms", files.Count, timeToIndex);
            var duplicates = files.GroupBy(zz => zz.MD5Hash).Where(zz => zz.Count() > 1).ToList();
            Console.WriteLine($"{duplicates.Count()} sets of duplicates.");
            foreach (var f in duplicates)
            {
                var md5String = f.First().MD5Hash;
                Console.WriteLine($"{md5String} - {f.Count()} matching files.");
                Console.WriteLine("\t{0}", string.Join("\r\n\t", f.Select(zz => zz.AbsolutePath)));
            }
            FileInformation.SerializeToDisk(args[1], files);
            return 0;
        }
    }
}
