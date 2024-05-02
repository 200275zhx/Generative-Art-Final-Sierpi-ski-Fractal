using UnityEngine;

public class CellularAutomataManager : MonoBehaviour
{
    public ComputeShader cellularAutomataShader;
    private ComputeBuffer cellsBuffer;
    private RenderTexture resultTexture;
    private int kernelHandle;

    public int numCells = 1024;
    public float deltaTime = 0.1f;
    public Vector3[] attractionForces = new Vector3[3];
    public Vector3[] repulsionForces = new Vector3[3];
    public float detectionRange = 5.0f;
    public int textureSize = 256; // Size of the texture to which we are writing

    // For visualization
    public Material displayMaterial;
    private GameObject displayQuad;

    void Start()
    {
        InitializeCells();
        InitializeTexture();
        SetupDisplayQuad();
        kernelHandle = cellularAutomataShader.FindKernel("CSMain");
    }

    void InitializeCells()
    {
        Cell[] cells = new Cell[numCells];
        for (int i = 0; i < numCells; i++)
        {
            float x = Random.value;
            float y = Random.value;
            cells[i].position = new Vector3(x, y, 0);
            cells[i].velocity = Vector3.zero;
            cells[i].colorIndex = Random.Range(0, 3);
        }

        cellsBuffer = new ComputeBuffer(numCells, sizeof(float) * 3 + sizeof(float) * 3 + sizeof(int));
        cellsBuffer.SetData(cells);

        cellularAutomataShader.SetBuffer(kernelHandle, "cellsBuffer", cellsBuffer);
        cellularAutomataShader.SetInt("numCells", numCells);
        cellularAutomataShader.SetFloat("deltaTime", deltaTime);
        cellularAutomataShader.SetFloat("detectionRange", detectionRange);
        cellularAutomataShader.SetVectorArray("attractionForces", ConvertToVector4Array(attractionForces));
        cellularAutomataShader.SetVectorArray("repulsionForces", ConvertToVector4Array(repulsionForces));
    }

    void InitializeTexture()
    {
        resultTexture = new RenderTexture(textureSize, textureSize, 24);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();

        cellularAutomataShader.SetTexture(kernelHandle, "ResultTexture", resultTexture);
    }

    void SetupDisplayQuad()
    {
        displayMaterial.mainTexture = resultTexture;
        displayQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayQuad.transform.position = new Vector3(0, 0, 0);
        displayQuad.transform.localScale = new Vector3(10, 10, 1); // Scale as needed to fit your camera setup
        displayQuad.GetComponent<Renderer>().material = displayMaterial;
    }

    Vector4[] ConvertToVector4Array(Vector3[] vector3Array)
    {
        Vector4[] vector4Array = new Vector4[vector3Array.Length];
        for (int i = 0; i < vector3Array.Length; i++)
        {
            vector4Array[i] = new Vector4(vector3Array[i].x, vector3Array[i].y, vector3Array[i].z, 0);
        }
        return vector4Array;
    }

    void Update()
    {
        cellularAutomataShader.Dispatch(kernelHandle, textureSize / 8, textureSize / 8, 1);
    }

    void OnDestroy()
    {
        if (cellsBuffer != null)
            cellsBuffer.Release();

        if (resultTexture != null)
            resultTexture.Release();
    }

    struct Cell
    {
        public Vector3 position;
        public Vector3 velocity;
        public int colorIndex;
    }
}
