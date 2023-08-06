using System.Text;

namespace LangaraCPSC.WebAPI;

public class Tools
{
    public static string EncodeToBase64(string buffer)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(buffer));
    }

    public static string DecodeFromBase64(string buffer)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(buffer));
    }

    public static string EliminateSubstring(string str, string substr)
    {
        string alteredString = "";

        for (int x = 0; x < str.Length; x++)
        {
            if (str.Substring(x, substr.Length) == substr)
            {
                x += substr.Length; 
                continue;
            }

            alteredString += str[x];
        }
   
        return alteredString; 
    }
}