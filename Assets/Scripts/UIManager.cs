using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject directionPanel; // Panel that contains direction buttons

    [SerializeField]
    private Button directionButtonPrefab; // Prefab for a direction button

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Show direction choices at a crossroad
    public void ShowDirectionChoices(List<Tile> neighbors, Action<Tile> onDirectionChosen)
    {
        // Activate the panel
        directionPanel.SetActive(true);

        // Clear any existing buttons
        foreach (Transform child in directionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each neighbor
        foreach (Tile neighbor in neighbors)
        {
            Button newButton = Instantiate(directionButtonPrefab, directionPanel.transform);
            newButton.GetComponentInChildren<Text>().text = neighbor.name; // Set button label

            // Add click listener to notify when this direction is chosen
            newButton.onClick.AddListener(() =>
            {
                directionPanel.SetActive(false); // Hide the panel
                onDirectionChosen(neighbor); // Notify the PlayerMovement script
            });
        }
    }
}
