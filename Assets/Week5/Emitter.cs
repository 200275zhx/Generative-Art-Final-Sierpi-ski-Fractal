using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    public GameObject selfPrefab;   // Prefab emitting
    public float spawnRate = 2.0f; // Time between spawns
    public float checkRate = 1.0f; // How often to check the frame rate

    public float minFrameRate = 30.0f; // Minimum acceptable frame rate to continue spawning
    public float recoveryFrameRate = 45.0f; // Frame rate above which spawning can resume

    private bool canSpawn = true; // Control whether new instances can be spawned

    void Start()
    {
        StartCoroutine(SpawnCoroutine());
        StartCoroutine(FrameRateCheckCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while (true) // Infinite loop
        {
            if (canSpawn)
            {
                Instantiate(selfPrefab, transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

    IEnumerator FrameRateCheckCoroutine()
    {
        while (true) // Infinite loop
        {
            float currentFrameRate = 1.0f / Time.deltaTime;

            if (currentFrameRate < minFrameRate)
            {
                canSpawn = false;
            }
            else if (currentFrameRate > recoveryFrameRate)
            {
                canSpawn = true;
            }

            yield return new WaitForSeconds(checkRate);
        }
    }
}
