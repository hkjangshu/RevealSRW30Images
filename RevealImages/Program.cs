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
        int hexIn;
        String hex;
        int position = -1;
        byte[] file = File.ReadAllBytes(path);
        string filename = Path.GetFileName(path);
        string directory = Path.GetDirectoryName(path);

        for (int i = 0; i < file.Length; i++)
        {
            hexIn = file[i];
            hex = string.Format("{0:X2}", hexIn);
            if (position > 0)
            {
                if (hex == "44")
                {
                    byte[] header = SliceArray(file, 0, position - 1);
                    byte[] image = SliceArray(file, position, file.Length - position);
                    ByteArrayToFile(directory + "/" + filename + ".header.bin", header);
                    ByteArrayToFile(directory + "/" + filename + ".dds", image);
                    break;
                }
                else
                {
                    position = -1;
                }
            }
            else
            {
                if (hex == "44")
                {
                    position = i;
                }
            }
        }
    }

    public static byte[] SliceArray(byte[] data, int index, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(data, index, result, 0, length);
        return result;
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