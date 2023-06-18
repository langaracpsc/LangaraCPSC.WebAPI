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
} 
