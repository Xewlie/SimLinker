using System.Runtime.InteropServices;

namespace SimLinker
{
    public abstract class Program
    {
        // DLL imports for creating symbolic links and hard links
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        // Func delegates for creating symbolic links and hard links
        private static readonly Func<string, string, SymbolicLink, bool> symbolicLink =
            (target, origin, flags) => CreateSymbolicLink(origin, target, flags);
        
        private static readonly Func<string, string, IntPtr, bool> hardLink =
            (target, origin, lpSecurityAttributes) => CreateHardLink(origin, target, lpSecurityAttributes);
        
        public static void Main(string[] args)
        {
            if (args?.Length != 3)
            {
                Console.WriteLine("Usage: <srcDir> <targetDir> -softlink / -hardlink / -copy");
                return;
            }
            
            var srcDirectory = args[0];
            var targetDirectory = args[1];
            var operation = args[2].ToLowerInvariant();
            
            var mode = operation switch
            {
                "-softlink" => Mode.Link,
                "-copy" => Mode.Copy,
                "-hardlink" => Mode.Hardlink,
                _ => throw new ArgumentException("Invalid operation. Use -softlink, -copy, or -hardlink.")
            };

            // Check if the source directory exists and if it is a directory
            var srcDirAttributes = File.GetAttributes(srcDirectory);
            if ((srcDirAttributes & FileAttributes.Directory) == 0 || !Directory.Exists(srcDirectory))
            {
                switch (mode)
                {
                    case Mode.Copy:
                        Console.WriteLine("Copying file");
                        File.Copy(srcDirectory, targetDirectory);
                        break;
                    case Mode.Link:
                        Console.WriteLine("Softlinking file");
                        symbolicLink(srcDirectory, targetDirectory, SymbolicLink.File);
                        break;
                    case Mode.Hardlink:
                        Console.WriteLine("Hardlinking file");
                        hardLink(srcDirectory, targetDirectory, IntPtr.Zero);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }

            // Check if the target directory exists and create it if it doesn't
            try
            {
                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);
            }
            catch
            {
                Console.WriteLine($"Could not create directory {targetDirectory}");
                return;
            }
            
            Action<string, string> perItemDelegate = mode switch
            {
                Mode.Link => (srcFile, tarFile) => symbolicLink(srcFile, tarFile, SymbolicLink.File),
                Mode.Copy => (srcFile, tarFile) => File.Copy(srcFile, tarFile, true),
                Mode.Hardlink => (srcFile, tarFile) => hardLink(srcFile, tarFile, IntPtr.Zero),
                _ => throw new ArgumentOutOfRangeException()
            };

            // Enumerate over all files in the source directory and its subdirectories
            var files = Directory.EnumerateFiles(srcDirectory, "*", SearchOption.AllDirectories);
            Parallel.ForEach(files, f =>
            {
                var relativePath = Path.GetRelativePath(srcDirectory, f);
                var targetPath = Path.Combine(targetDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException());
                perItemDelegate(f, targetPath);
            });

            Console.WriteLine($"Done! Operation completed from {srcDirectory} to {targetDirectory}");
        }


        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        private enum Mode
        {
            Link,
            Copy,
            Hardlink
        }
    }
}
