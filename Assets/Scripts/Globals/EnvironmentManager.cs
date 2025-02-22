using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [SerializeField] private Material boardPlaneMaterial;
    [SerializeField] private GameObject quizUIObject;
    
    [SerializeField] private string boardPlaneMaterialUrl = "http://127.0.0.1:8000/api/v1.0.0/assets/material-texture/";
    [SerializeField] private string QuizUIImageUrl = "http://127.0.0.1:8000/api/v1.0.0/assets/ui-image/";
    private Image quizUIImage;
    private bool textureDownloadComplete = false;
    private Sprite downloadedSprite;

    private void Awake()
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

        // Get the Image component from the GameObject
        if (quizUIObject != null)
        {
            quizUIImage = quizUIObject.GetComponent<Image>();
            if (quizUIImage == null)
            {
                Debug.LogWarning("No Image component found on quizUIObject. Adding one...");
                quizUIImage = quizUIObject.AddComponent<Image>();
            }
        }
    }

    void Start()
    {
        StartCoroutine(LoadEnvironmentAssets());
    }

    private void Update()
    {
        // Apply the sprite when both the component exists and texture is ready
        if (textureDownloadComplete && downloadedSprite != null && quizUIImage != null)
        {
            quizUIImage.sprite = downloadedSprite;
            textureDownloadComplete = false; // Reset flag after applying
            downloadedSprite = null;
        }
    }

    private IEnumerator LoadEnvironmentAssets()
    {
        bool materialDownloadComplete = false;
        bool uiImageDownloadComplete = false;

        // Download material texture URL
        NetworkManager.Instance.GetRequest(boardPlaneMaterialUrl,
            (textureUrl) => {
                StartCoroutine(ProcessTextureResponse(textureUrl, true));
                materialDownloadComplete = true;
            },
            (error) => {
                Debug.LogWarning($"Failed to get material texture URL: {error}");
                materialDownloadComplete = true;
            });

        // Download UI image URL
        NetworkManager.Instance.GetRequest(QuizUIImageUrl,
            (imageUrl) => {
                StartCoroutine(ProcessTextureResponse(imageUrl, false));
                uiImageDownloadComplete = true;
            },
            (error) => {
                Debug.LogWarning($"Failed to get UI image URL: {error}");
                uiImageDownloadComplete = true;
            });

        // Wait for both downloads to complete
        while (!materialDownloadComplete || !uiImageDownloadComplete)
        {
            yield return null;
        }

        Debug.Log("Environment assets loaded successfully");
    }

    private IEnumerator ProcessTextureResponse(string textureUrl, bool isMaterial)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to process texture: {www.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            if (isMaterial && boardPlaneMaterial != null)
            {
                boardPlaneMaterial.mainTexture = texture;
            }
            else if (!isMaterial)
            {
                downloadedSprite = Sprite.Create(texture, 
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                textureDownloadComplete = true;
            }
        }
    }
}