using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private static StringCollection WorkingDirectories { get; set; }
        private static string[] Args { get; set; }
        
        static void Main(string[] args)
        {
            Args = args;

            if (!Settings.Default.Registered)
            {
                Settings.Default.WorkingDirectories = new StringCollection();
                Settings.Default.Save();

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

                Settings.Default.Registered = true;
                Settings.Default.Save();
            }

            WorkingDirectories = Settings.Default.WorkingDirectories;

            if (WorkingDirectories.Count == 0)
            {
                AddWorkingDirectory();
            }

            if (TryGetBranchFromArgs(out var branch))
            {
                while (!Checkout(branch)) { }

                Console.WriteLine(@"Press any key to exit.");
                Console.ReadKey();
            }
            else
            {
                ListWorkingDirectories();

                while (ChangeWorkingDirectory("Exit")) { }
            }
        }

        private static bool TryGetBranchFromArgs(out string branch)
        {
            branch = null;
            
            if (!Args.Any()) return false;
            
            var urlString = Args[0];

            if (string.IsNullOrWhiteSpace(urlString)) return false;
            
            var url = new Uri(urlString);

            if (url.Host != "checkoutref") return false;
            
            var queryString = System.Web.HttpUtility.ParseQueryString(url.Query);
            var refString = queryString["ref"];
            branch = refString.Split('/').Last();

            return true;
        }

        private static bool Checkout(string branch)
        {
            var checkoutDirectoryChoice = Choose("Please select a git repo to checkout on", 
                new Choices(WorkingDirectories)
                    .Add("Manage")
                    .Add("Cancel"));
                
            if (checkoutDirectoryChoice == null) return false;

            switch (checkoutDirectoryChoice.Value)
            {
                case "Manage":
                    while(ChangeWorkingDirectory()) {}
                    return false;
                case "Cancel":
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
        
        private static bool ChangeWorkingDirectory(string returnText = "Return")
        {
            var workingDirectoryChangeChoice = Choose(@"What would you like to do?", 
                new Choices<WorkingDirectoryChanges>()
                    .Add(@"Add a git repo", WorkingDirectoryChanges.Add)
                    .Add(@"Update a git repo", WorkingDirectoryChanges.Update)
                    .Add(@"Remove a git repo", WorkingDirectoryChanges.Remove)
                    .Add(@"List repos", WorkingDirectoryChanges.List)
                    .Add(returnText, WorkingDirectoryChanges.Return));
                
            switch (workingDirectoryChangeChoice?.Value)
            {
                case WorkingDirectoryChanges.Add:
                    AddWorkingDirectory();
                    break;
                case WorkingDirectoryChanges.Update:
                    UpdateWorkingDirectory();                        
                    break;
                case WorkingDirectoryChanges.Remove:
                    RemoveWorkingDirectory();
                    break;                
                case WorkingDirectoryChanges.List:
                    ListWorkingDirectories();
                    break;
                case WorkingDirectoryChanges.Return:
                    return false;
            }

            return true;
        }
        
        private static Choices<T>.Choice Choose<T>(string question, Choices<T> choices)
        {
            Console.WriteLine(question);
            
            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                Console.WriteLine($@"[{i + 1}] {choice.Text}");
            }

            var chosenString = Console.ReadLine();
            Console.WriteLine();

            var chosen = choices.FirstOrDefault(x => x.Text.Equals(chosenString, StringComparison.InvariantCultureIgnoreCase));
            if (chosen != null)
            {
                return chosen;
            }
            
            if (!int.TryParse(chosenString, out var chosenNumber))
            {
                return null;
            }
            
            return choices.ElementAtOrDefault(chosenNumber - 1);
        }

        private class Choices : Choices<string>
        {
            public Choices(StringCollection collection)
            {
                foreach (var choice in collection)
                {
                    Add(choice);
                }
            }
            public Choices Add(string value)
            {
                Add(value, value);
                return this;
            }
        }
        
        private class Choices<T> : List<Choices<T>.Choice>
        {
            public Choices<T> Add(string text, T value)
            {
                Add(new Choice {Text = text, Value = value});
                return this;
            }
            
                    
            public class Choice
            {
                public string Text { get; set; }
                public T Value { get; set; }
            }
        }


        public enum WorkingDirectoryChanges
        {
            Add,
            Update,
            Remove,
            List,
            Return
        }
        
        private static void AddWorkingDirectory()
        {
            Console.WriteLine($@"Please enter your git repo: ");
            var workingDirectory = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(workingDirectory)) return;
            
            Settings.Default.WorkingDirectories.Add(workingDirectory);
            Settings.Default.Save();
            
            Console.WriteLine($@"Added repo ""{workingDirectory}""");
            Console.WriteLine();
        }

        private static void UpdateWorkingDirectory()
        {
            var updateDirectoryChoice = Choose("Please select a git repo to update", new Choices(WorkingDirectories));

            if (updateDirectoryChoice == null) return;
            
            Console.WriteLine($@"Please enter your new git repo: [{updateDirectoryChoice.Value}]");
            var workingDirectory = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(workingDirectory)) return;
            
            var index = Settings.Default.WorkingDirectories.IndexOf(updateDirectoryChoice.Value);
            Settings.Default.WorkingDirectories[index] = workingDirectory;
            Settings.Default.Save();
            
            Console.WriteLine($@"Updated repo from ""{updateDirectoryChoice.Value}"" to ""{workingDirectory}""");
            Console.WriteLine();
        }

        private static void RemoveWorkingDirectory()
        {
            var removeDirectoryChoice = Choose("Please select a git repo to remove", new Choices(WorkingDirectories));
            
            if (removeDirectoryChoice == null) return;

            Settings.Default.WorkingDirectories.Remove(removeDirectoryChoice.Value);
            Settings.Default.Save();
            
            Console.WriteLine($@"Removed repo ""{removeDirectoryChoice.Value}""");
            Console.WriteLine();
        }

        private static void ListWorkingDirectories()
        {
            Console.WriteLine(@"Repos:");
            foreach (var workingDirectory in WorkingDirectories)
            {
                Console.WriteLine(workingDirectory);
            }
            Console.WriteLine();
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
    }
}
