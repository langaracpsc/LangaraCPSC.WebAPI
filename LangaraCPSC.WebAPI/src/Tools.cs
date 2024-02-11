using System.Text;
using System.Text.Json;

namespace LangaraCPSC.WebAPI;

public class Tools
{
    public static object GetTypedJsonElementValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                return element.GetInt64();
            
            default:
                return null;
        }
    }

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