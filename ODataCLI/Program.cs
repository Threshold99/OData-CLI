﻿using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace ODataCLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("-------------");
                Console.WriteLine("Odata CLI Tool to bootstrap an Odata service");
                Console.WriteLine("-------------");
                Console.WriteLine();
                Console.WriteLine("OPTIONS");
                Console.WriteLine("--csdl <Path to metadata file>");
                Console.WriteLine("--subscriptionId <The Azure Subscription Id>");
                Console.WriteLine("--appServiceName <The App Service Name.>");
                return;
            }
            var rootCommand = new RootCommand("Odata CLI Tool to bootstrap an Odata service")
            {
                new Option(new string[] {"--csdl", "--metadata"}, "The path to the metadata file.")
                {
                    Argument = new Argument<FileInfo>()
                },
                new Option(new string[] { "--subscriptionId", "--id" }, "The Azure Subscription Id.")
                {
                    Argument = new Argument<string>()
                },
                new Option(new string[] { "--appServiceName", "--app" }, "The App Service Name.")
                {
                    Argument = new Argument<string>()
                },
            };

            rootCommand.Handler = CommandHandler.Create<FileInfo, string, string>(Bootstrap);
            await rootCommand.InvokeAsync(args);
        }

        static void Bootstrap(FileInfo csdl, string subscriptionId, string appServiceName)
        {
            if (subscriptionId != null)
            {
                Console.WriteLine($"Subscription Id: {subscriptionId}");
            }

            if (csdl != null)
            {
                Console.WriteLine($"Csdl Path: {csdl.FullName}");
            }


            Console.WriteLine("Cloning from git .....");
            string _args = "https://github.com/habbes/ODataApiServiceHackathon.git";

            string tempWorkingDir = Path.GetTempPath() + appServiceName;
            Console.WriteLine(tempWorkingDir);

            while (Directory.Exists(tempWorkingDir))
            {
                tempWorkingDir += "-"+RandomString(3);
            }
            Console.WriteLine($"Cloning to {tempWorkingDir}");

            Repository.Clone(_args, tempWorkingDir);

            var projectPreparer = new ProjectPreparer(tempWorkingDir, csdl.FullName, appServiceName);
            projectPreparer.Prepare();

            var parametersJsonPath = projectPreparer.ParamsPath;
            var projectZipPath = projectPreparer.ZipPath;

            Console.WriteLine($"JsonParams {parametersJsonPath}");
            Console.WriteLine($"ProjectZip {projectZipPath}");

            //Execute the powershell script
            using (PowerShell PowerShellInst = PowerShell.Create())
            {

                //string path = System.IO.Path.GetDirectoryName(@"C:\Temp\") + "\\Get-EventLog.ps1";
                string path = @"ODataCLI\deploy.ps1";

                if (!string.IsNullOrEmpty(path))
                    PowerShellInst
                        .AddScript(File.ReadAllText(path))
                        .AddParameter("subscriptionId", subscriptionId)
                        .AddParameter("projectFilePath", projectZipPath)
                        .AddParameter("parametersFilePath", parametersJsonPath)
                        .AddParameter("resourceGroupName", $"rg_{appServiceName}")
                        .AddParameter("deploymentName", $"dpl_{appServiceName}")
                        .Invoke();
            }

            Console.WriteLine($"Cleaning directory: {tempWorkingDir}");
            DeleteDirectory(tempWorkingDir);
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var files = Directory.GetFiles(directoryPath);
            var directories = Directory.GetDirectories(directoryPath);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in directories)
            {
                DeleteDirectory(dir);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);

            Directory.Delete(directoryPath, false);
        }

    }
}
