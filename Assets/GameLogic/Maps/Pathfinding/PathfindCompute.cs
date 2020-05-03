using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PathfindCompute : MonoBehaviour
{
    const int ThreadGroupSize = 1024;

    public ComputeShader flowCompute;

    public int iterations = 50;

    public int height;
    public int width;

    public float decay = 0.9f;

    private int[,] _flowGridIn;

    private void Start()
    {
        Debug.Log("Recomputing");

        _flowGridIn = new int[height, width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                _flowGridIn[i, j] = 0;
            }

        // setup
        _flowGridIn[0, 0] = 255;
        _flowGridIn[50, 50] = 255;
        _flowGridIn[50, 51] = 255;
        _flowGridIn[50, 52] = 255;
        _flowGridIn[51, 51] = 255;
        _flowGridIn[6, 5] = 255;
        _flowGridIn[6, 1] = 255;

        long time1 = DateTime.Now.Ticks;

        Compute(iterations);

        long time2 = DateTime.Now.Ticks;
        Debug.Log($"it took [{(time2 - time1) / TimeSpan.TicksPerMillisecond}]");

        Display();
    }

    void Display()
    {
        Texture2D tex = new Texture2D(width, height);
        tex.anisoLevel = 0;
        tex.filterMode = FilterMode.Point;
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                float val = 1f * _flowGridIn[i, j] / (iterations + 1);

                if (val > 1)
                    Debug.Log($"found {val}");
                tex.SetPixel(j, i, new Color(val, 0, 0, 1));
            }
        tex.Apply();
        this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
    }

    void Compute(int iterations = 1)
    {
        int count = height * width;

        flowCompute.SetInt("height", height);
        flowCompute.SetInt("width", width);
        flowCompute.SetInt("count", count);
        flowCompute.SetFloat("decay", decay);

        var buffer = new ComputeBuffer(count, FlowData.Size);

        for (int i = 0; i < iterations; i++)
            Compute(buffer, count);

        buffer.Release();
    }

    void Compute(ComputeBuffer buffer, int count)
    {
    
        FlowData[] flowData = new FlowData[count];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                int index = i * width + j;
                flowData[index].AmountIn = _flowGridIn[i, j];
            }

        buffer.SetData(flowData);

        flowCompute.SetBuffer(0, "flowData", buffer);

        int threadGroupsX = Mathf.CeilToInt(count / (float)ThreadGroupSize);
        int threadGroupsY = Mathf.CeilToInt(threadGroupsX / (float)ThreadGroupSize);
        flowCompute.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        buffer.GetData(flowData);

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                int index = i * width + j;
                _flowGridIn[i, j] = flowData[index].AmountOut;
            }

        Debug.Log($"threadGroups [{threadGroupsX}, {threadGroupsY}]");
    }

    private void OnValidate()
    {
        Start();
    }

    public struct FlowData
    {
        public int AmountIn;
        public int AmountOut;

        public static int Size
        {
            get
            {
                return sizeof(int) * 2;
            }
        }
    }
}


