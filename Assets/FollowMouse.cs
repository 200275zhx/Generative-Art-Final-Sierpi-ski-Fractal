using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public GameObject prefabToInstantiate; // Assign the prefab in the Unity Editor

    private GameObject instantiatedPrefab; // Reference to the instantiated prefab

    void Start()
    {
        // Instantiate the prefab at the start
        Vector3 mousePosition = GetMouseWorldPosition();
        instantiatedPrefab = Instantiate(prefabToInstantiate, mousePosition, Quaternion.identity);
    }

    void Update()
    {
        // Move the instantiated prefab to follow the mouse position
        if (instantiatedPrefab != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            instantiatedPrefab.transform.position = mousePosition;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f; // Distance from the camera to the object
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
