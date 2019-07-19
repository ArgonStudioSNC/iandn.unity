using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class ImageDownloader
{
    public static IEnumerator GetTextureAsync(string texturePath, Uri textureURL, Action<Texture2D, string> callbackTexture)
    {
        Texture2D texture = new Texture2D(2, 2);
        string path = Path.Combine(Application.persistentDataPath + texturePath);
        byte[] byteArray;

        if (File.Exists(path))
        {
            Debug.Log("Loading from " + path);
            byteArray = File.ReadAllBytes(path);
            texture.LoadImage(byteArray); // auto-resize texture
            yield return null;
            callbackTexture(texture, texture == null ? "Échec du chargement. I&N a t'elle accès au stockage externe?" : "Succès. Texture chargée depuis le cache.");
        }
        else
        {
            Debug.Log("Downloading from the web " + textureURL);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureURL);
            using (www)
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    yield return null;
                    callbackTexture(null, "Échec du chargement. Êtes-vous connecté à internet?");
                }
                else
                {
                    texture = DownloadHandlerTexture.GetContent(www);
                    byteArray = texture.EncodeToJPG();
                    ValidateDir(path);
                    File.WriteAllBytes(path, byteArray);
                    yield return null;
                    callbackTexture(texture, "Succès. Texture chargée depuis internet.");
                }
            }
        }
    }

    public static IEnumerator GetBytesAsync(Uri url, Action<byte[], string> file)
    {
        byte[] byteArray;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                yield return null;
                file(null, "Échec du chargement. Êtes-vous connecté à internet?");
            }
            else
            {
                byteArray = www.downloadHandler.data;

                yield return null;
                file(byteArray, "Succès. Données chargées depuis internet.");
            }
        }
    }

    private static void ValidateDir(string path)
    {
        string directories = Path.GetDirectoryName(path);
        if (!Directory.Exists(directories)) Directory.CreateDirectory(directories);
    }
}
