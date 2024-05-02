using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public List<GameObject> targetPrefabs;
    public List<GameObject> evadePrefabs;
    public List<string> chaseTag;
    public List<string> evadeTag;

    public float attractionForce = 10.0f;
    public float repellingForce = 15.0f; // Force applied to evade each other
    public float evadeDistance = 5.0f; // Distance detect evading target
    public float lifespan = 5.0f;
    public float bufferTime = 3.0f; // After how long can a cell instantiate another cells and being destroy by other cells
    public float repulseRadius = 5.0f; // Area to apply force when instantiate a new cell
    public float repulseForce = 5.0f; // Force to apply to all other cell that are not with same tag within the repulseRadius when instantiate a new cell

    private bool isProtected = true; // Can a cell instantiate another cells and being destroy by other cells
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifespan);
        StartCoroutine(Cooldown(bufferTime));
    }

    private void FixedUpdate()
    {
        MoveTowardsTargets();
        EvadeEvaders();
    }

    private void MoveTowardsTargets()
    {
        foreach (var target in targetPrefabs)
        {
            if (target != null)
            {
                Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
                rb.AddForce(directionToTarget * attractionForce, ForceMode.Acceleration);
            }
        }
    }

    private void EvadeEvaders()
    {
        foreach (var evade in evadePrefabs)
        {
            if (evade != null)
            {
                Vector3 directionToEvade = (evade.transform.position - transform.position);
                if (directionToEvade.magnitude < evadeDistance)
                {
                    Vector3 evadeDirection = (transform.position - evade.transform.position).normalized;
                    rb.AddForce(evadeDirection * repellingForce, ForceMode.Acceleration);
                }
            }
        }
    }

    private IEnumerator Cooldown(float buffer)
    {
        yield return new WaitForSeconds(buffer);
        isProtected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isProtected)
        {
            // Check if the collider's tag is in the list of evade tags and destroy this object if it is
            if (evadeTag.Contains(other.tag))
            {
                Destroy(gameObject);
            }

            // Check if the collider's tag is in the list of chase tags, instantiate at collider's position, and apply force
            if (chaseTag.Contains(other.tag))
            {
                Instantiate(gameObject, other.transform.position, other.transform.rotation);
                ApplyForceToNearbyObjects(other);
            }
        }
    }


    void ApplyForceToNearbyObjects(Collider instantiatedCollider)
    {
        Collider[] colliders = Physics.OverlapSphere(instantiatedCollider.transform.position, repulseRadius);
        foreach (Collider hit in colliders)
        {
            if (!hit.gameObject.CompareTag(gameObject.tag))  // Only apply force to objects with a different tag
            {
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = hit.transform.position - instantiatedCollider.transform.position;
                    rb.AddForce(direction.normalized * repulseForce, ForceMode.Impulse);
                }
            }
        }
    }
}
