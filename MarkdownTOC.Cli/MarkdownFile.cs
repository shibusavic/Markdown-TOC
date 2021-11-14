using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownTOC.Cli
{
    /// <summary>
    /// Utility for creating table of contents within a file or a file with a table of contents for
    /// external links within a specified directory.
    /// </summary>
    public class MarkdownFile
    {
        private static readonly List<string> validMarkdownFileExtensions = new()
        {
            ".markdn",
            ".markdown",
            ".md",
            ".mdown",
            ".mdtext",
            ".mdtxt",
            ".mdwn",
            ".mkd",
            ".mkdn",
            ".text",
            ".mmd"
        };

        /// <summary>
        /// Gets the list of supported markdown file extensions.
        /// </summary>
        public static IEnumerable<string> ValidFileExtensions => validMarkdownFileExtensions;

        /// <summary>
        /// Create a "Table of Contents" section with internal links within the specified file.
        /// </summary>
        /// <param name="inputFileInfo">The <see cref="FileInfo"/> of the file to change.</param>
        /// <returns>A task representing the asyncronous operation.</returns>
        public static async Task CreateInternalTableOfContentsAsync(FileInfo inputFileInfo)
        {
            if (inputFileInfo == null) { throw new ArgumentNullException(nameof(inputFileInfo)); }

            if (!inputFileInfo.Exists)
            {
                throw new ArgumentException("Provided input file does not exist.");
            }

            if (!validMarkdownFileExtensions.Contains(Path.GetExtension(inputFileInfo.Name), StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"The provided input file does not a supported extension. Supported extensions are: {string.Join(',', validMarkdownFileExtensions)}");
            }

            var fileLines = (await File.ReadAllLinesAsync(inputFileInfo.FullName)).ToList();

            if (fileLines.Any())
            {
                List<string> headers = fileLines.Where(l => l.TrimStart().StartsWith("#")).ToList();

                headers.Reverse();

                fileLines.Insert(0, "-----");

                foreach (var header in headers)
                {
                    int headerLevel = CountHashes(header);

                    string newTitle = $"#{header[headerLevel..].Trim().ToLower().Replace(' ', '-')}";

                    string newLine = $"[{header[headerLevel..].Trim()}]({newTitle}){Environment.NewLine}";

                    fileLines.Insert(0, newLine);
                }

                fileLines.Insert(0, $"# Table of Contents{Environment.NewLine}");

                await File.WriteAllLinesAsync(inputFileInfo.FullName, fileLines);
            }
        }

        /// <summary>
        /// Create a new file 
        /// </summary>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> of the directory to search.</param>
        /// <param name="outputFileInfo">The <see cref="FileInfo"/> of the file to generate.</param>
        /// <param name="recursive">An indicator of whether the directory search should be recursive.</param>
        /// <param name="overwriteOutputFile">An indicator of whether the output file should be overwritten when it exists.</param>
        /// <returns>A task representing the asyncronous operation.</returns>
        public static async Task WriteDirectoryTableOfContentsToFileAsync(DirectoryInfo directoryInfo,
            FileInfo outputFileInfo,
            bool recursive = true,
            bool overwriteOutputFile = true)
        {
            if (directoryInfo == null) { throw new ArgumentNullException(nameof(directoryInfo)); }
            if (outputFileInfo == null) { throw new ArgumentNullException(nameof(outputFileInfo)); }
            if (!directoryInfo.Exists) { throw new ArgumentException("Provided directory does not exist."); }
            if (outputFileInfo.Exists && !overwriteOutputFile) { throw new ArgumentException($"Specified output file already exists; use the {nameof(overwriteOutputFile)} argument to overwrite."); }

            List<string> lines = new();
            lines.Add($"# Table of Contents{Environment.NewLine}");

            var files = directoryInfo.GetFiles("*", new EnumerationOptions()
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = recursive,
                MatchType = MatchType.Simple
            }).Where(f => validMarkdownFileExtensions.Contains(Path.GetExtension(f.Name), StringComparer.OrdinalIgnoreCase)).OrderBy(f => f.FullName);

            foreach (FileInfo file in files)
            {
                string relativePath = Path.GetRelativePath(Path.GetDirectoryName(outputFileInfo.FullName), file.FullName);

                if (!TryGetFileDescription(file, out string description))
                {
                    description = Path.GetFileNameWithoutExtension(file.Name);
                }

                lines.Add($"[{description}]({relativePath}){Environment.NewLine}");
            }

            await File.WriteAllLinesAsync(outputFileInfo.FullName, lines);
        }

        static readonly Regex regexDescription = new(@"^[#]+\s+?([^\r\n]+)", RegexOptions.Multiline);

        static bool TryGetFileDescription(FileInfo fileInfo, out string description)
        {
            var matches = regexDescription.Matches(File.ReadAllText(fileInfo.FullName));
            description = matches.Any() ? matches.First().Groups[1].Captures[0].Value : null;
            return matches.Any();
        }

        static int CountHashes(string header)
        {
            int count = 0;
            char[] arr = header.TrimStart().ToCharArray();
            for (int c = 0; c < arr.Length; c++)
            {
                if (arr[c] == '#') { count++; }
                else { break; }
            }
            return count;
        }
    }
}
