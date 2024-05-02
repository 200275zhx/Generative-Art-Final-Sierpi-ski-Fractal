using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CellularAutomata_3D : MonoBehaviour
{
    public GameObject cellPrefab;
    public int maxSize = 4;  // Controls the depth of the fractal recursion
    public float delay = 1f;  // Delay between updates
    public float attractionForce = 10.0f;   // Attraction to Original Position

    private Dictionary<Vector3Int, GameObject> activeCells;
    private List<GameObject> pool;
    private bool isExpanding = true;
    private int currentLevel = 1;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        activeCells = new Dictionary<Vector3Int, GameObject>();
        pool = new List<GameObject>();
        InitializePool(CalculatePoolSize(maxSize));
        StartCoroutine(IterateFractal());
    }

/*    void Update()
    {
        foreach (var cell in activeCells.Keys)
        {
            Vector3 direction = cell - activeCells[cell].transform.position; // Calculate direction vector
            rb.AddForce(direction.normalized * attractionForce); // Apply force towards the target
        }
    }*/

    int CalculatePoolSize(int maxSize) { return (int)Mathf.Pow(8, maxSize); } // Each level can increase the number of cells eightfold in a Sierpinski fractal.

    void InitializePool(int capacity)
    {
        for (int i = 0; i < capacity; i++)
        {
            GameObject obj = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, this.transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    IEnumerator IterateFractal()
    {
        Vector3Int origin = new Vector3Int(0, 0, 0);
        ManageFractal(origin, currentLevel, isExpanding);

        while (true)
        {
            yield return new WaitForSeconds(delay);

            ManageFractal(origin, currentLevel, isExpanding);

            if (isExpanding){
                if (currentLevel < maxSize) { currentLevel++; }
                else {
                    isExpanding = false;
                    currentLevel--;
                }
            }
            else {
                if (currentLevel > 1) { currentLevel--; }
                else {
                    isExpanding = true;
                    currentLevel++;
                }
            }
        }
    }

    void ManageFractal(Vector3Int origin, int level, bool activating)
    {
        float cellSize = Mathf.Pow(2, maxSize - level);  // Calculate the size of each cell division
        int numCellsPerSide = (int)Mathf.Pow(2, level - 1);

        for (int x = 0; x < numCellsPerSide; x++)
        {
            for (int y = 0; y < numCellsPerSide; y++)
            {
                for (int z = 0; z < numCellsPerSide; z++)
                {
                    if ((x % 2 == 1) ^ (y % 2 == 1) ^ (z % 2 == 1)) continue;  // Skip the cells that form the holes in a Sierpinski fractal
                    Vector3Int pos = origin + new Vector3Int(x, y, z) * (int)cellSize;
                    if (activating)
                        ActivateCell(pos);
                    else
                        DeactivateCell(pos);
                }
            }
        }
    }

    void ActivateCell(Vector3Int position)
    {
        if (!activeCells.ContainsKey(position))
        {
            GameObject cell = GetPooledObject();
            if (cell != null)
            {
                cell.transform.position = position;
                cell.SetActive(true);
                activeCells[position] = cell;
            }
        }
        else
        {
            // Ensure the cell is visible if it already exists
            GameObject cell = activeCells[position];
            cell.SetActive(true);
        }
    }

    void DeactivateCell(Vector3Int position)
    {
        if (activeCells.TryGetValue(position, out GameObject cell))
        {
            cell.SetActive(false);
        }
    }

    GameObject GetPooledObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null; // All objects are in use
    }
}
