using System;
using UnityEngine;
using Utilities.Misc;

namespace NoiseGenerators
{
    public abstract class Noise
    {
        private int _noiseResolution = 500;
        private float[,] _values;
        private bool _generated = false;
        private int _seed;

        public Noise(int noiseResolution, int seed)
        {
            this._noiseResolution = noiseResolution;
            this._seed = seed;
            UnityEngine.Random.InitState(seed);
        }

        public void GenerateNoise()
        {
            if (_generated)
            {
                Debug.Log("Warning: Noise already generated.");
                return;
            }
            _values = GenerateNoiseValues();
            Debug.Log("Noise was generated.");
            _generated = true;
        }

        // extending noise classes must implement this method
        public abstract float[,] GenerateNoiseValues();

        public void Regenerate()
        {
            _generated = false;
            GenerateNoise();
        }

        public bool GetGenerated()
        {
            return _generated;
        }

        public float[,] GetNoiseValues()
        {
            return this._values;
        }

        public void SetNoiseValues(float[,] noiseValues)
        {
            this._values = noiseValues;
        }

        // returns linearly interpolated weighted average of a local area of 4 pixels
        // the noise is usually a lower resolution array than the map which uses it, thus we need to interpolate
        public float LerpNoiseValue(float baseU, float baseV)
        {
            float noiseIndex = GetNoiseRes() - 1;
            float uInd = noiseIndex * baseU;
            float vInd = noiseIndex * baseV;

            int uF = (int)Mathf.Floor(uInd);
            int uC = (int)Mathf.Ceil(uInd);
            int vF = (int)Mathf.Floor(vInd);
            int vC = (int)Mathf.Ceil(vInd);

            float valFf = _values[uF, vF];
            float valFc = _values[uF, vC];
            float valCf = _values[uC, vF];
            float valCc = _values[uC, vC];

            float u = uInd - uF;
            float v = vInd - vF;

            float val1 = Mathf.Lerp(valFf, valCf, u);
            float val2 = Mathf.Lerp(valFc, valCc, u);
            return Mathf.Lerp(val1, val2, v);
        }

        public int GetNoiseRes()
        {
            return _noiseResolution;
        }
    }

    public class ZeroNoiseMap : Noise
    {
        public ZeroNoiseMap(int noiseResolution, int seed) : base(noiseResolution, seed)
        {
            this.GenerateNoise();
        }

        override
        public float[,] GenerateNoiseValues()
        {
            float[,] zeros = new float[this.GetNoiseRes(), this.GetNoiseRes()];
            for (int i = 0; i < this.GetNoiseRes(); i++)
            {
                for (int j = 0; j < this.GetNoiseRes(); j++)
                {
                    zeros[i, j] = 0;
                }
            }
            return zeros;
        }
    }

    [System.Serializable]
    public class FastPerlinNoiseConfig
    {
        // noise inputs
        [Range(0.001f, 500f)]
        public float amplitude = 5f;

        [Range(0.001f, 1f)]
        public float persistance = 0.25f;

        [Range(1, 16)]
        public int octaves = 8;

        [Range(1, 10)]
        public int levels = 5;

        public int resolution { get; set; }

        // initialize the region parameters
        public FastPerlinNoiseConfig() { }
    }

    public class FastPerlinNoise : Noise
    {
        private FastPerlinNoiseConfig _noiseConfig;

        public FastPerlinNoiseConfig config { get { return this._noiseConfig; } }
        public int resolution { get { return this._noiseConfig.resolution; } }
        public float amplitude { get { return this._noiseConfig.amplitude; } }
        public float persistance { get { return this._noiseConfig.persistance; } }
        public int octaves { get { return this._noiseConfig.octaves; } }
        public int levels { get { return this._noiseConfig.levels; } }

        public FastPerlinNoise(int seed, FastPerlinNoiseConfig config) : base(config.resolution, seed)
        {
            this._noiseConfig = config;
            this.GenerateNoise();
        }

        override
        public float[,] GenerateNoiseValues()
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

            float amplitude = this.amplitude;

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