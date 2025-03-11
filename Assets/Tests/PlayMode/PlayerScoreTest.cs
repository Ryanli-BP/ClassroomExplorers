using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

// If your Player script is in a namespace, import it
// using MyGameNamespace;

public class PlayerScoreTest
{
    private GameObject playerObject;
    private Player player;

    [SetUp]
    public void Setup()
    {
        // Create a GameObject and attach the Player script
        playerObject = new GameObject();
        player = playerObject.AddComponent<Player>();  // ✅ Ensures Player is recognized
    }

    [UnityTest] // Play Mode test
    public IEnumerator Player_Gains_Score()
    {
        player.AddScore(10);
        yield return null; // Wait a frame
        Assert.AreEqual(10, player.Score);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
    }
}
