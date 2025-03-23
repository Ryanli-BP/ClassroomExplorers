using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AvatarGenerator : MonoBehaviour
{
    public RenderTexture renderTexture;  // Assign this in Unity Inspector
    [SerializeField] public List<RawImage> avatarDisplays;  // UI Element where avatar is shown
    private Camera avatarCamera;         // Camera that will capture the player's model

    public void GenerateAvatar(GameObject player, int avatarIndex)
    {
        if (avatarCamera == null)
        {
            // Create a new camera if it doesn’t exist
            GameObject camObj = new GameObject("AvatarCamera");
            avatarCamera = camObj.AddComponent<Camera>();
            avatarCamera.clearFlags = CameraClearFlags.SolidColor;  // Ensure background is solid (we'll make it transparent)
            avatarCamera.backgroundColor = Color.clear;  // Transparent background
            avatarCamera.orthographic = false;  // Use perspective for better 3D effect
            avatarCamera.targetTexture = renderTexture;
            
            avatarCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Background"));
        }

        if (GameObject.Find("AvatarLight") == null) // Avoid creating multiple lights
        {
            GameObject lightObj = new GameObject("AvatarLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(30, 30, 0); // Angle the light
            light.intensity = 1.5f;  // Increase brightness if too dim
        }

        // Get player's renderer bounds to center the camera correctly
        Renderer playerRenderer = player.GetComponentInChildren<Renderer>();
        if (playerRenderer == null)
        {
            Debug.LogError("Player has no renderer!");
            return;
        }

        Bounds bounds = playerRenderer.bounds;

        // Calculate best camera position (slightly in front)
        Vector3 cameraPosition = bounds.center + new Vector3(0, 0f, -1.5f); // Adjust as needed
        avatarCamera.transform.position = cameraPosition;
        avatarCamera.transform.LookAt(bounds.center + Vector3.up * 0f); // Focus on upper body

        // Render the image
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        avatarCamera.Render();
        RenderTexture.active = null;

        // Convert the RenderTexture to a UI image
        Texture2D avatarTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        avatarTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        avatarTexture.Apply();
        RenderTexture.active = null;

        // Ensure the avatarDisplays list has enough elements
        if (avatarDisplays.Count > avatarIndex)
        {
            // Assign the generated texture to the correct RawImage UI element
            avatarDisplays[avatarIndex].texture = avatarTexture;
        }
        else
        {
            Debug.LogError("Avatar display index is out of bounds.");
        }

        Debug.Log($"Avatar generated for player {avatarIndex + 1}");
    }
}
