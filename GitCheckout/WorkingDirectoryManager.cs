using System;
using GitCheckout.Properties;

namespace GitCheckout
{
    internal static class WorkingDirectoryManager
    {
        public static bool AddWorkingDirectory()
        {
            Console.WriteLine(@"Please enter your git repo: ");
            var workingDirectory = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(workingDirectory)) return false;
            
            Settings.Default.WorkingDirectories.Add(workingDirectory);
            Settings.Default.Save();
            
            Console.WriteLine($@"Added repo ""{workingDirectory}""");
            Console.WriteLine();
            
            return true;
        }

        public static void UpdateWorkingDirectory()
        {
            var updateDirectoryChoice = new Choices("Please select a git repo to update", Settings.Default.WorkingDirectories).Choose();

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

        public static void RemoveWorkingDirectory()
        {
            var removeDirectoryChoice = new Choices("Please select a git repo to remove", Settings.Default.WorkingDirectories).Choose();
            
            if (removeDirectoryChoice == null) return;

            Settings.Default.WorkingDirectories.Remove(removeDirectoryChoice.Value);
            Settings.Default.Save();
            
            Console.WriteLine($@"Removed repo ""{removeDirectoryChoice.Value}""");
            Console.WriteLine();
        }

        public static void ListWorkingDirectories()
        {
            Console.WriteLine(@"Repos:");
            foreach (var workingDirectory in Settings.Default.WorkingDirectories)
            {
                Console.WriteLine(workingDirectory);
            }
            Console.WriteLine();
        }
        
        public static void SetDefaultWorkingDirectory()
        {
            var defaultDirectoryChoice = new Choices("Please select a git repo to set as default", Settings.Default.WorkingDirectories)
                .Add("Cancel")
                .Default(Settings.Default.DefaultWorkingDirectory ?? Settings.Default.LastUsedWorkingDirectory)
                .Choose();
            
            if (defaultDirectoryChoice == null) return;

            switch (defaultDirectoryChoice.Value)
            {
                case "Cancel":
                    return;
                default:
                    var index = Settings.Default.WorkingDirectories.IndexOf(defaultDirectoryChoice.Value);
                    Settings.Default.DefaultWorkingDirectory = index;
                    Settings.Default.Save();
            
                    Console.WriteLine($@"Set repo ""{defaultDirectoryChoice.Value}"" as default");
                    Console.WriteLine();
                    return;
            }
        }

        public static void ClearDefaultWorkingDirectory()
        {
            Settings.Default.DefaultWorkingDirectory = null;
            Settings.Default.Save();
            
            Console.WriteLine(@"Cleared default repo");
            Console.WriteLine();
        }

        public static bool ManageWorkingDirectories()
        {
            var workingDirectoryManageChoice =
                new Choices<ManageChoices>(@"How would you like to manage your repos?")
                    .Add(@"Add a git repo", ManageChoices.Add)
                    .Add(@"Update a git repo", ManageChoices.Update)
                    .Add(@"Remove a git repo", ManageChoices.Remove)
                    .Add(@"List git repos", ManageChoices.List)
                    .Add(@"Set a git repo as default", ManageChoices.SetDefault)
                    .Add(@"Clear default repo", ManageChoices.ClearDefault)
                    .Add("Return", ManageChoices.Return)
                    .Choose();
                
            switch (workingDirectoryManageChoice?.Value)
            {
                case ManageChoices.Add:
                    AddWorkingDirectory();
                    break;
                case ManageChoices.Update:
                    UpdateWorkingDirectory();                        
                    break;
                case ManageChoices.Remove:
                    RemoveWorkingDirectory();
                    break;                
                case ManageChoices.List:
                    ListWorkingDirectories();
                    break;                
                case ManageChoices.SetDefault:
                    SetDefaultWorkingDirectory();
                    break;
                case ManageChoices.ClearDefault:
                    ClearDefaultWorkingDirectory();
                    break;
                case ManageChoices.Return:
                    return false;
            }

            return true;
        }
    }
}