using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalRecursive : MonoBehaviour
{
    public float bdry = 0;
    public GameObject cellPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IterateFractal(Vector3Int root, Vector3Int size)
    {
        if (size.x > bdry) { return; }

        Node node = new Node(root);
        //adding children
        {
            node.children.Add(new Node(root + new Vector3Int(size.x, size.x, size.x)));
            node.children.Add(new Node(root + new Vector3Int(size.x, size.x, -size.x)));
            node.children.Add(new Node(root + new Vector3Int(-size.x, size.x, size.x)));
            node.children.Add(new Node(root + new Vector3Int(-size.x, size.x, -size.x)));

            node.children.Add(new Node(root + new Vector3Int(size.x, -size.x, size.x)));
            node.children.Add(new Node(root + new Vector3Int(size.x, -size.x, -size.x)));
            node.children.Add(new Node(root + new Vector3Int(-size.x, -size.x, size.x)));
            node.children.Add(new Node(root + new Vector3Int(-size.x, -size.x, -size.x)));
        }
        size = size * new Vector3Int(3, 3, 3);
        IterateFractal(root, size);


        GameObject obj = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, this.transform);
    }
}

public class Node
{
    public GameObject obj;
    public Vector3Int pos;
    public List<Node> children;
    public Node parent;

    // Constructor that initializes the pos field
    public Node(Vector3Int initialPos)
    {
        pos = initialPos;
    }

    // Default constructor if no position is specified
    public Node() {}
}
