using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CredentialsHelper
{
    public static Dictionary<string, string> GetDictionary(TextAsset textAsset)
    {
        string plainText = textAsset.text;

        string[] fLines = Regex.Split(plainText, "\n |\r |\r\n");

        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        foreach (string entry in fLines)
        {
            string[] pair = Regex.Split(entry, "=");
            dictionary[pair[0]] = pair[1];
        }
        return dictionary;
    }
}
