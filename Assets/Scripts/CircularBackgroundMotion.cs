using UnityEngine;

public class CircularBackgroundMotion : MonoBehaviour
{
    // Radius of the circular motion
    public float radius = 1.0f;

    // Speed of the circular motion
    public float speed = 0.5f;

    // Initial position of the background
    private Vector3 initialPosition;

    void Start()
    {
        // Save the initial position of the object
        initialPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new position in a circular motion
        float x = Mathf.Cos(Time.time * speed) * radius * 1.5f;
        float y = Mathf.Sin(Time.time * speed) * radius;

        // Apply the new position while keeping the original Z
        transform.position = initialPosition + new Vector3(x, y, 0);
    }
}
