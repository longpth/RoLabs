using OpenCvSharp;
using SkiaSharp;
using System.Diagnostics;

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

        public static async Task<string> SaveAudioFile(Stream inputStream, string filename)
        {
            //string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            //// Copy the file to the AppDataDirectory
            //using FileStream outputStream = File.Create(targetFile);
            //await inputStream.CopyToAsync(outputStream);

            //return targetFile;

            string fileNameToSave;

#if __ANDROID__
            var stream = inputStream;
            //string cacheFolder = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsoluteFile.Path.ToString(); // gives app package in data structure
            string cacheFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsoluteFile.Path.ToString(); //gives general downloads folder
            cacheFolder = cacheFolder + System.IO.Path.DirectorySeparatorChar;

            fileNameToSave = cacheFolder + "audiotemp.wav";
            if (stream != null)
            {

                Directory.CreateDirectory(Path.GetDirectoryName(fileNameToSave));
                if (System.IO.File.Exists(fileNameToSave))
                {
                    System.IO.File.Delete(fileNameToSave); //must delete first or length not working properly
                }
                FileStream fileStream = System.IO.File.Create(fileNameToSave);
                fileStream.Position = 0;
                stream.Position = 0;
                stream.CopyTo(fileStream);
                fileStream.Close();

                Debug.WriteLine("AUDIO FILE SAVED DONE: " + fileNameToSave);
            }
#endif
            return fileNameToSave;

        }


    }
}
