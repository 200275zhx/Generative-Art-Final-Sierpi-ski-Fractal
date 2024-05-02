using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;


public class FlipSolver : MonoBehaviour
{
    private List<Particle> particles;
    private float[,] velocityX;
    private float[,] velocityY;
    private float[,] pressure;
    private int gridWidth, gridHeight;
    private float cellSize;

    void Start()
    {
        InitializeGrid();
        InitializeParticles();
    }

    void Update()
    {
        TransferToGrid();
        CalculateGridVelocities();
        SolvePressure();
        ApplyPressureToVelocity();
        TransferToParticles();
        AdvectParticles();
        HandleCollisions();
    }

    void InitializeGrid()
    {
        // Define grid dimensions (example values, adjust as needed)
        gridWidth = 100;
        gridHeight = 100;
        cellSize = 0.1f;  // The physical size of each grid cell

        // Initialize velocity grids for X and Y components
        velocityX = new float[gridWidth, gridHeight];
        velocityY = new float[gridWidth, gridHeight];

        // Initialize pressure grid
        pressure = new float[gridWidth, gridHeight];

        // Set all initial values to zero
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                velocityX[i, j] = 0.0f;
                velocityY[i, j] = 0.0f;
                pressure[i, j] = 0.0f;
            }
        }
    }

    void InitializeParticles()
    {
        particles = new List<Particle>();

        int numberOfParticles = 1000; // Adjust based on the density you need
        float areaWidth = 10.0f; // Width of the area to fill with particles
        float areaHeight = 10.0f; // Height of the area to fill with particles

        // Calculate the spacing based on the number of particles and the area size
        float spacing = Mathf.Sqrt((areaWidth * areaHeight) / numberOfParticles);

        for (float x = 0.5f * spacing; x < areaWidth; x += spacing)
        {
            for (float y = 0.5f * spacing; y < areaHeight; y += spacing)
            {
                // Initialize each particle at position (x, y) with no initial velocity
                Particle newParticle = new Particle(new Vector2(x, y), Vector2.zero);
                particles.Add(newParticle);
            }
        }
    }


    void TransferToGrid()
    {
        // Clear grid velocities before transferring
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                velocityX[i, j] = 0.0f;
                velocityY[i, j] = 0.0f;
            }
        }

        // Weighting contributions for each particle to the grid cells
        foreach (Particle particle in particles)
        {
            int baseX = Mathf.FloorToInt(particle.Position.x / cellSize);
            int baseY = Mathf.FloorToInt(particle.Position.y / cellSize);

            // Initialize weightSum for each particle
            float weightSumX = 0.0f;
            float weightSumY = 0.0f;

            // Traverse neighboring cells (example uses 2x2 neighborhood)
            for (int i = -1; i <= 2; i++)
            {
                for (int j = -1; j <= 2; j++)
                {
                    int gridX = baseX + i;
                    int gridY = baseY + j;
                    if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                    {
                        float weight = Kernel(particle.Position, new Vector2(gridX * cellSize, gridY * cellSize));
                        velocityX[gridX, gridY] += particle.Velocity.x * weight;
                        velocityY[gridX, gridY] += particle.Velocity.y * weight;
                        weightSumX += weight;
                        weightSumY += weight;
                    }
                }
            }

            // Normalize the velocity by the weight sum to prevent accumulation of mass
            if (weightSumX > 0 && weightSumY > 0)
            {
                for (int i = -1; i <= 2; i++)
                {
                    for (int j = -1; j <= 2; j++)
                    {
                        int gridX = baseX + i;
                        int gridY = baseY + j;
                        if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                        {
                            velocityX[gridX, gridY] /= weightSumX;
                            velocityY[gridX, gridY] /= weightSumY;
                        }
                    }
                }
            }
        }
    }

    // Kernel function for weight calculation (Quadratic Kernel in this example)
    float Kernel(Vector2 particlePos, Vector2 cellCenter)
    {
        Vector2 dist = particlePos - cellCenter;
        float r = dist.magnitude / cellSize;
        float q = 1.0f - r;
        return Mathf.Max(0.0f, q * q);
    }


    void CalculateGridVelocities()
    {
        // Temporary arrays to store updated velocities
        float[,] newVelocityX = new float[gridWidth, gridHeight];
        float[,] newVelocityY = new float[gridWidth, gridHeight];

        // External force (e.g., gravity)
        Vector2 gravity = new Vector2(0.0f, -9.81f) * Time.deltaTime;

        // Advection step
        for (int i = 1; i < gridWidth - 1; i++)
        {
            for (int j = 1; j < gridHeight - 1; j++)
            {
                // Simple forward Euler integration for gravity
                newVelocityX[i, j] = velocityX[i, j] + gravity.x;
                newVelocityY[i, j] = velocityY[i, j] + gravity.y;

                // Corrected semi-Lagrangian advection (backward tracing)
                Vector2 backTraceX = new Vector2(i * cellSize, j * cellSize) - new Vector2(newVelocityX[i, j] * Time.deltaTime, 0);
                Vector2 backTraceY = new Vector2(i * cellSize, j * cellSize) - new Vector2(0, newVelocityY[i, j] * Time.deltaTime);

                // Interpolating the velocity from the grid
                newVelocityX[i, j] = InterpolateVelocity(backTraceX, velocityX);
                newVelocityY[i, j] = InterpolateVelocity(backTraceY, velocityY);
            }
        }

        // Copy the new velocities back to the main velocity arrays
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                velocityX[i, j] = newVelocityX[i, j];
                velocityY[i, j] = newVelocityY[i, j];
            }
        }
    }

    float InterpolateVelocity(Vector2 position, float[,] velocityGrid)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.y / cellSize);

        // Ensure the indices are within the grid boundaries
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        // Simple bilinear interpolation
        float xFrac = (position.x / cellSize) - x;
        float yFrac = (position.y / cellSize) - y;

        float v1 = velocityGrid[x, y];
        float v2 = velocityGrid[Mathf.Min(x + 1, gridWidth - 1), y];
        float v3 = velocityGrid[x, Mathf.Min(y + 1, gridHeight - 1)];
        float v4 = velocityGrid[Mathf.Min(x + 1, gridWidth - 1), Mathf.Min(y + 1, gridHeight - 1)];

        float i1 = Mathf.Lerp(v1, v2, xFrac);
        float i2 = Mathf.Lerp(v3, v4, xFrac);

        return Mathf.Lerp(i1, i2, yFrac);
    }


    void SolvePressure()
    {
        // Temporary array for the new pressures
        float[,] newPressure = new float[gridWidth, gridHeight];

        // Constants for the pressure solve
        float alpha = -cellSize * cellSize / (4 * Time.deltaTime * Time.deltaTime);
        float beta = 4.0f;

        // Number of iterations for Gauss-Seidel solver
        int iterations = 20; // You can increase this for more accuracy

        // Perform Gauss-Seidel iterations
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int i = 1; i < gridWidth - 1; i++)
            {
                for (int j = 1; j < gridHeight - 1; j++)
                {
                    float divergence = (velocityX[i + 1, j] - velocityX[i - 1, j] +
                                        velocityY[i, j + 1] - velocityY[i, j - 1]) / (2 * cellSize);

                    newPressure[i, j] = (pressure[i + 1, j] + pressure[i - 1, j] +
                                         pressure[i, j + 1] + pressure[i, j - 1] - alpha * divergence) / beta;
                }
            }

            // Update pressures for next iteration
            for (int i = 1; i < gridWidth - 1; i++)
            {
                for (int j = 1; j < gridHeight - 1; j++)
                {
                    pressure[i, j] = newPressure[i, j];
                }
            }
        }
    }


    void ApplyPressureToVelocity()
    {
        // Adjust the velocities based on the pressure gradient
        for (int i = 1; i < gridWidth - 1; i++)
        {
            for (int j = 1; j < gridHeight - 1; j++)
            {
                // Compute the pressure gradient
                float pressureGradientX = (pressure[i + 1, j] - pressure[i - 1, j]) / (2 * cellSize);
                float pressureGradientY = (pressure[i, j + 1] - pressure[i, j - 1]) / (2 * cellSize);

                // Subtract the pressure gradient from the velocity fields
                velocityX[i, j] -= pressureGradientX * Time.deltaTime;
                velocityY[i, j] -= pressureGradientY * Time.deltaTime;
            }
        }
    }


    void TransferToParticles()
    {
        float picRatio = 0.05f;  // Ratio of PIC in the PIC/FLIP blend (adjustable)

        foreach (Particle particle in particles)
        {
            int baseX = Mathf.FloorToInt(particle.Position.x / cellSize);
            int baseY = Mathf.FloorToInt(particle.Position.y / cellSize);

            // Ensure we're within bounds
            if (baseX < 0 || baseY < 0 || baseX >= gridWidth - 1 || baseY >= gridHeight - 1)
                continue;

            // Bilinear interpolation for velocities
            Vector2 gridVelocity = BilinearInterpolateVelocity(baseX, baseY, particle.Position);

            // PIC/FLIP blending
            Vector2 updatedVelocity = picRatio * gridVelocity + (1 - picRatio) * (particle.Velocity + (gridVelocity - particle.Velocity));

            // Update particle velocity
            particle.Velocity = updatedVelocity;
        }
    }

    Vector2 BilinearInterpolateVelocity(int baseX, int baseY, Vector2 position)
    {
        float xFrac = (position.x / cellSize) - baseX;
        float yFrac = (position.y / cellSize) - baseY;

        float vx1 = Mathf.Lerp(velocityX[baseX, baseY], velocityX[baseX + 1, baseY], xFrac);
        float vx2 = Mathf.Lerp(velocityX[baseX, baseY + 1], velocityX[baseX + 1, baseY + 1], yFrac);
        float velX = Mathf.Lerp(vx1, vx2, yFrac);

        float vy1 = Mathf.Lerp(velocityY[baseX, baseY], velocityY[baseX + 1, baseY], xFrac);
        float vy2 = Mathf.Lerp(velocityY[baseX, baseY + 1], velocityY[baseX + 1, baseY + 1], yFrac);
        float velY = Mathf.Lerp(vy1, vy2, yFrac);

        return new Vector2(velX, velY);
    }


    void AdvectParticles()
    {
        foreach (Particle particle in particles)
        {
            // Update particle position based on its velocity
            // Simple Euler integration
            particle.Position += particle.Velocity * Time.deltaTime;

            // Handle boundary conditions
            HandleBoundaries(particle);
        }
    }

    void HandleBoundaries(Particle particle)
    {
        // Reflective boundary conditions
        if (particle.Position.x < 0)
        {
            particle.Position.x = -particle.Position.x;
            particle.Velocity.x *= -1;
        }
        else if (particle.Position.x > gridWidth * cellSize)
        {
            particle.Position.x = 2 * (gridWidth * cellSize) - particle.Position.x;
            particle.Velocity.x *= -1;
        }

        if (particle.Position.y < 0)
        {
            particle.Position.y = -particle.Position.y;
            particle.Velocity.y *= -1;
        }
        else if (particle.Position.y > gridHeight * cellSize)
        {
            particle.Position.y = 2 * (gridHeight * cellSize) - particle.Position.y;
            particle.Velocity.y *= -1;
        }
    }


    void HandleCollisions()
    {
        // Define the obstacle, for example, a sphere obstacle at the center with a given radius
        Vector2 obstacleCenter = new Vector2(gridWidth * cellSize / 2, gridHeight * cellSize / 2);
        float obstacleRadius = 5.0f; // radius in units

        foreach (Particle particle in particles)
        {
            // Boundary collision handling
            if (particle.Position.x < 0)
            {
                particle.Position.x = 0;
                particle.Velocity.x *= -1;
            }
            else if (particle.Position.x > gridWidth * cellSize)
            {
                particle.Position.x = gridWidth * cellSize;
                particle.Velocity.x *= -1;
            }

            if (particle.Position.y < 0)
            {
                particle.Position.y = 0;
                particle.Velocity.y *= -1;
            }
            else if (particle.Position.y > gridHeight * cellSize)
            {
                particle.Position.y = gridHeight * cellSize;
                particle.Velocity.y *= -1;
            }

            // Obstacle collision handling (simple sphere collision)
            Vector2 diff = particle.Position - obstacleCenter;
            float dist = diff.magnitude;
            if (dist < obstacleRadius)
            {
                Vector2 normal = diff.normalized;
                particle.Position = obstacleCenter + normal * obstacleRadius; // Reposition particle to the surface of the sphere
                particle.Velocity -= 2 * Vector2.Dot(particle.Velocity, normal) * normal; // Reflect velocity
            }
        }
    }

}

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;

    public Particle(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }
}