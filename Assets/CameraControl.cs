using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float speed = 10.0f; // Speed of camera movement
    public float mouseSensitivity = 100.0f; // Mouse sensitivity

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;

    void Update()
    {
        // Camera translation
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput) * speed * Time.deltaTime;
        transform.Translate(movement, Space.Self);

        // Mouse movement input for looking around
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Calculate x-axis rotation to look up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent the camera from flipping

        // Calculate y-axis rotation to look left and right
        yRotation += mouseX;

        // Apply the calculated rotations to the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
