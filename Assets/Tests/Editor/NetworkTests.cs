// Tests/NetworkTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class NetworkTests
{
    [UnityTest]
    public IEnumerator Test_GET_Request()
    {
        // Flag to indicate if the request is done
        bool isDone = false;
        // Flag to indicate if the request was successful
        bool isSuccess = false;
        
        // Initiate a GET request using the NetworkManager
        NetworkManager.Instance.GetRequest("http://localhost:9000/generate/?topic=Business&number=1&ageGroup=12-15&item=head%20phones",
            response => {
                // If the request is successful, set isSuccess to true
                isSuccess = true;
                // Mark the request as done
                isDone = true;
            },
            error => {
                // If the request encounters an error, set isSuccess to false
                isSuccess = false;
                // Mark the request as done
                isDone = true;
            }
        );

        // Wait for the request to complete (maximum 10 seconds)
        float timeout = 10f;
        float startTime = Time.time;
        while (!isDone && Time.time - startTime < timeout)
        {
            // Yield control back to the Unity engine and wait for the next frame
            yield return null;
        }

        // Assert that the request was successful
        Assert.IsTrue(isSuccess, "GET request failed");
    }
}