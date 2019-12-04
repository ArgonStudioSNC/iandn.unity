using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;

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

    public static IEnumerator GetTextureFromZipAsync(string texturePath, string textureName, Uri zipURL, Action<Texture2D, string> callback)
    {
        Texture2D texture = new Texture2D(2, 2);
        string path = Path.Combine(Application.persistentDataPath + texturePath);
        string file = Path.Combine(path + textureName);
        byte[] byteArray;

        if (File.Exists(file))
        {
            Debug.Log("Loading from " + file);
            byteArray = File.ReadAllBytes(file);
            texture.LoadImage(byteArray); // auto-resize texture
            yield return null;
            callback(texture, texture == null ? "Échec du chargement. I&N a t'elle accès au stockage externe?" : "Succès. Texture chargée depuis le cache.");
        }
        else
        {
            Debug.Log("Downloading zip from the web " + zipURL);

            using (UnityWebRequest www = UnityWebRequest.Get(zipURL))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    LoadingSpinScript.Text = "Nous téléchargeons votre galerie photos.\n\n" + (100 * www.downloadProgress).ToString("##\\%");
                    yield return null;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("!Error while downloading the album zip!");
                    yield return null;
                    callback(null, "Échec du chargement. Êtes-vous connecté à internet?");
                }
                else
                {
                    byte[] zipFileData = www.downloadHandler.data;
                    using (MemoryStream albumFileStream = new MemoryStream())
                    {
                        albumFileStream.Write(zipFileData, 0, zipFileData.Length);
                        albumFileStream.Flush();
                        albumFileStream.Seek(0, SeekOrigin.Begin);

                        ZipArchive zipArchive = new ZipArchive(albumFileStream);

                        ValidateDir(path);
                        Debug.Log("Axtracting archive to " + path);
                        zipArchive.ExtractToDirectory(path + "/..");
                    }

                    Debug.Log("Loading from " + file);
                    byteArray = File.ReadAllBytes(file);
                    texture.LoadImage(byteArray); // auto-resize texture
                    yield return null;

                    callback(texture, texture == null ? "Échec du chargement. I&N a t'elle accès au stockage externe?" : "Succès. Texture chargée depuis le cache.");
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
