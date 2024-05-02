using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IterativeFractal : MonoBehaviour
{
    public int initialSize = 4;  // Initial size of the fractal grid
    public GameObject cellPrefab;
    public int maxRecursionLevel = 4;  // Maximum depth of recursion
    private int currentRecursionLevel = 0;  // Current depth of recursion

    private List<GameObject> pool;
    private Dictionary<Vector3Int, GameObject> activeCells;

    void Start()
    {
        pool = new List<GameObject>();
        activeCells = new Dictionary<Vector3Int, GameObject>();
        InitializePool();
        StartCoroutine(GrowFractal());
    }

    void InitializePool()
    {
        float estimatedMaxCells = Mathf.Pow(4, maxRecursionLevel);  // Estimating the number of cells needed
        for (int i = 0; i < estimatedMaxCells; i++)
        {
            GameObject obj = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, this.transform);
            obj.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);  // Initially invisible
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    IEnumerator GrowFractal()
    {
        Vector3Int origin = new Vector3Int(0, 0, 0);
        while (currentRecursionLevel <= maxRecursionLevel)
        {
            GenerateFractal(origin, initialSize, currentRecursionLevel);
            currentRecursionLevel++;
            yield return new WaitForSeconds(1);  // Wait for 1 second before next iteration
        }
    }

    void GenerateFractal(Vector3Int origin, int size, int level)
    {
        if (level == 0)
        {
            PlaceActiveCell(origin);
        }
        else
        {
            int newSize = size / 2;
            GenerateFractal(origin, newSize, level - 1);
            GenerateFractal(origin + new Vector3Int(newSize, 0, 0), newSize, level - 1);
            GenerateFractal(origin + new Vector3Int(0, newSize, 0), newSize, level - 1);
            GenerateFractal(origin + new Vector3Int(0, 0, newSize), newSize, level - 1);
        }
    }

    void PlaceActiveCell(Vector3Int position)
    {
        GameObject cell = GetPooledObject();
        if (cell != null)
        {
            cell.transform.position = position;
            cell.SetActive(true);
            cell.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);  // Make visible
            activeCells[position] = cell;
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
        return null;
    }
}
