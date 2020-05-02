using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities.Misc;

namespace Noises
{
    public class FastPerlinNoiseGpu : FastPerlinNoise
    {
        public ComputeShader Compute;

        public FastPerlinNoiseGpu(int seed, FastPerlinNoiseConfig config) : base(seed, config) { }

        public override float[,] GenerateNoiseValues()
        {
            return GenerateMultipleLevelPerlinNoise(octaves, levels);
        }

        private float[,] GenerateMultipleLevelPerlinNoise(int octaveCount, int levels)
        {
            float[,] perlinNoiseCombined = new float[GetNoiseRes(), GetNoiseRes()];
            // generate 0,1,...,levels of perlin noise patterns and merge these
            for (int i = 1; i <= levels; i++)
            {
                float[,] baseNoise = GenerateWhiteNoise(GetNoiseRes());
                float[,] perlinNoise = GeneratePerlinNoise(baseNoise, octaveCount);
                // merge results of new perlin level with previous perlinNoise
                perlinNoiseCombined = Tools.MergeArrays(perlinNoise, perlinNoiseCombined, 1f / levels, (float)i / levels);
            }
            return perlinNoiseCombined;
        }

        private float[,] GenerateWhiteNoise(int size)
        {
            float[,] noise = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    noise[i, j] = (float)UnityEngine.Random.value;
                }
            }
            return noise;
        }

        private float[,] GenerateSmoothNoise(float[,] baseNoise, int octave)
        {
            int length = baseNoise.GetLength(0);
            float[,] smoothNoise = new float[length, length];

            int samplePeriod = (int)(2 * octave + 1); // calculates 2 ^ k
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < length; i++)
            {
                //calculate the horizontal sampling indices
                int sampleI0 = (i / samplePeriod) * samplePeriod;
                int sampleI1 = (sampleI0 + samplePeriod) % length; //wrap around
                float horizontalBlend = (i - sampleI0) * sampleFrequency;

                for (int j = 0; j < length; j++)
                {
                    //calculate the vertical sampling indices
                    int sampleJ0 = (j / samplePeriod) * samplePeriod;
                    int sampleJ1 = (sampleJ0 + samplePeriod) % length; //wrap around
                    float verticalBlend = (j - sampleJ0) * sampleFrequency;

                    //blend the top two corners
                    float top = Mathf.Lerp(baseNoise[sampleI0, sampleJ0],
                        baseNoise[sampleI1, sampleJ0], horizontalBlend);

                    //blend the bottom two corners
                    float bottom = Mathf.Lerp(baseNoise[sampleI0, sampleJ1],
                        baseNoise[sampleI1, sampleJ1], horizontalBlend);

                    //final blend
                    smoothNoise[i, j] = Mathf.Lerp(top, bottom, verticalBlend);
                }
            }
            return smoothNoise;
        }

        private float[,] GeneratePerlinNoise(float[,] baseNoise, int octaveCount)
        {
            int length = baseNoise.GetLength(0);
            float[][,] smoothNoise = new float[octaveCount][,]; //an array of 2D arrays

            //generate smooth noise
            for (int i = 0; i < octaveCount; i++)
            {
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
            }

            float[,] perlinNoise = new float[length, length]; //an array of floats initialized to 0

            float totalAmplitude = 0.0f;

            float amplitude = base.amplitude;

            //blend noise together
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        perlinNoise[i, j] += smoothNoise[octave][i, j] * amplitude;
                    }
                }
            }

            //normalisation
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    perlinNoise[i, j] /= totalAmplitude;
                }
            }

            return perlinNoise;
        }
    }
}
