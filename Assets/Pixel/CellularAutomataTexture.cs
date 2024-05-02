using UnityEngine;
using UnityEngine.UIElements;

public class CellularAutomataTexture : MonoBehaviour
{
    public int pixWidth, pixHeight;
    public float updateInterval = 0.1f;  // Time in seconds between updates
    public float scale = 0.01f;

    private Texture2D myTex;
    private Color[] pix;
    private float lastUpdateTime;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        myTex = new Texture2D(pixWidth, pixHeight);
        r.material.mainTexture = myTex;
        pix = new Color[pixWidth * pixHeight];
        InitializeTexture();
    }

    void InitializeTexture()
    {
        for (int x = 0; x < pixWidth; x++)
        {
            for (int y = 0; y < pixHeight; y++)
            {
                float randomValue = Mathf.PerlinNoise(x * scale, y * scale);
                if (randomValue < 0.33f)
                    pix[y * pixWidth + x] = Color.red;
                else if (randomValue < 0.66f)
                    pix[y * pixWidth + x] = Color.yellow;
                else
                    pix[y * pixWidth + x] = Color.blue;
            }
        }
        myTex.SetPixels(pix);
        myTex.Apply();
    }

    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateTexture();
        }
    }

    void UpdateTexture()
    {
        Color[] newPix = new Color[pixWidth * pixHeight];

        for (int x = 0; x < pixWidth; x++)
        {
            for (int y = 0; y < pixHeight; y++)
            {
                int index = y * pixWidth + x;
                Color originalColor = pix[index];
                Color dominantNeighborColor = GetDominantNeighborColor(x, y);

                if (originalColor == Color.red && dominantNeighborColor == Color.yellow ||
                    originalColor == Color.yellow && dominantNeighborColor == Color.blue ||
                    originalColor == Color.blue && dominantNeighborColor == Color.red)
                {
                    newPix[index] = dominantNeighborColor;
                }
                else
                {
                    newPix[index] = originalColor;
                }
            }
        }

        pix = newPix;
        myTex.SetPixels(pix);
        myTex.Apply();
    }

    Color GetDominantNeighborColor(int x, int y)
    {
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int redCount = 0, yellowCount = 0, blueCount = 0;

        for (int i = 0; i < 8; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];
            if (nx >= 0 && ny >= 0 && nx < pixWidth && ny < pixHeight)
            {
                Color neighborColor = pix[ny * pixWidth + nx];
                if (neighborColor == Color.red) redCount++;
                if (neighborColor == Color.yellow) yellowCount++;
                if (neighborColor == Color.blue) blueCount++;
            }
        }

        if (redCount > yellowCount && redCount > blueCount)
            return Color.red;
        else if (yellowCount > blueCount)
            return Color.yellow;
        else
            return Color.blue;
    }
}
