using System.Collections.Generic;
using UnityEngine;

public class Erosion
{
    public ComputeShader ErosionShader;

    [Header("Mesh Settings")]
    public int MapSize = 255;
    public float Scale = 20;
    public float ElevationScale = 10;

    [Header("Erosion Settings")]
    public int ErosionBrushRadius = 3;

    public int MaxLifetime = 30;
    public float SedimentCapacityFactor = 3;
    public float MinSedimentCapacity = .01f;
    public float DepositSpeed = 0.3f;
    public float ErodeSpeed = 0.3f;

    public float EvaporateSpeed = .01f;
    public float Gravity = 4;
    public float StartSpeed = 1;
    public float StartWater = 1;
    [Range(0, 1)] public float Inertia = 0.3f;

    static int _numThreadsPerGroup = 1024;

    public void Erode(float[,] mapIn, int numIterations)
    {
        Debug.Log($"trying to run erosion with numIterations {numIterations}");

        int mapSize = mapIn.GetLength(0);
        int mapSizeWithBorder = mapSize + ErosionBrushRadius * 2;

        float[] map = new float[mapSize * mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                map[i * mapSize + j] = mapIn[i, j];
            }
        }

        {
            int numThreads = numIterations / _numThreadsPerGroup;

            // Create brush
            List<int> brushIndexOffsets = new List<int>();
            List<float> brushWeights = new List<float>();

            float weightSum = 0;
            for (int brushY = -ErosionBrushRadius; brushY <= ErosionBrushRadius; brushY++)
            {
                for (int brushX = -ErosionBrushRadius; brushX <= ErosionBrushRadius; brushX++)
                {
                    float sqrDst = brushX * brushX + brushY * brushY;
                    if (sqrDst < ErosionBrushRadius * ErosionBrushRadius)
                    {
                        brushIndexOffsets.Add(brushY * mapSize + brushX);
                        float brushWeight = 1 - Mathf.Sqrt(sqrDst) / ErosionBrushRadius;
                        weightSum += brushWeight;
                        brushWeights.Add(brushWeight);
                    }
                }
            }
            for (int i = 0; i < brushWeights.Count; i++)
            {
                brushWeights[i] /= weightSum;
            }

            // Send brush data to compute shader
            ComputeBuffer brushIndexBuffer = new ComputeBuffer(brushIndexOffsets.Count, sizeof(int));
            ComputeBuffer brushWeightBuffer = new ComputeBuffer(brushWeights.Count, sizeof(int));
            brushIndexBuffer.SetData(brushIndexOffsets);
            brushWeightBuffer.SetData(brushWeights);
            ErosionShader.SetBuffer(0, "brushIndices", brushIndexBuffer);
            ErosionShader.SetBuffer(0, "brushWeights", brushWeightBuffer);

            // Generate random indices for droplet placement
            int[] randomIndices = new int[numIterations];
            for (int i = 0; i < numIterations; i++)
            {
                int randomX = Random.Range(ErosionBrushRadius, mapSize + ErosionBrushRadius);
                int randomY = Random.Range(ErosionBrushRadius, mapSize + ErosionBrushRadius);
                randomIndices[i] = randomY * mapSize + randomX;
            }

            // Send random indices to compute shader
            ComputeBuffer randomIndexBuffer = new ComputeBuffer(randomIndices.Length, sizeof(int));
            randomIndexBuffer.SetData(randomIndices);
            ErosionShader.SetBuffer(0, "randomIndices", randomIndexBuffer);

            // Heightmap buffer
            ComputeBuffer mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
            mapBuffer.SetData(map);
            ErosionShader.SetBuffer(0, "map", mapBuffer);

            // Settings
            ErosionShader.SetInt("borderSize", ErosionBrushRadius);
            ErosionShader.SetInt("mapSize", mapSizeWithBorder);
            ErosionShader.SetInt("brushLength", brushIndexOffsets.Count);
            ErosionShader.SetInt("maxLifetime", MaxLifetime);
            ErosionShader.SetFloat("inertia", Inertia);
            ErosionShader.SetFloat("sedimentCapacityFactor", SedimentCapacityFactor);
            ErosionShader.SetFloat("minSedimentCapacity", MinSedimentCapacity);
            ErosionShader.SetFloat("depositSpeed", DepositSpeed);
            ErosionShader.SetFloat("erodeSpeed", ErodeSpeed);
            ErosionShader.SetFloat("evaporateSpeed", EvaporateSpeed);
            ErosionShader.SetFloat("gravity", Gravity);
            ErosionShader.SetFloat("startSpeed", StartSpeed);
            ErosionShader.SetFloat("startWater", StartWater);

            // Run compute shader
            ErosionShader.Dispatch(0, numThreads, 1, 1);
            mapBuffer.GetData(map);

            // Release buffers
            mapBuffer.Release();
            randomIndexBuffer.Release();
            brushIndexBuffer.Release();
            brushWeightBuffer.Release();
        }

        // write back
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                mapIn[i, j] = map[i * mapSize + j];
            }
        }
    }
}
