using System;
using System.IO;
using UnityEngine;

public static class PersistentToken
{
    [Serializable]
    internal class Token
    {
        public bool isLogged;
        public bool hasAccessToMeal;
        public bool isRegistred;

        public Token(bool isLogged, bool hasAccessToMeal, bool isRegistred)
        {
            this.isLogged = isLogged;
            this.hasAccessToMeal = hasAccessToMeal;
            this.isRegistred = isRegistred;
        }
    }

    private static Token token = new Token(false, false, false);
    private static string tokenName = "token";

    static PersistentToken()
    {
        LoadToken();
    }

    public static bool IsLogged()
    {
        return token.isLogged;
    }

    public static bool hasAccessToMeal()
    {
        return token.hasAccessToMeal;
    }

    public static bool IsRegistred()
    {
        return token.isRegistred;
    }

    public static void SetLogged(bool s)
    {
        token.isLogged = s;
        SaveToken();
    }

    public static void SetMealAccess(bool s)
    {
        token.hasAccessToMeal = s;
        SaveToken();
    }

    public static void SetRegistration(bool s)
    {
        token.isRegistred = s;
        SaveToken();
    }

    private static void LoadToken()
    {
        string jsonFile;
        try
        {
            jsonFile = DeserializeData(ResolvePersistentDataPath(tokenName));
            token = JsonUtility.FromJson<Token>(jsonFile);
        } catch (FileNotFoundException)
        {
            Debug.Log("Can't find the file " + tokenName);
            token = new Token(false, false, false);
            SaveToken();
        }
    }

    private static void SaveToken()
    {
        string jsonFile = JsonUtility.ToJson(token);
        SerializeData(ResolvePersistentDataPath(tokenName), jsonFile);
    }

    private static string DeserializeData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("No file at location " + path);
        return File.ReadAllText(path);
    }

    private static void SerializeData(string path, string file)
    {
        try
        {
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
            }
            File.WriteAllText(path, file);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private static  string ResolvePersistentDataPath(string fileName)
    {
        string path = Application.persistentDataPath;
        path = Path.Combine(path, fileName);
        string directories = Path.GetDirectoryName(path);
        if (!Directory.Exists(directories)) Directory.CreateDirectory(directories);

        return path;
    }
}

