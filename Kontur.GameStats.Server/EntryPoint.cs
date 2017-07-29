using System;
using System.Net;
using Fclp;

namespace Kontur.GameStats.Server
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            var commandLineParser = new FluentCommandLineParser<Options>();

            commandLineParser
                .Setup(options => options.Prefix)
                .As("prefix")
                .SetDefault("http://+:8080/")
                .WithDescription("HTTP prefix to listen on");

            commandLineParser
                .SetupHelp("h", "help")
                .WithHeader($"{AppDomain.CurrentDomain.FriendlyName} [--prefix <prefix>]")
                .Callback(text => Console.WriteLine(text));

            if (commandLineParser.Parse(args).HelpCalled)
                return;

            RunServer(commandLineParser.Object);
        }

        private static void RunServer(Options options)
        {
            using (var server = new StatServer())
            {
                try
                {
                    server.Start(options.Prefix);
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 5)
                    {
                        Console.WriteLine("You need to run in admin mode or run the following command:");
                        Console.WriteLine("  netsh http add urlacl url={0} user={1}\\{2} listen=yes",
                            options.Prefix, Environment.GetEnvironmentVariable("USERDOMAIN"),
                            Environment.GetEnvironmentVariable("USERNAME"));
                        return;
                    }
                    if (ex.ErrorCode == 183) // Failed to listen on prefix  because it conflicts with an existing registration on the machine.
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    throw;
                }
                
                Console.ReadKey(true);
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
