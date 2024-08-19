using OpenCvSharp;
using SkiaSharp;

namespace Rolabs.MVVM.Helpers
{
    public class Utils
    {
        public static async Task<string> CopyFileToAppDataDirectory(string filename)
        {
            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Create an output filename
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
            return targetFile;
        }

        public static async Task<string> CopyFileToAppDataDirectory2(string filename)
        {
            // Create the target file path in the AppDataDirectory
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            // Check if the file already exists
            if (File.Exists(targetFile))
            {
                // If the file exists, return the path without copying
                return targetFile;
            }

            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);

            return targetFile;
        }


    }
}
