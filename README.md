## Markdown Table of Contents Creator

This utility will do one or both of the following:

1. Create a table of contents within a markdown file.
1. Create a table of contents file by finding all markdown files within a directory structure.

### Examples

To create a table of contents within an existing file, use:

```
md-toc -f c:\documents\myfile.md
```

You can specify multiple markdown files, as in:

```
md-toc -f c:\documents\myfile.md -f c:\documents\my-other-file.md
```

The above will create a "Table of Contents" section at the top of the file and provide **internal** links to all of the headers within that file.

To create a table of contents file from all markdown files within a directory structure, use:

```
md-toc -d c:\documents -o c:\documents\toc.md
```

The above will create a file with a "Table of Contents" header followed by a list of **external** links to all of the discovered markdown files. Note that the location of the target (generated) file is important and **must be the file's final resting place** because the external links within the document use **relative** paths. Moving the file after creation will break the generated links.

To find files recursively, use:

```
md-toc -d c:\documents -o c:\documents\toc.md -r
```

If the target file already exists, use the `--overwrite` argument like this:

```
md-toc -d c:\documents -o c:\documents\toc.md -r --overwrite
```

Note that you can do both.

```
md-toc -f c:\documents\myfile.md -d c:\documents -o c:\documents\toc.md -r --overwrite
```
-----
### Supported markdown file extensions

- .md
- .markdn
- .markdown
- .mdown
- .mdtext
- .mdtxt
- .mdwn
- .mkd
- .mkdn
- .mmd
- .text

-----

### Output from md-toc.exe --help

```
md-toc [-f|--file <file name>] [-d|--directory <directory name>] [-r|--recursive] [-o|--output-file <file name>] [--overwrite] [-v|--verbose] [-h|-?|?|--help]

[-f|--file <file name>]                 Add a table of contents to the specified file.
[-d|--directory <directory name>]       Find all markdown file in the specified directory.
[-r|--recursive]                        Make directory search recursive.
[-o|--output-file <file name>]          Write output to the specified file.
[--overwrite]                           Overwrite the output file if it already exists.
[-v|--verbose]                          Write details to console.
```
-----

### Notes on possible future enhancements

1. It occurs to me that it would be nice to have multiple -f arguments. Right now you can only have one.
1. It might also be nice to use the -f and the -d together to create an internal TOC for each markdown file in a directory.
