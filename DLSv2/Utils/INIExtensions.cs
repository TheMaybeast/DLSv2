using System.Collections.Generic;
using System.Globalization;
using Rage;
using static System.String;

namespace DLSv2.Utils;

public static class INIExtensions
{
    public delegate bool TryParseDelegate<U>(string input, out U output);

    public static T[] ReadArray<T>(this InitializationFile File, string sectionName, string keyName, T[] defaultValue, TryParseDelegate<T> convertFunc, char splitOn = ',', bool stripWhitespace = true)
    {
        var rawEntry = File.ReadString(sectionName, keyName, "");
        if (IsNullOrWhiteSpace(rawEntry)) return defaultValue;

        var convertedItems = new List<T>();
        var splitString = rawEntry.Split(splitOn);
        if (splitString.Length <= 0) return convertedItems.ToArray();

        foreach (var itemStr in splitString)
        {
            var input = itemStr;
            if (stripWhitespace)
            {
                input = itemStr.Trim();
            }

            var success = convertFunc(input, out var convertedItem);
            if (success)
            {
                convertedItems.Add(convertedItem);
            }
            else
            {
                return defaultValue;
            }
        }

        return convertedItems.ToArray();
    }

    public static int[] ReadIntArray(this InitializationFile File, string sectionName, string KeyName, int[] defaultValue)
    {
        return File.ReadArray<int>(sectionName, KeyName, defaultValue,
            (string s, out int result) =>
                int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result));
    }

    public static float[] ReadFloatArray(this InitializationFile File, string sectionName, string KeyName, float[] defaultValue)
    {
        return File.ReadArray<float>(sectionName, KeyName, defaultValue,
            (string s, out float result) =>
                float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result));
    }

    public static string[] ReadStringArray(this InitializationFile File, string sectionName, string KeyName, string[] defaultValue)
    {
        return File.ReadArray<string>(sectionName, KeyName, defaultValue, (string s, out string result) =>
        {
            result = s;
            return true;
        });
    }
}