using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(IGetRequest(url, onSuccess, onError));
    }

    private IEnumerator IGetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke($"Error: {webRequest.error} | Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    public void DownloadFile(string url, string savePath, System.Action onSuccess, System.Action<string> onError)
    {
        StartCoroutine(IDownloadFile(url, savePath, onSuccess, onError));
    }

    private IEnumerator IDownloadFile(string url, string savePath, System.Action onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke($"Error: {webRequest.error}");
            }
            else
            {
                try
                {
                    File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
                    onSuccess?.Invoke();
                }
                catch (System.Exception e)
                {
                    onError?.Invoke($"Error saving file: {e.Message}");
                }
            }
        }
    }
}