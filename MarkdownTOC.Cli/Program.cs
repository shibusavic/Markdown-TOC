using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MarkdownTOC.Cli
{
    class Program
    {
        static bool verbose = false;
        static bool showHelp = false;
        static HashSet<FileInfo> markdownFileInfos = new();
        static FileInfo outputFileInfo = null;
        static DirectoryInfo directoryInfo = null;
        static bool recursiveDirectorySearch = false;
        static bool overwriteOutputFile = false;

        static async Task Main(string[] args)
        {
            int exitCode = -1;

            ProcessArguments(args);

            try
            {
                ValidateArguments();

                if (showHelp)
                {
                    ShowHelp();
                }
                else
                {
                    if (markdownFileInfos.Any())
                    {
                        foreach (var markdownFileInfo in markdownFileInfos)
                        {
                            await MarkdownFile.CreateInternalTableOfContentsAsync(markdownFileInfo);
                            Communicate($"TOC added to {markdownFileInfo.FullName}");
                        }
                    }

                    if (directoryInfo != null)
                    {
                        await MarkdownFile.WriteDirectoryTableOfContentsToFileAsync(directoryInfo,
                            outputFileInfo, recursiveDirectorySearch, overwriteOutputFile);
                        Communicate($"{outputFileInfo.FullName} created.");
                    }
                }

                exitCode = 0;
            }
            catch (Exception exc)
            {
                ShowHelp(exc.ToString());
                exitCode = 1;
            }
            finally
            {
                Environment.Exit(exitCode);
            }
        }

        static void Communicate(string message, bool force = false)
        {
            if (verbose || force)
            {
                Console.WriteLine(message);
            }
        }

        static void ShowHelp(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"{message}{Environment.NewLine}");
            }

            Dictionary<string, string> helpDefinitions = new()
            {
                { "[-f|--file <file name>]", "Add a table of contents to the specified file." },
                { "[-d|--directory <directory name>]", "Find all markdown file in the specified directory." },
                { "[-r|--recursive]", "Make directory search recursive." },
                { "[-o|--output-file <file name>]", "Write output to the specified file." },
                { "[--overwrite]", "Overwrite the output file if it already exists." },
                { "[-v|--verbose]", "Write details to console." },
                { "[-h|-?|?|--help]", "Show this help." },
            };

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

            Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

            foreach (KeyValuePair<string, string> helpItem in helpDefinitions)
            {
                Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
            }
        }

        static void ProcessArguments(string[] args)
        {
            for (int a = 0; a < args.Length; a++)
            {
                string argument = args[a].ToLower();

                switch (argument)
                {
                    case "--file":
                    case "-f":
                        if (a == args.Length - 1) { throw new ArgumentException($"Expecting a file name after {args[a]}"); }
                        markdownFileInfos.Add(new FileInfo(args[++a]));
                        break;
                    case "--directory":
                    case "-d":
                        if (a == args.Length - 1) { throw new ArgumentException($"Expecting a directory name after {args[a]}"); }
                        directoryInfo = new DirectoryInfo(args[++a]);
                        break;
                    case "--recursive":
                    case "-r":
                        recursiveDirectorySearch = true;
                        break;
                    case "--output-file":
                    case "-o":
                        if (a == args.Length - 1) { throw new ArgumentException($"Expecting a file name after {args[a]}"); }
                        outputFileInfo = new FileInfo(args[++a]);
                        break;
                    case "--overwrite":
                        overwriteOutputFile = true;
                        break;
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "--help":
                    case "-h":
                    case "-?":
                    case "?":
                        showHelp = true;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: {args[a]}");
                }
            }
        }

        static void ValidateArguments()
        {
            if (markdownFileInfos.Any())
            {
                foreach (FileInfo markdownFileInfo in markdownFileInfos)
                {
                    if (!markdownFileInfo.Exists)
                    {
                        throw new ArgumentException($"'{markdownFileInfo.FullName}' could not be found.");
                    }
                }
            }

            if (directoryInfo != null && !directoryInfo.Exists)
            {
                throw new ArgumentException($"'{directoryInfo.FullName}' could not be found.");
            }

            if (outputFileInfo != null && outputFileInfo.Exists && !overwriteOutputFile)
            {
                throw new ArgumentException($"'{outputFileInfo.FullName}' already exists. Use the --overwrite argument to overwrite.");
            }

            if (directoryInfo != null && outputFileInfo == null)
            {
                throw new ArgumentException($"When doing a directory search, you must provide an output file. Use the -o <file name> argument.");
            }
        }
    }
}
