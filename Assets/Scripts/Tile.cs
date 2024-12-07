using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private List<Tile> neighbors = new List<Tile>();

    public List<Tile> GetNeighbors()
    {
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        // Optional: Visualize neighbors in the editor
        Gizmos.color = Color.green;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}
