using System;
using System.Collections;
using System.Collections.Generic;
using HeightMapGenerators;
using Noises;
using Regions;
using UnityEngine;

namespace SquareRegions
{
    public class SquareRegion : Region
    {
        public RegionGenConfig RegionGenConfig;

        public SquareRegion(int seed,
            RegionGenConfig regionGenConfig,
            HeightMapConfig heightMapConfig,
            FastPerlinNoiseConfig noiseConfig,
            ErosionConfig erosionConfig
        ) : base(seed)
        {
            // ErosionConfig erosionConfig

            this.RegionGenConfig = regionGenConfig;

            // compute required array dimensions
            this.GridRadius = ComputeGridRadius();

            noiseConfig.resolution = (int)(this.GridRadius * heightMapConfig.resolutionScale) + 1;
            this.HeightMap = new HeightMap(seed, heightMapConfig, noiseConfig, erosionConfig);

            this.TileSize = regionGenConfig.tileSize;

            ComputeTileCenterCoords();
            ComputeElevationParameters();

            Debug.Log("Generated square region.");
        }

        private void ComputeTileCenterCoords()
        {
            Tile[,] coords;

            int arraySize = 2 * GridRadius + 1;
            if (arraySize < 0)
                return;

            coords = new Tile[arraySize, arraySize];

            this.RegionSize = TileSize * arraySize * 2;

            // loop over X and Y in hex cube coordinates
            for (int X = -GridRadius; X <= GridRadius; X++)
            {
                for (int Y = -GridRadius; Y <= GridRadius; Y++)
                {
                    int i = X + GridRadius;
                    int j = Y + GridRadius;

                    Vector2 uv = new Vector2(i / 2f / GridRadius, j / 2f / GridRadius);

                    float y = GetElevation(uv.x, uv.y);

                    // initialize tile
                    // compute tile pos in unity axis coordinates
                    float x = TileSize * X;
                    float z = TileSize * Y;
                    coords[i, j] = new Tile(new Vector3(x, y, z), i, j);
                }
            }

            this.Tiles = coords;
        }

        private float GetElevation(float x, float y)
        {
            // TODO: preprocess elevation here since Noise gets interpolated

            return this.RegionGenConfig.maxElevation * this.HeightMap.GetNoiseValueUv(x, y);
        }

        // *** TILE POSITION COMPUTATIONS AND GETTERS *** //

        // unity coordinate pos to storage array index
        override
        public Tile GetTileAt(Vector3 pos)
        {
            Vector2 index = RegionWorldCoordToIndex(new Vector2(pos.x, pos.z));

            int i, j;
            i = (int)index.x + this.GridRadius;
            j = (int)index.y + this.GridRadius;

            if (i < 0 || j < 0 || i >= Tiles.GetLength(0) || j >= Tiles.GetLength(0))
            {
                return null;
            }

            return this.Tiles[i, j];
        }

        // unity units coordinates
        override
        public List<Vector2Int> GetNeighborDirections()
        {
            return new List<Vector2Int>(SquareDirections.Neighbors);
        }

        // unity coordinate system to square coords
        override
        protected Vector3 RegionWorldCoordToIndex(float x, float y)
        {
            float i = (int)(Mathf.Floor(x / this.TileSize + 0.5f));
            float j = (int)(Mathf.Floor(y / this.TileSize + 0.5f));
            return new Vector3(i, j, 0);
        }

        // *** REGION SIZE COMPUTATIONS *** //
        override
        protected int ComputeGridRadius()
        {
            return (int)(Mathf.Floor(Mathf.Sqrt(this.RegionGenConfig.numberOfTiles)) / 2) - 1;
        }
    }

    public static class SquareDirections
    {
        public static List<Vector2Int> Neighbors
        {
            get
            {
                return new List<Vector2Int>() {
                Top,
                TopRight,
                Right,
                BottomRight,
                Bottom,
                BottomLeft,
                Left,
                TopLeft
                };
            }

        }

        public static List<Vector2Int> NeighborsNoDiags
        {
            get
            {
                return new List<Vector2Int>() {
                Top,
                Right,
                Bottom,
                Left,
                };
            }
        }

        public static Vector2Int Top
        {
            get
            {
                return new Vector2Int(-1, 0);
            }
        }
        public static Vector2Int TopRight
        {
            get
            {
                return new Vector2Int(-1, +1);
            }
        }
        public static Vector2Int Right
        {
            get
            {
                return new Vector2Int(0, +1);
            }
        }
        public static Vector2Int BottomRight
        {
            get
            {
                return new Vector2Int(+1, +1);
            }
        }
        public static Vector2Int Bottom
        {
            get
            {
                return new Vector2Int(+1, 0);
            }
        }
        public static Vector2Int BottomLeft
        {
            get
            {
                return new Vector2Int(+1, -1);
            }
        }
        public static Vector2Int Left
        {
            get
            {
                return new Vector2Int(0, -1);
            }
        }
        public static Vector2Int TopLeft
        {
            get
            {
                return new Vector2Int(-1, -1);
            }
        }
    }
}