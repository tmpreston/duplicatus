using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace duplicatus
{
    class Program
    {
        class Options
        {
            // Omitting long name, defaults to name of property, ie "--verbose"
            [Option(Default = false, HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }

        [Verb("catalog", HelpText = "Enumerate all files in path and create a catalog.json file")]
        class CatalogOptions
        {
             [Value(0, MetaName = "Search path",  Required = true, HelpText = "Path to catalog.")]
            public string InputPath { get; set; }

            [Value(1, MetaName = "Catalog filename",  Required = true, HelpText = "Filename to save.")]
            public string CatalogFilename { get; set;}
        }

        static int Main(string[] args)
        {
            var retValue = CommandLine.Parser.Default.ParseArguments<Options, CatalogOptions>(args)
            .MapResult<Options, CatalogOptions, int>(
                (Options opts) => RunOptionsAndReturnExitCode(opts),
                (CatalogOptions opts) => RunOptionsAndReturnExitCode(opts),
                errs  => HandleParseError(errs)
            );
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine($"Result: {retValue}");
                Console.ReadLine();
            }

            return retValue;
        }

        private static int RunOptionsAndReturnExitCode(Options opts)
        {
            return 0;
        }

        private static int RunOptionsAndReturnExitCode(CatalogOptions opts)
        {
            var pathToCheck = opts.InputPath;

            Console.WriteLine($"Checking path: '{opts.InputPath}' and saving catalog to '{opts.CatalogFilename}'.");
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
            var co = new CatalogOperation(opts.InputPath, trackProgress);
            var files = co.Run().ToList();
            running = false;
            var timeToIndex = sw.ElapsedMilliseconds;
            progressTask.GetAwaiter().GetResult();
            Console.WriteLine("Time to index: {0} files.  {1}ms", files.Count, timeToIndex);
            var duplicates = files.GroupBy(zz => zz.MD5Hash).Where(zz => zz.Count() > 1).ToList();
            Console.WriteLine($"{duplicates.Count()} sets of duplicates.");
            foreach (var f in duplicates)
            {
                var md5String = f.First().MD5Hash;
                Console.WriteLine($"{md5String} - {f.Count()} matching files.");
                Console.WriteLine("\t{0}", string.Join("\r\n\t", f.Select(zz => zz.AbsolutePath)));
            }
            FileInformation.SerializeToDisk(opts.CatalogFilename, files);
            return 0;
        }

        private static int HandleParseError(IEnumerable<Error> errs)
        {
            return -1;
        }
    }
}
