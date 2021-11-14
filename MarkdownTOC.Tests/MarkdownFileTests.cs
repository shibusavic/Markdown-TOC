using MarkdownTOC.Cli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MarkdownTOC.Tests
{
    public class MarkdownFileTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public MarkdownFileTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void CreateInternalTableOfContentsAsync_NullFileName_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => MarkdownFile.CreateInternalTableOfContentsAsync(null));
        }

        [Fact]
        public void CreateInternalTableOfContentsAsync_FileDoesNotExist_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(() => MarkdownFile.CreateInternalTableOfContentsAsync(new FileInfo("test.md")));
        }

        [Fact]
        public void CreateInternalTableOfContentsAsync_BadExtension_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(() => MarkdownFile.CreateInternalTableOfContentsAsync(new FileInfo("test.docx")));
        }

        [Fact]
        public async Task CreateInternalTableOfContentsAsync_AddTOC()
        {
            const int NumberOfHeadersToCreate = 5;

            var fileName = await CreateMarkdownFileAsync(Path.GetTempPath(), NumberOfHeadersToCreate);

            testOutputHelper.WriteLine(fileName);

            await MarkdownFile.CreateInternalTableOfContentsAsync(new FileInfo(fileName));

            var lines = await File.ReadAllLinesAsync(fileName);

            Assert.Contains("Table of Contents", lines[0]);

            for (int i = 2; i <= NumberOfHeadersToCreate; i += 2)
            {
                Assert.Contains($"Header {i / 2}", lines[i]);
            }
        }

        [Fact]
        public async Task WriteDirectoryTableOfContentsToFile_TargetFileExists_OverwriteFalse_Throws()
        {
            string name = Guid.NewGuid().ToString().Substring(0, 5);
            await File.WriteAllTextAsync(name, "test");
            DirectoryInfo dirInfo = new(".");
            FileInfo fileInfo = new(name);
            await Assert.ThrowsAsync<ArgumentException>(() => MarkdownFile.WriteDirectoryTableOfContentsToFileAsync(dirInfo, fileInfo, overwriteOutputFile: false));
        }

        [Fact]
        public async Task WriteDirectoryTableOfContentsToFile_FindsFiles()
        {
            string tempDirName = Guid.NewGuid().ToString().Substring(0, 5);
            string newPath = Path.Combine(Path.GetTempPath(), tempDirName);

            DirectoryInfo dirInfo = Directory.CreateDirectory(newPath);

            const int NumberToCreate = 5;
            string[] files = new string[NumberToCreate];

            for (int i = 0; i < NumberToCreate; i++)
            {
                files[i] = await CreateMarkdownFileAsync(newPath, 3);
            }

            FileInfo outputFileInfo = new("toc.md");
            await MarkdownFile.WriteDirectoryTableOfContentsToFileAsync(dirInfo, outputFileInfo);

            testOutputHelper.WriteLine(outputFileInfo.FullName);

            var linesFromOutputFile = await File.ReadAllLinesAsync(outputFileInfo.FullName);

            // We've got the header plus the NumberToCreate and each line has an extra line.
            Assert.Equal((NumberToCreate + 1) * 2, linesFromOutputFile.Length);
        }

        [Fact]
        public async Task WriteDirectoryTableOfContentsToFile_FindsFilesRecursively()
        {
            string tempDirName1 = Guid.NewGuid().ToString().Substring(0, 5);
            string tempDirName2 = Guid.NewGuid().ToString().Substring(0, 5);
            string newPath1 = Path.Combine(Path.GetTempPath(), tempDirName1);
            string newPath2 = Path.Combine(Path.GetTempPath(), tempDirName1, tempDirName2);

            DirectoryInfo dirInfo1 = Directory.CreateDirectory(newPath1);
            _ = Directory.CreateDirectory(newPath2);

            const int NumberToCreate = 5;
            string[] files = new string[NumberToCreate * 2];

            for (int i = 0; i < NumberToCreate; i++)
            {
                files[i] = await CreateMarkdownFileAsync(newPath1, 3);
                files[i + NumberToCreate] = await CreateMarkdownFileAsync(newPath2, 3);
            }

            FileInfo outputFileInfo = new("toc.md");
            await MarkdownFile.WriteDirectoryTableOfContentsToFileAsync(dirInfo1, outputFileInfo);

            testOutputHelper.WriteLine(outputFileInfo.FullName);

            var linesFromOutputFile = await File.ReadAllLinesAsync(outputFileInfo.FullName);

            // Twice the number to create plus one, times 2, lines should be in file.
            Assert.Equal(((NumberToCreate * 2) + 1) * 2, linesFromOutputFile.Length);
        }

        private static async Task<string> CreateMarkdownFileAsync(string path, int numberOfHeaders)
        {
            string name = Guid.NewGuid().ToString().Substring(0, 5);
            string fileName = Path.Combine(path, $"{name}.md");

            Random rnd = new();

            List<string> lines = new();

            for (int i = 1; i < numberOfHeaders; i++)
            {
                int headerLevel = rnd.Next(1, 5);
                lines.Add($"{string.Empty.PadLeft(headerLevel, '#')} Header {i}{Environment.NewLine}");
            }

            await File.WriteAllLinesAsync(fileName, lines);

            return fileName;
        }
    }
}
