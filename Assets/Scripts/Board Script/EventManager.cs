using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [SerializeField] private Camera arCamera; // Reference to the AR camera

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


// Separate method to handle the fight or movement
    public IEnumerator HandleFightOrMove(bool choice, int targetPlayerID, int currentPlayerID) {
        if (choice) {
            Debug.Log("Player chose to fight.");
            yield return StartCoroutine(HandleFight(targetPlayerID, currentPlayerID));
        } else {
            Debug.Log("Player chose to continue moving.");
        }
    }


    public IEnumerator HandleFight(int opponentPlayerID, int currentPlayerID)
    {
        // Record original positions
        GameObject currentPlayer = PlayerManager.Instance.GetPlayerByID(currentPlayerID).gameObject;
        GameObject opponentPlayer = PlayerManager.Instance.GetPlayerByID(opponentPlayerID).gameObject;
        Vector3 originalCurrentPlayerPosition = currentPlayer.transform.position;
        Vector3 originalOpponentPlayerPosition = opponentPlayer.transform.position;

        // Move camera to fighting area
        arCamera.transform.position = new Vector3(30, 9, -9);
        //arCamera.transform.LookAt(new Vector3(30, 0, 0));

        // Move player models to fighting area
        currentPlayer.transform.position = new Vector3(29, 2, 0);
        opponentPlayer.transform.position = new Vector3(28, 2, 0);

        // Implement fighting logic here
        yield return new WaitForSeconds(3); // Simulate fight duration

        // Move camera back to board
        arCamera.transform.position = new Vector3(0, 9, -9);
        //arCamera.transform.LookAt(new Vector3(0, -1, 0));

        // Move player models back to their original positions
        currentPlayer.transform.position = originalCurrentPlayerPosition;
        opponentPlayer.transform.position = originalOpponentPlayerPosition;

        // Handle fight result (e.g., update player health, determine winner, etc.)
        Debug.Log("Fight ended. Handle fight result here.");
    }
}