using System.Runtime.InteropServices;

public class FileProcessor
{
    public static void Main(string[] args)
    {
        foreach (string path in args)
        {
            if (File.Exists(path))
            {
                ProcessFile(path);
            }
            else if (Directory.Exists(path))
            {
                ProcessDirectory(path);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
            }
        }
    }

    // Process all files in the directory passed in, recurse on any directories
    // that are found, and process the files they contain.
    public static void ProcessDirectory(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
            ProcessFile(fileName);

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory);
    }


    public static void ProcessFile(string path)
    {
        Console.WriteLine(path);
        var file = new Span<byte>(File.ReadAllBytes(path));
        string filename = Path.GetFileName(path);
        string directory = Path.GetDirectoryName(path);
        var magicNumber = file.Slice(0, 4);

        if (!filename.Contains(".header") && System.Text.Encoding.Default.GetString(magicNumber) == "DSRW")
        {
            var numberOfImagesSlice = file.Slice(4, 4);
            numberOfImagesSlice.Reverse();
            var numberOfImages = BitConverter.ToInt32(numberOfImagesSlice);
            var headerStartSlice = file.Slice(8, 4);
            var headerStart = BitConverter.ToInt32(headerStartSlice);

            var filesStartSlice = file.Slice(12, 4);
            var filesStart = BitConverter.ToInt32(filesStartSlice);

            var headerFile = file.Slice(0, filesStart);
            ByteArrayToFile(directory + "/" + filename + ".header", headerFile);

            List<int> filesPositions = new();
            for (var i = 0; i < numberOfImages * 4; i=i+4)
            {
                var positionSlice = file.Slice(headerStart + i, 4);
                var position = BitConverter.ToInt32(positionSlice);
                filesPositions.Add(position);

            }

            Console.WriteLine("positions " + filesPositions.Count);

            for (int i = 0; i < filesPositions.Count; i++)
            {
                var fileStart = filesStart + filesPositions[i];
                int fileEnd;
                if (i == filesPositions.Count - 1)
                {
                    fileEnd = file.Length;

                }
                else
                {
                    fileEnd = filesStart + filesPositions[i + 1];

                }
                var fileSplit = file.Slice(fileStart, fileEnd - fileStart);
                ByteArrayToFile(directory + "/" + filename + "." + i + ".dds", fileSplit);
            }
        }
        
    }

    public static byte[] SliceArray(byte[] data, int index, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

    public static bool ByteArrayToFile(string fileName, ReadOnlySpan<byte> byteArray)
    {
        try
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught in process: {0}", ex);
            return false;
        }
    }
    public static bool ByteArrayToFile(string fileName, byte[] byteArray)
    {
        try
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught in process: {0}", ex);
            return false;
        }
    }

}