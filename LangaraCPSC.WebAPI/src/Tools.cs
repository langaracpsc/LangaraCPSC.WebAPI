using System.Text;
using System.Text.Json;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Provides utility methods for working with JSON and string manipulation.
    /// </summary>
    public class Tools
    {
        /// <summary>
        /// Gets the typed value of a JSON element.
        /// </summary>
        /// <param name="element">The JSON element to get the value from.</param>
        /// <returns>The typed value of the JSON element, or null if the value kind is not supported.</returns>
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

        /// <summary>
        /// Encodes a string to a base64 string.
        /// </summary>
        /// <param name="buffer">The string to encode.</param>
        /// <returns>The base64-encoded string.</returns>
        public static string EncodeToBase64(string buffer)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(buffer));
        }

        /// <summary>
        /// Decodes a base64 string to a regular string.
        /// </summary>
        /// <param name="buffer">The base64-encoded string to decode.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeFromBase64(string buffer)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(buffer));
        }

        /// <summary>
        /// Eliminates a substring from a given string.
        /// </summary>
        /// <param name="str">The string to remove the substring from.</param>
        /// <param name="substr">The substring to remove.</param>
        /// <returns>The string with the substring removed.</returns>
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
}