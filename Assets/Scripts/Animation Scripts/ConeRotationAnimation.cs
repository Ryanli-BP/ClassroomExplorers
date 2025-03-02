using UnityEngine;

public class ConeRotationAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 45f; // Degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Rotate around Y-axis by default

    void Update()
    {
        // Rotate the cone around the specified axis at a constant rate
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}