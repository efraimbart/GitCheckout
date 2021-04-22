using System;
using System.Diagnostics;
using System.IO;
using GitCheckout.Properties;

namespace GitCheckout
{
    internal static class ProtocolManager
    {
        public static bool AddProtocol()
        {
            var protocolChoice =
                new Choices<ProtocolChoices>("Add a protocol")
                    .Add(ProtocolChoices.GitHub)
                    .Add(ProtocolChoices.SourceTree)
                    .Add(ProtocolChoices.Custom)
                    .Choose();

            switch (protocolChoice?.Value)
            {
                case ProtocolChoices.SourceTree:
                    AddProtocol(new Protocol { Scheme = "sourcetree", Host = "checkoutref", Query = "ref"});
                    return true;
                case ProtocolChoices.GitHub:
                    AddProtocol(new Protocol { Scheme = "x-github-client", Host = "openRepo", Query = "branch"});
                    return true;
                case ProtocolChoices.Custom:
                    var protocol = RequestProtocol();
                    if (protocol != null)
                    {
                        AddProtocol(protocol);
                        return true;
                    }
                    return false;
            }

            return false;

            static Protocol RequestProtocol()
            {
                Console.WriteLine(@"Please enter a protocol: ");
                var scheme = Console.ReadLine();
                Console.WriteLine();

                if (string.IsNullOrWhiteSpace(scheme)) return null;
                
                Console.WriteLine(@"Please enter the protocol's host: ");
                var host = Console.ReadLine();
                Console.WriteLine();

                if (string.IsNullOrWhiteSpace(host)) return null;

                Console.WriteLine(@"Please enter the protocol's branch param: ");
                var query = Console.ReadLine();
                Console.WriteLine();

                if (string.IsNullOrWhiteSpace(query)) return null;

                return new Protocol { Scheme = scheme, Host = host, Query = query };
            }

            static void AddProtocol(Protocol protocol)
            {
                Settings.Default.Protocols.Add(protocol.ToString());
                Settings.Default.Save();

                if (RegisterProtocol(protocol))
                {
                    Console.WriteLine($@"Protocol ""{protocol.Scheme}"" added and registered");
                }
                else
                {
                    Console.WriteLine($@"Protocol ""{protocol.Scheme}"" added but not registered. To register protocols, please attempt to run the application manually as administrator.");
                }
                Console.WriteLine();
            }
        }

        public static void UpdateProtocol()
        {
            var updateProtocolChoice = new Choices("Please select a protocol to update", Settings.Default.Protocols).Choose();
            if (updateProtocolChoice == null) return;

            var updateProtocolChoiceValue = new Protocol(updateProtocolChoice.Value);

            Console.WriteLine($@"Please enter a protocol: [{updateProtocolChoiceValue.Scheme}]");
            var scheme = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(scheme))
            {
                scheme = updateProtocolChoiceValue.Scheme;
            }
                
            Console.WriteLine($@"Please enter the protocol's host: [{updateProtocolChoiceValue.Host}]");
            var host = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(host))
            {
                host = updateProtocolChoiceValue.Host;
            }
            
            Console.WriteLine($@"Please enter the protocol's branch param: [{updateProtocolChoiceValue.Query}]");
            var query = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(query))
            {
                query = updateProtocolChoiceValue.Query;
            }
            
            var protocol = new Protocol { Scheme = scheme, Host = host, Query = query };

            var index = Settings.Default.Protocols.IndexOf(updateProtocolChoice.Value);
            Settings.Default.Protocols[index] = protocol.ToString();
            Settings.Default.Save();

            if (protocol.Scheme != updateProtocolChoiceValue.Scheme)
            {
                if (RegisterProtocol(protocol))
                {
                    Console.WriteLine(@"Protocol updated and registered");
                }
                else
                {
                    Console.WriteLine(@"Protocol updated but not registered. To register protocols, please attempt to run the application manually as administrator.");
                }
                Console.WriteLine();
            }
        }

        public static void RemoveProtocol()
        {
            var removeProtocolChoice = new Choices("Please select a git repo to remove", Settings.Default.Protocols).Choose();
            if (removeProtocolChoice == null) return;
            
            var removeProtocolChoiceValue = new Protocol(removeProtocolChoice.Value);

            //TODO: Deregister protocol
            
            Settings.Default.Protocols.Remove(removeProtocolChoice.Value);
            Settings.Default.Save();
            
            Console.WriteLine($@"Removed protocol ""{removeProtocolChoiceValue.Scheme}""");
            Console.WriteLine();
        }

        public static void ReRegisterProtocol()
        {
            var reRegisterProtocolChoice = new Choices("Please select a protocol to re-register", Settings.Default.Protocols).Choose();
            if (reRegisterProtocolChoice == null) return;

            var reRegisterProtocolChoiceValue = new Protocol(reRegisterProtocolChoice.Value);
            
            if (RegisterProtocol(reRegisterProtocolChoiceValue))
            {
                Console.WriteLine(@"Protocol registered");
            }
            else
            {
                Console.WriteLine(@"Protocol not registered. To register protocols, please attempt to run the application manually as administrator.");
            }
            Console.WriteLine();
        }

        public static void ListProtocols()
        {
            Console.WriteLine(@"Protocols:");
            foreach (var protocol in Settings.Default.Protocols)
            {
                Console.WriteLine(protocol);
            }
            Console.WriteLine();
        }

        public static bool ManageProtocols()
        {
            var protocolManageChoice =
                new Choices<ManageChoices>(@"How would you like to manage your protocols?")
                    .Add(@"Add a protocol", ManageChoices.Add)
                    .Add(@"Update a protocol", ManageChoices.Update)
                    .Add(@"Remove a protocol", ManageChoices.Remove)
                    .Add(@"Re-register a protocol", ManageChoices.ReRegister)
                    .Add(@"List protocols", ManageChoices.List)
                    .Add("Return", ManageChoices.Return)
                    .Choose();
                
            switch (protocolManageChoice?.Value)
            {
                case ManageChoices.Add:
                    AddProtocol();
                    break;
                case ManageChoices.Update:
                    UpdateProtocol();                        
                    break;
                case ManageChoices.Remove:
                    RemoveProtocol();
                    break;                
                case ManageChoices.ReRegister:
                    ReRegisterProtocol();
                    break;
                case ManageChoices.List:
                    ListProtocols();
                    break;
                case ManageChoices.Return:
                    return false;
            }

            return true;
        }
        
        private static bool RegisterProtocol(Protocol protocol)
        {
            var pathSplit = AppDomain.CurrentDomain.BaseDirectory.Split('\\');
            var path = string.Join(@"\\", pathSplit);
            
            var protocolReg = Resources.protocol
                .Replace("{{protocol}}", protocol.Scheme)
                .Replace("{{filepath}}", path);

            try
            {
                File.WriteAllText("protocol.reg", protocolReg);

                Process regeditProcess = Process.Start("regedit.exe", $"/s \"{AppDomain.CurrentDomain.BaseDirectory}protocol.reg\"");
                regeditProcess?.WaitForExit();

                File.Delete("protocol.reg");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}