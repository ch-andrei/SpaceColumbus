// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Regions;
// using UnityEngine;
// using Utilities.PriorityQueue;
//
// namespace Pathfinding
// {
//     public class PathResult
//     {
//         protected List<PathTile> PathTiles; // goal tile is first in this list
//         protected List<PathTile> ExploredPathTiles;
//
//         public bool reachedGoal { get; set; }
//         public float pathCost { get; set; }
//
//         public PathResult()
//         {
//             PathTiles = new List<PathTile>();
//             ExploredPathTiles = new List<PathTile>();
//             reachedGoal = false;
//         }
//
//         public List<Tile> getTilesOnPath()
//         {
//             List<Tile> tilesOnPath = new List<Tile>();
//             foreach (PathTile pt in PathTiles)
//             {
//                 tilesOnPath.Add(pt.Tile);
//             }
//             return tilesOnPath;
//         }
//
//         public List<Tile> getTilesOnPathStartFirst()
//         {
//             List<Tile> tilesOnPath = getTilesOnPath();
//             // reverse the order to be start tile firsts
//             tilesOnPath.Reverse();
//             return tilesOnPath;
//         }
//
//         public List<Tile> GetExploredTiles()
//         {
//             List<Tile> exploredTiles = new List<Tile>();
//             foreach (PathTile pt in ExploredPathTiles)
//             {
//                 exploredTiles.Add(pt.Tile);
//             }
//             return exploredTiles;
//         }
//
//         public void AddPathtile(PathTile pt)
//         {
//             this.PathTiles.Add(pt);
//         }
//
//         public void AddExploredPathtile(PathTile pt)
//         {
//             this.ExploredPathTiles.Add(pt);
//         }
//
//         public string ComputeHashString()
//         {
//             string hash = "";
//             foreach (Tile tile in GetExploredTiles())
//             {
//                 hash += tile.index;
//             }
//             return hash;
//         }
//     }
//
//     // to be used as key in a dictionary
//     public interface IKeyable<T1> { T1 GetKey(); }
//
//     public class PathTile : IKeyable<Vector2Int>
//     {
//         public Tile Tile;
//         public int Depth;
//
//         public PathTile(Tile tile) { this.Tile = tile; }
//
//         public bool Equals(PathTile pt) { return this.Tile.Equals(pt.Tile); }
//
//         public Vector2Int GetKey() { return this.Tile.index; }
//     }
//
//     public abstract class PathFinder
//     {
//         public static float UpElevatonPerPoint = 0.6f;
//         public static float DownElevatonPerPoint = 0.7f;
//
//         public int maxDepth { get; set; }
//         public float maxCost { get; set; }
//         public float maxIncrementalCost { get; set; }
//
//         public PathFinder(int maxDepth, float maxCost, float maxIncrementalCost)
//         {
//             this.maxDepth = maxDepth;
//             this.maxCost = maxCost;
//             this.maxIncrementalCost = maxIncrementalCost;
//         }
//
//         // TODO: refactor this to be handled by region
//         // assumes the tiles are adjacent to each other
//         public virtual float CostBetween(PathTile t1, PathTile t2)
//         {
//             float cost = ((t1.Tile.index - t2.Tile.index).magnitude > 1f) ? Mathf.Sqrt(2f) : 1f; // base cost between tiles
//
//             // cost due to elevation
//             float elevationDelta = (t2.Tile.Pos.y - t1.Tile.Pos.y);
//             if (elevationDelta < 0)
//                 cost -= elevationDelta / DownElevatonPerPoint;
//             else
//                 cost += elevationDelta / UpElevatonPerPoint;
//
//             //Debug.Log("PathFinder cost between " + t1.tile.index + " and " + t2.tile.index + ": " + cost + " elevation influence " + (elevationDelta / downElevatonPerPoint));
//
//             // cost due to tile attributes
//             //cost += t2.tile.moveCostPenalty;
//
//             if (cost > this.maxIncrementalCost)
//                 return float.PositiveInfinity;
//             return cost;
//         }
//
//         public abstract PathResult PathFromTo(Region region, Tile start, Tile goal, bool playersCanBlockPath = false);
//     }
//
//     public class LongDistancePathFinder : PathFinder
//     {
//         private static int _maxDepth = 50;
//         private static float _maxCost = 500;
//
//         DijkstraPathFinder _dijsktraPf;
//         AstarPathFinder _astarPf;
//
//         public LongDistancePathFinder(int maxDepth, float maxCost, float maxIncrementalCost) : base(maxDepth, maxCost, maxIncrementalCost)
//         {
//             _dijsktraPf = new DijkstraPathFinder(maxDepth, maxCost, maxIncrementalCost);
//             _astarPf = new AstarPathFinder(_maxDepth, _maxCost, maxIncrementalCost);
//         }
//
//         override
//         public PathResult PathFromTo(Region region, Tile start, Tile goal, bool playersCanBlockPath = false)
//         {
//             // attempt normal Dijsktra pathfinder first
//             PathResult pr = _dijsktraPf.PathFromTo(
//                 region,
//                 start,
//                 goal,
//                 playersCanBlockPath
//             );
//
//             if (pr.reachedGoal)
//             {
//                 return pr;
//             }
//
//             // get full path to tile even if its out of range
//             PathResult prA = _astarPf.PathFromTo(
//                 region,
//                 start,
//                 goal,
//                 playersCanBlockPath
//             );
//
//             // get move range
//             PathResult prD = _dijsktraPf.PathFromTo(
//                 region,
//                 start,
//                 new Tile(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), int.MaxValue, int.MaxValue),
//                 playersCanBlockPath
//             );
//
//             // get the last tile given by astar pathfinder to goal that is still within move range
//             Tile goal = null;
//             if (prA.reachedGoal)
//             {
//                 foreach (Tile t in prA.getTilesOnPathStartFirst())
//                 {
//                     bool outOfRange = true;
//                     foreach (Tile explored in prD.GetExploredTiles())
//                     {
//                         if (t.Pos == explored.Pos)
//                         {
//                             goal = t;
//                             outOfRange = false;
//                             break;
//                         }
//                     }
//                     if (outOfRange)
//                         break;
//                 }
//             }
//
//             if (goal != null)
//             {
//                 return _dijsktraPf.PathFromTo(
//                     region,
//                     start,
//                     goal,
//                     playersCanBlockPath
//                 );
//             }
//             else
//             {
//                 return prD;
//             }
//         }
//     }
//
//     public abstract class HeuristicPathFinder : PathFinder
//     {
//         public static float HeuristicDepthInfluence = 1e-3f; // nudges priorities for tie breaking
//
//         public HeuristicPathFinder(int maxDepth, float maxCost, float maxIncrementalCost) : base(maxDepth, maxCost, maxIncrementalCost) { }
//
//         override
//         public PathResult PathFromTo(Region region, Tile start, Tile goal, bool playersCanBlockPath = false)
//         {
//             PathResult pathResult = new PathResult();
//
//             PathTile goalPt = new PathTile(goal);
//
//             // set up lists 
//             PriorityQueue<PathTile> frontier = new PriorityQueue<PathTile>();
//             Dictionary<Vector2Int, PathTile> explored = new Dictionary<Vector2Int, PathTile>();
//             Dictionary<Vector2Int, PathTile> previous = new Dictionary<Vector2Int, PathTile>();
//             Dictionary<Vector2Int, float> costs = new Dictionary<Vector2Int, float>();
//
//             PathTile crt;
//
//             crt = new PathTile(start);
//             crt.Depth = 0;
//
//             frontier.Enqueue(crt, 0);
//             previous[crt.Tile.index] = null;
//             costs[crt.Tile.index] = 0;
//
//             // start pathfinding
//             while (!frontier.IsEmpty())
//             {
//                 // get current 
//                 crt = frontier.Dequeue();
//
//                 // record that the tile was explored
//                 explored[crt.Tile.index] = crt;
//
//                 if (crt.Equals(goalPt))
//                 {
//                     // reached goal; search complete
//                     pathResult.reachedGoal = true;
//                     pathResult.pathCost = costs[crt.Tile.index];
//                     break;
//                 }
//
//                 // get neighbor tiles
//                 List<PathTile> neighbors = new List<PathTile>();
//                 foreach (Tile neighborTile in region.GetTileNeighbors(crt.Tile.index))
//                 {
//                     PathTile neighbor = new PathTile(neighborTile);
//                     //neighborPt.cost = crt.cost + costBetween(crt, neighborPt);
//                     neighbor.Depth = crt.Depth + 1;
//                     neighbors.Add(neighbor);
//                 }
//
//                 // add neighbor tiles to search
//                 float cost, priority;
//                 foreach (PathTile neighbor in neighbors)
//                 {
//
//                     // check if exceeding max depth
//                     if (neighbor.Depth > maxDepth)
//                     {
//                         break;
//                     }
//
//                     // compute cost
//                     float cost = CostBetween(crt, neighbor);
//
//                     //// check if path is blocked by another player
//                     //if (playersCanBlockPath && GameControl.gameSession.checkForPlayersAt(neighbor.tile) != null)
//                     //{
//                     //    if (!neighbor.CompareTo(goalPt))  // ensures that you can move to a tile with an enemy
//                     //        _cost = float.PositiveInfinity; // set highest cost to signify that the tile is unreachable
//                     //}
//
//                     cost = costs[crt.Tile.index] + cost;
//
//                     if (cost <= maxCost)
//                     {
//                         if (!costs.ContainsKey(neighbor.Tile.index) || cost < costs[neighbor.Tile.index])
//                         {
//                             costs[neighbor.Tile.index] = cost;
//
//                             // compute heuristic priority
//                             priority = cost + Heuristic(region, neighbor, goalPt);
//                             priority -= neighbor.Depth * HeuristicDepthInfluence; // makes so that tiles closest to goal are more eagerly explored
//
//                             frontier.Enqueue(neighbor, priority);
//
//                             previous[neighbor.Tile.index] = crt;
//                         }
//                     }
//                 }
//             }
//
//             // build list of tiles on path if goal was reached
//             if (pathResult.reachedGoal)
//             {
//                 pathResult.AddPathtile(goalPt);
//
//                 crt = previous[goal.index];
//
//                 while (crt != null)
//                 {
//                     pathResult.AddPathtile(crt);
//                     crt = previous[crt.Tile.index];
//                 }
//             }
//
//             foreach (PathTile pt in explored.Values)
//             {
//                 pathResult.AddExploredPathtile(pt);
//             }
//
//             return pathResult;
//         }
//
//         // *** HEURISTIC COMPUTATIONS *** ///
//
//         public abstract float Heuristic(Region region, PathTile start, PathTile goal);
//     }
//
//     public class AstarPathFinder : HeuristicPathFinder
//     {
//         public AstarPathFinder(int maxDepth, float maxCost, float maxIncrementalCost) : base(maxDepth, maxCost, maxIncrementalCost) { }
//
//         override
//         public float Heuristic(Region region, PathTile start, PathTile goal)
//         {
//             float cost = region.DistanceBetweenTiles(start.Tile, goal.Tile);
//             float elevationDelta = start.Tile.Pos.y - goal.Tile.Pos.y;
//             if (elevationDelta < 0)
//                 cost += -elevationDelta / DownElevatonPerPoint;
//             else
//                 cost += elevationDelta / UpElevatonPerPoint;
//             return cost;
//         }
//     }
//
//     public class DijkstraPathFinder : HeuristicPathFinder
//     {
//         public DijkstraPathFinder(int maxDepth, float maxCost, float maxIncrementalCost) : base(maxDepth, maxCost, maxIncrementalCost) { }
//
//         override
//         public float Heuristic(Region region, PathTile start, PathTile goal)
//         {
//             // Dijkstra can be considered as a special case of A* where heuristic is always equal zero
//             return 0;
//         }
//     }
//
//     public class DijkstraUniformCostPathFinder : DijkstraPathFinder
//     {
//         private float _uniformCost;
//
//         public DijkstraUniformCostPathFinder(float uniformCost, int maxDepth, float maxCost, float maxIncrementalCost = 0) : base(maxDepth, maxCost, maxIncrementalCost)
//         {
//             this._uniformCost = uniformCost;
//         }
//
//         override
//         public float CostBetween(PathTile t1, PathTile t2)
//         {
//             return _uniformCost;
//         }
//     }
// }