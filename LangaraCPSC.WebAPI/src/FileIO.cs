using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LangaraCPSC.WebAPI;

public class FileIO
{
    public static bool WriteToFile(string buffer, string file)
    {
        try
        {
            using (StreamWriter writer  = new StreamWriter(file))
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
