using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using GitCheckout.Properties;

namespace GitCheckout
{
    class Program
    {
        private static string WorkingDirectory { get; set; }

        static void Main(string[] args)
        {
            WorkingDirectory = Settings.Default.WorkingDirectory;

            if (string.IsNullOrWhiteSpace(WorkingDirectory) || !args.Any())
            {
                var pathSplit = AppDomain.CurrentDomain.BaseDirectory.Split('\\');
                var path = string.Join(@"\\", pathSplit);
                var sourcetree = Resources.sourcetree.Replace("{{filepath}}", path);

                try
                {
                    File.WriteAllText("sourcetree.reg", sourcetree);

                    Process regeditProcess = Process.Start("regedit.exe", $"/s \"{AppDomain.CurrentDomain.BaseDirectory}sourcetree.reg\"");
                    regeditProcess?.WaitForExit();

                    File.Delete("sourcetree.reg");
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"To set up, please attempt to run the application manually as administrator.");
                    Console.WriteLine();
                    Console.WriteLine(@"Press any key to exit.");
                    Console.ReadKey();
                    return;
                }

                var currentDirString = !string.IsNullOrWhiteSpace(WorkingDirectory) ? $"[{WorkingDirectory}]" : "";
                Console.WriteLine($@"Please enter your git folder: {currentDirString}");
                var newDir = Console.ReadLine();
                WorkingDirectory = !string.IsNullOrWhiteSpace(newDir) ? newDir : WorkingDirectory;
                Console.WriteLine();

                Settings.Default.WorkingDirectory = WorkingDirectory;
                Settings.Default.Save();
            }

            if (args.Any())
            {
                var urlString = args[0];
                var url = new Uri(urlString);

                if (url.Host == "checkoutref")
                {
                    var queryString = System.Web.HttpUtility.ParseQueryString(url.Query);
                    var refString = queryString["ref"];
                    var branch = refString.Split('/').Last();

                    Console.WriteLine(@"Fetching new branches...");
                    RunGit("fetch --progress");
                    Console.WriteLine(@"Done!");
                    Console.WriteLine();

                    Console.WriteLine($@"Checking out branch '{branch}'...");
                    RunGit($"checkout {branch}");  
                    Console.WriteLine(@"Done!");
                    Console.WriteLine();
                }
            }
            Console.WriteLine(@"Press any key to exit.");
            Console.ReadKey();
        }

        private static void RunGit(string command)
        {
            var info = new ProcessStartInfo("git", command)
            {
                WorkingDirectory = WorkingDirectory,
                UseShellExecute = false
            };
            var proc = Process.Start(info);
            proc?.WaitForExit();
        }
    }
}
