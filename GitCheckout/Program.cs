using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GitCheckout.Properties;

namespace GitCheckout
{
    class Program
    {
        private static string[] Args { get; set; }
        
        static void Main(string[] args)
        {
            Args = args;

            CheckElevate();
            CheckInit();

            if (TryGetBranch(out var branch))
            {
                while (!Checkout(branch)) { }

                Console.WriteLine(@"Press any key to exit.");
                Console.ReadKey();
            }
            else
            {
                if (Args.Any())
                {
                    Console.WriteLine(@$"Invalid checkout url '{Args[0]}'");
                    Console.WriteLine();
                }
                
                while (MainMenu()) { }
            }
        }

        private static void CheckElevate()
        {
            if (Args.Any() && Args[0] == "/elevateFor")
            {
                var protocol = new Protocol(Args[2]);

                if (!Enum.TryParse<ElevateFor>(Args[1], out var elevateFor))
                {
                    Environment.Exit(1);
                }
                
                int exitCode;
                switch (elevateFor)
                {
                    case ElevateFor.Register:
                        exitCode = ProtocolManager.RegisterProtocolInner(protocol) ? 0 : 1;
                        Environment.Exit(exitCode);
                        break;
                    case ElevateFor.Deregister:
                        exitCode = ProtocolManager.DeregisterProtocolInner(protocol) ? 0 : 1;
                        Environment.Exit(exitCode);
                        break;
                }
            }
        }
        
        private static void CheckInit()
        {
            if (Settings.Default.WorkingDirectories == null)
            {
                Settings.Default.WorkingDirectories = new StringCollection();
                Settings.Default.Save();
            }

            if (Settings.Default.Protocols == null)
            {
                Settings.Default.Protocols = new StringCollection();
                Settings.Default.Save();
            }

            if (Settings.Default.Protocols.Count == 0)
            {
                while (!ProtocolManager.AddProtocol()) {}
            }

            if (Settings.Default.WorkingDirectories.Count == 0)
            {
                while (!WorkingDirectoryManager.AddWorkingDirectory()) {}
            }
        }


        private static bool TryGetBranch(out string branch)
        {
            branch = null;

            if (!Args.Any()) return false;
            
            var urlString = Args[0];

            if (string.IsNullOrWhiteSpace(urlString)) return false;
            
            var url = new Uri(urlString);
            
            foreach (var protocolString in Settings.Default.Protocols)
            {
                var protocol = new Protocol(protocolString);

                if (url.Scheme != protocol.Scheme) continue; 
                
                if (!url.Host.Equals(protocol.Host, StringComparison.InvariantCultureIgnoreCase)) continue;
                
                var queryString = System.Web.HttpUtility.ParseQueryString(url.Query);
                var branchQuery = queryString[protocol.Query];
                
                if (string.IsNullOrWhiteSpace(branchQuery)) continue;
                
                branch = branchQuery.Replace("refs/heads/", "");
                return true;
            }

            return false;
        }

        private static bool Checkout(string branch)
        {
            var checkoutDirectoryChoice = 
                new Choices($@"Please select a git repo to checkout branch '{branch}' on:", Settings.Default.WorkingDirectories)
                    .Add("Manage repos")
                    .Add("Cancel")
                    .Choose();
                
            if (checkoutDirectoryChoice == null) return false;

            switch (checkoutDirectoryChoice.Value)
            {
                case "Manage repos":
                    while(WorkingDirectoryManager.ManageWorkingDirectories()) {}
                    return false;
                case "Cancel":
                    Environment.Exit(0);
                    return true;
                default:
                    Console.WriteLine(@"Fetching new branches...");
                    RunGit("fetch --progress", checkoutDirectoryChoice.Value);
                    Console.WriteLine(@"Done!");
                    Console.WriteLine();

                    Console.WriteLine($@"Checking out branch '{branch}'...");
                    RunGit($"checkout {branch}", checkoutDirectoryChoice.Value);
                    Console.WriteLine(@"Done!");
                    Console.WriteLine();
                    return true;
            }
        }

        private static bool MainMenu()
        {
            var mainMenuChoice = 
                new Choices<MainMenuChoices>(@"What would you like to do?")
                    .Add(@"Manage protocols", MainMenuChoices.Protocols)
                    .Add(@"Manage repos", MainMenuChoices.Directories)
                    .Add("Exit", MainMenuChoices.Exit)
                    .Choose();
                
            switch (mainMenuChoice?.Value)
            {
                case MainMenuChoices.Protocols:
                    while (ProtocolManager.ManageProtocols()) {}
                    break;
                case MainMenuChoices.Directories:
                    while (WorkingDirectoryManager.ManageWorkingDirectories()) {}
                    break;
                case MainMenuChoices.Exit:
                    return false;
            }

            return true;
        }

        private static void RunGit(string command, string workingDirectory)
        {
            var info = new ProcessStartInfo("git", command)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            };
            var proc = Process.Start(info);
            proc?.WaitForExit();
        }

        public static bool Elevate(ElevateFor elevateFor, string extraArgs)
        {
            var path = typeof(Program).Assembly.Location;
                
            try
            {
                var startInfo = new ProcessStartInfo(path, $"/elevateFor {elevateFor} {extraArgs}")
                {
                    Verb = "runas"
                };

                var process = Process.Start(startInfo);

                if (process == null) return false;

                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
