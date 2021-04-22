﻿using System;
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

        public static bool ManageWorkingDirectories()
        {
            var workingDirectoryManageChoice =
                new Choices<ManageChoices>(@"How would you like to manage your repos?")
                    .Add(@"Add a git repo", ManageChoices.Add)
                    .Add(@"Update a git repo", ManageChoices.Update)
                    .Add(@"Remove a git repo", ManageChoices.Remove)
                    .Add(@"List repos", ManageChoices.List)
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
                case ManageChoices.Return:
                    return false;
            }

            return true;
        }
    }
}