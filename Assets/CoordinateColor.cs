using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateColor : MonoBehaviour
{
    // Reference to the Renderer component of the cube
    private Renderer cubeRenderer;

    // Factor to shrink the color palette
    public float shrinkFactor = 0.1f;

    private void Start()
    {
        // Get the Renderer component attached to the cube
        cubeRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Get the current position of the cube
        Vector3 position = transform.position;

        // Normalize the position values and apply shrink factor
        float r = Mathf.Abs(position.x * shrinkFactor);
        float g = Mathf.Abs(position.y * shrinkFactor);
        float b = Mathf.Abs(position.z * shrinkFactor);

        // Create a color based on the normalized position values
        Color color = new Color(r, g, b);

        // Set the material color to the calculated color
        cubeRenderer.material.color = color;
    }

}
