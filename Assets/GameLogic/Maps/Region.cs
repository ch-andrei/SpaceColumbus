using System;
using System.Collections.Generic;
using HeightMapGenerators;
using UnityEngine;

namespace Regions
{
    public class Tile
    {
        public Vector3 Pos;
        public int I;
        public int J;

        public Vector2Int index { get { return new Vector2Int(this.I, this.J); } }

        public Tile(Vector3 pos, int i, int j)
        {
            this.Pos = pos;
            this.I = i;
            this.J = j;
        }

        public bool Equals(Tile tile) { return this.I == tile.I && this.J == tile.J; }
    }

    [System.Serializable] // for unity editor
    public class RegionGenConfig
    {
        private const int MaxNumberOfTiles = 64000; // slightly less than ~2^16 -> unity's mesh vertex count limitation 

        [Range(1, MaxNumberOfTiles)]
        public int numberOfTiles;

        [Range(0.001f, 100f)]
        public float tileSize;

        [Range(1, 250)]
        public int maxElevation;

        public RegionGenConfig() { }
    }

    public abstract class Region
    {
        protected int Seed;

        protected HeightMap HeightMap;

        protected float TileSize;
        protected int GridRadius;

        protected float RegionSize;
        protected float Center;

        //protected float waterLevelElevation;
        protected float MinElevation, MaxElevation, AvgElevation;

        public Tile[,] Tiles;
        protected abstract int ComputeGridRadius();

        protected Vector3 RegionWorldCoordToIndex(Vector2 pos) { return RegionWorldCoordToIndex(pos.x, pos.y); }
        protected Vector3 RegionWorldCoordToIndex(Vector3 pos) { return RegionWorldCoordToIndex(pos.x, pos.z); }
        protected abstract Vector3 RegionWorldCoordToIndex(float x, float y);

        public abstract Tile GetTileAt(Vector3 pos);
        public abstract List<Vector2Int> GetNeighborDirections();

        public Region(int seed)
        {
            this.Seed = seed;
        }

        public Tile[,] GetTiles()
        {
            return this.Tiles;
        }

        // unity units coordinates
        public List<Tile> GetTileNeighbors(Vector3 tilePos)
        {
            return GetTileNeighbors(RegionWorldCoordToIndex(tilePos));
        }

        // unity units coordinates
        public List<Tile> GetTileNeighbors(Vector2Int tileIndex)
        {
            return GetTileNeighbors(tileIndex.x, tileIndex.y);
        }

        public float DistanceBetweenTiles(Tile tile1, Tile tile2)
        {
            return (tile1.Pos - tile2.Pos).magnitude;
        }

        // array index coordinates
        public List<Tile> GetTileNeighbors(int i, int j)
        {
            List<Tile> neighbors = new List<Tile>();
            foreach (Vector2Int dir in this.GetNeighborDirections())
            {
                try
                {
                    Tile neighbor = this.Tiles[i + dir.x, j + dir.y];
                    if (neighbor != null)
                        neighbors.Add(neighbor);
                }
                catch (IndexOutOfRangeException e)
                {
                    // nothing to do
                }
            }
            return neighbors;
        }

        public Vector2 Pos2Uv(Vector3 pos)
        {
            float u = pos.x / RegionSize + 1f / 2f;
            float v = pos.z / RegionSize + 1f / 2f;
            return new Vector2(u, v);
        }

        // *** ELEVATION PARAMETERS COMPUTATIONS *** //
        protected void ComputeElevationParameters()
        {
            this.MinElevation = this.ComputeMinimumElevation();
            this.MaxElevation = this.ComputeMaximumElevation();
            this.AvgElevation = this.ComputeAverageElevation();
        }

        protected float ComputeAverageElevation()
        {
            double sum = 0;
            List<Vector3> positions = GetTileVertices();
            foreach (Vector3 pos in positions)
            {
                sum += pos.y;
            }
            return (float)(sum / (positions.Count));
        }

        protected float ComputeMaximumElevation()
        {
            float max = -float.MaxValue;
            List<Vector3> positions = GetTileVertices();
            foreach (Vector3 pos in positions)
            {
                if (max < pos.y)
                {
                    max = pos.y;
                }
            }
            return max;
        }

        protected float ComputeMinimumElevation()
        {
            float min = float.MaxValue;
            List<Vector3> positions = GetTileVertices();
            foreach (Vector3 pos in positions)
            {
                if (min > pos.y)
                {
                    min = pos.y;
                }
            }
            return min;
        }

        // *** GETTERS AND SETTERS *** //

        public List<Vector3> GetTileVertices()
        {
            List<Vector3> tilesList = new List<Vector3>();

            int length = Tiles.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (Tiles[i, j] != null)
                        tilesList.Add(Tiles[i, j].Pos);
                }
            }

            return tilesList;
        }
        //public int getMinimumElevation()
        //{
        //    return this.minElevation;
        //}
        //public int getMaximumElevation()
        //{
        //    return this.maxElevation;
        //}
        //public int getAverageElevation()
        //{
        //    return this.averageElevation;
        //}
        //public int getWaterLevelElevation()
        //{
        //    return this.waterLevelElevation;
        //}
        //public int getViewableSize()
        //{
        //    return this.regionConfig.regionSize;
        //}
        //public long getViewableSeed()
        //{
        //    return this.regionConfig.regionGenConfig.seed;
        //}
        //public int getMaxTileIndex()
        //{
        //    return this.tiles.GetLength(0);
        //}
    }
}