using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Provides utility methods for working with files.
    /// </summary>
    public class FileIO
    {
        /// <summary>
        /// Writes a string buffer to a file.
        /// </summary>
        /// <param name="buffer">The string buffer to write to the file.</param>
        /// <param name="file">The file path to write the buffer to.</param>
        /// <returns>True if the write was successful, false otherwise.</returns>
        public static bool WriteToFile(string buffer, string file)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.Write(buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes a byte array to a file.
        /// </summary>
        /// <param name="buffer">The byte array to write to the file.</param>
        /// <param name="file">The file path to write the buffer to.</param>
        /// <returns>True if the write was successful, false otherwise.</returns>
        public static bool WriteBytesToFile(byte[] buffer, string file)
        {
            try
            {
                File.WriteAllBytes(file, buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the contents of a file as a string.
        /// </summary>
        /// <param name="file">The file path to read from.</param>
        /// <returns>The contents of the file as a string.</returns>
        public static string ReadFromFile(string file)
        {
            string buffer = null;

            try
            {
                using (StreamReader reader = new StreamReader(file))
                    buffer = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return buffer;
        }

        /// <summary>
        /// Reads the contents of a file as a byte array.
        /// </summary>
        /// <param name="file">The file path to read from.</param>
        /// <returns>The contents of the file as a byte array.</returns>
        public static byte[] ReadBytesFromFile(string file)
        {
            byte[] buffer = null;

            try
            {
                buffer = File.ReadAllBytes(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return buffer;
        }

        /// <summary>
        /// Ensures that a directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directory">The directory path to ensure exists.</param>
        /// <returns>True if the directory exists or was created successfully, false otherwise.</returns>
        public static bool AssertDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}