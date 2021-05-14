using DbUp;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.IO;
using CommandLine;


namespace dbupa_application
{
    public class Program
    {
        public class Options
        {
            [Option('e', "environment", Required = true, HelpText = "Set the aplication environment runtime.")]
            public string Environment { get; set; }

            [Option('a', "application", Required = true, HelpText = "Set the aplication name configuration setting.")]
            public string Application { get; set; }
        }

        public static IConfigurationRoot Configuration { get; set; }
        public static string Environment { get; set; }
        static int Main(string[] args)
        {

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json")
                               .AddEnvironmentVariables();
            Configuration = builder.Build();

            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                var allowedEnvironments = Configuration.GetSection("environments").GetChildren().ToList().Select(e => e.Value.ToLower());
                var allowedApplications = Configuration.GetSection("connections").GetChildren().ToList().Select(e => e.Key.ToLower());

                if (allowedEnvironments.Contains(o.Environment.ToLower()))
                {
                    //verify if there's any application with the name provided
                    if (allowedApplications.Contains(o.Application.ToLower()))
                    {
                        try
                        {
                            var ApplicationConnectionString = Configuration.GetSection("connections")
                               .GetSection(o.Application).GetSection(o.Environment).GetSection("connection").Value;
                            Console.WriteLine("[" + o.Environment.ToUpper() + "][" + o.Application + "] Configuration :: " + ApplicationConnectionString);
                            DbUpAutomation(ApplicationConnectionString, o.Application);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" - Something went wrong retrieving the application settings or running the scripts ...");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" - Invalid application provided. Please provide one of the bellow: ");
                        allowedApplications.ToList().ForEach(e => { Console.WriteLine(" - " + e); });
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" - Invalid environment provided. Please provide one of the bellow: ");
                    allowedEnvironments.ToList().ForEach(e => { Console.WriteLine(" - " + e); });
                }

            });

            Console.WriteLine("Press any key to exit ...");

            return -1;
        }
        static void DbUpAutomation(string connectionString, string Application = "")
        {
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), (s) => s.ToLower().Contains("main.scripts"))
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), (s) => s.ToLower().Contains("application.scripts." + Application.ToLower() + "."))
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                Console.ReadLine(); //remove this line if you want to run it in azure
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[" + Application + "] Generic database scripts executed successfully");
            Console.ResetColor();
        }


    }
}
