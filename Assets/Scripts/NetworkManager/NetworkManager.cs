using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

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

    public void DownloadText(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(IDownloadText(url, onSuccess, onError));
    }

    private IEnumerator IDownloadText(string url, System.Action<string> onSuccess, System.Action<string> onError)
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
}