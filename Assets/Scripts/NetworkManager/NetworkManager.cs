using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    // Singleton pattern for global access
    public static NetworkManager Instance;

    void Awake()
    {
        // Check if an instance of NetworkManager already exists
        if (Instance == null)
        {
            // If not, set this instance as the singleton instance
            Instance = this;
            // Prevent this object from being destroyed when loading a new scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this object to enforce the singleton pattern
            Destroy(gameObject);
        }
    }

    // Method to initiate a GET request
    // url: The URL to send the GET request to
    // onSuccess: Callback to invoke if the request is successful
    // onError: Callback to invoke if the request encounters an error
    public void GetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        // Start the coroutine to handle the GET request
        StartCoroutine(IGetRequest(url, onSuccess, onError));
    }

    // Coroutine to handle the GET request
    // url: The URL to send the GET request to
    // onSuccess: Callback to invoke if the request is successful
    // onError: Callback to invoke if the request encounters an error
    private IEnumerator IGetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        // Create a UnityWebRequest for the specified URL
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the web request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check if there was a connection error or protocol error
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Invoke the onError callback with the error message and response text
                onError?.Invoke($"Error: {webRequest.error} | Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                // Invoke the onSuccess callback with the response text
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }
}