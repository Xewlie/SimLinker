# SimLinker

SimLinker is a Windows tool for mass hard/soft symbolic linking or copying entire directories (and subdirectories) to a target folder.
Files are done in parallel asynchronously.

For example, you can soft/hard link 250 thousand files even on a HDD within about 20-30 seconds.

This was initially created as a way to mirror an entire game development repo for build operations, which requires a majority of files to be hard linked, but a few folders soft linked, and couple of files copied.

Created using .NET 7.0

# You spelt SymLinker wro--

NO, THATS THE JOKE, LEAVE ME ALONE

# Usage

SimLinker.exe <srcDir> <targetDir> -softlink/-hardlink/-copy

    <srcDir>: The source directory to copy or link from.
    <targetDir>: The target directory to copy or link to.
    -softlink: Soft links the source files to the target directory.
    -hardlink: Hard links the source files to the target directory.
    -copy: Copies the source files to the target directory.

# Example

SimLinker.exe C:\Source\Directory C:\Target\Directory -hardlink
