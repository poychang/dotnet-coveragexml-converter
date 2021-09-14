using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CoverageXmlConverter
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(new[]{ "-f", "--coverage-files-folder" }, "The folder contain the .coverage files.") { IsRequired = true, ArgumentHelpName = "FOLDER_PATH"},
                new Option<bool>(new[]{ "-a", "--all-directories" }, () => true, "Includes sub-folders for search operation."),
                new Option<bool>(new[]{ "-p", "--process-all-files" }, () => false, "Convert all .coverage files. Default only convert the folders which are GUID (create by VSTest)."),
                new Option<bool>(new[]{ "-o", "--overwrite" }, () => true, "Overwrite the existing .coveragexml files."),
                new Option<bool>(new[]{ "-r", "--remove-original-files" }, "Remove the original .coverage files."),
            };

            rootCommand.Description = "Convert coverage files from '.coverage' binary files to '.coveragexml' xml files.";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<string, bool, bool, bool, bool>(
                (coverageFilesFolder, allDirectories, processAllFiles, overwrite, removeOriginalFiles) =>
                {
                    try
                    {
                        var coverageFiles = FindCoverageFiles(coverageFilesFolder, allDirectories, processAllFiles);

                        if (coverageFiles.Count == 0)
                        {
                            Logger.LogWarning($"Not found '.coverage' files in folder '{coverageFilesFolder}'.");
                            return;
                        }

                        coverageFiles.ForEach(sourceFilePath =>
                        {
                            var destinationFilePath = sourceFilePath.Replace(".coverage", ".coveragexml");

                            Logger.LogInformation($"Generating file '{destinationFilePath}' based on '{sourceFilePath}'.");

                            DeleteExistingDestinationFile(overwrite, destinationFilePath);
                            RunCodeCoverage(sourceFilePath, destinationFilePath);
                            DeleteOriginalCoverageFile(removeOriginalFiles, sourceFilePath);
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.LogCritical(ex, $"Error reading folder '{coverageFilesFolder}'.");
                    }
                });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        private static readonly ILogger<Program> Logger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddConsole();
        }).CreateLogger<Program>();

        private static List<string> FindCoverageFiles(string coverageFilesFolder, bool allDirectories, bool processAllFiles)
        {
            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory
                .EnumerateFiles(coverageFilesFolder, "*.coverage", searchOption)
                .Where(path => processAllFiles || Guid.TryParse(new DirectoryInfo(Path.GetDirectoryName(path) ?? string.Empty).Name, out _))
                .ToList();
        }

        private static void DeleteExistingDestinationFile(bool overwrite, string destinationFilePath)
        {
            try
            {
                if (overwrite && File.Exists(destinationFilePath)) File.Delete(destinationFilePath);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Unable to delete existing file '{destinationFilePath}'.");
            }
        }

        private static void RunCodeCoverage(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                var codeCoverageExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Microsoft.CodeCoverage", "CodeCoverage.exe");
                var process = Process.Start(codeCoverageExePath, $"analyze /output:\"{destinationFilePath}\" \"{sourceFilePath}\"");
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Error processing file '{sourceFilePath}'.");
            }
        }

        private static void DeleteOriginalCoverageFile(bool removeOriginalFile, string sourceFilePath)
        {
            try
            {
                if (removeOriginalFile && File.Exists(sourceFilePath)) File.Delete(sourceFilePath);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Unable to delete original file '{sourceFilePath}'.");
            }
        }
    }
}
