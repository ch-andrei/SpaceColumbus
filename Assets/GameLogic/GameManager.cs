using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using UnityEngine;
using UnityEngine.AI;

using Noises;
using HeightMapGenerators;

using Regions;
using SquareRegions;
using RegionModelGenerators;

using Entities;
using Entities.Bodies;
using Navigation;

using Utilities.Misc;
using EntitySelection;

using Players;
using UnityEngine.Serialization;

[System.Serializable]
public class GameSessionParams
{
    public bool debug = false;
    public bool debugMapGen = false;

    [Header("Agent Config")]
    public bool spawnAgents = false;
    public int numAgentsToSpawn = 2000;
    public bool spawnAgentRandom = false;
    public float spawnAgentRandomDistance = 1000f;

    [Header("Region Config")]
    public bool generateRegion = true;
    [Range(1, 100000000)] public int seed = 0;
    public bool useRandomSeed;
    public bool regenerate = false;

    public RegionGenConfig regionGenConfig;
    public HeightMapConfig heightMapConfig;
    public FastPerlinNoiseConfig noiseConfig;
    public ErosionConfig erosionConfig;
}

public class GameManager : MonoBehaviour
{
    public GameSessionParams gameSessionParams;
    public GameSession gameSession { get; private set; }

    public void Awake()
    {
        EntityManager.Initialize();
        PlayerManager.Initialize();
        SelectionManager.Initialize();
        BodyPartFactory.Initialize();

        StartGameSession();
    }

    public void OnDestroy()
    {
        AnimationManager.OnDestroy();
    }

    public void StartGameSession()
    {
        this.gameSession = new GameSession(gameSessionParams);

        if (gameSessionParams.spawnAgents)
            for (int i = 0; i < gameSessionParams.numAgentsToSpawn; i++)
                SpawnSimpleAgent(gameSessionParams.spawnAgentRandom);
    }

    public void SpawnSimpleAgent(Vector3 position)
    {
        var agentRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.AgentRootTag);

        var gameRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.GameRootTag);
        var agentPrefab = gameRoot.GetComponent<PrefabManager>().agentPrefab;

        Instantiate(agentPrefab, position, agentPrefab.transform.rotation, agentRoot.transform);
    }

    public void SpawnSimpleAgent(bool random = false)
    {
        Vector3 pos;
        if (random)
        {
            Vector2 rc = UnityEngine.Random.onUnitSphere;
            pos = gameSessionParams.spawnAgentRandomDistance * new Vector3(rc.x, 0, rc.y);
        }
        else {
            pos = Vector3.zero;
            pos.y = this.gameSession.region.GetTileAt(new Vector3()).Pos.y;
        }

        SpawnSimpleAgent(pos);
    }

    public void MoveSelectedAgents(Vector3 destination)
    {
        var selectedObjects = SelectionManager.GetSelectedObjects();
        foreach (var selectedObject in selectedObjects)
        {
            try
            {
                var agent = selectedObject.GetComponent<Agent>();
                agent.MoveTo(destination);
            }
            // these may occur when an object is destroyed
            catch (MissingReferenceException e) { }
            catch (NullReferenceException e) { }
        }
    }

    public void StopSelectedAgents()
    {
        var selectedObjects = SelectionManager.GetSelectedObjects();
        foreach (var selectedObject in selectedObjects)
        {
            var agent = selectedObject.GetComponent<Agent>();
            if (agent != null)
                agent.Stop();
        }
    }
}

public class GameSession
{
    private GameSessionParams _params;

    public Region region { get; private set; }
    public Player CurrentPlayer { get; private set; }

    private int _seed;

    public GameSession(GameSessionParams gameSessionParams)
    {
        _params = gameSessionParams;

        Initialize();
    }

    public void Initialize()
    {
        if (_params.debug)
            return;

        this._seed = _params.useRandomSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : _params.seed;

        Debug.Log($"Initializing GameSession with seed {this._seed}");

        if (_params.generateRegion)
        {
            BuildRegion();
            BuildRegionView();
        }

        if (_params.debugMapGen)
            return;

        BuildNavMeshes();

        CurrentPlayer = PlayerManager.AddNewPlayer();
    }

    public void BuildRegion()
    {
        this.region = new SquareRegion(
            this._seed, _params.regionGenConfig, _params.heightMapConfig, _params.noiseConfig, _params.erosionConfig);
    }

    public void BuildRegionView()
    {
        var viewablesRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.ViewablesTag);

        foreach (Transform t in viewablesRoot.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        GameObject regionView = new GameObject(StaticGameDefs.RegionViewObjectName);
        regionView.transform.parent = viewablesRoot.transform;

        SquareRegionModelGenerator squareRegionModelGenerator = regionView.AddComponent<RegionModelGenerators.SquareRegionModelGenerator>();
        squareRegionModelGenerator.InitializeMesh(this.region);
    }

    public void BuildNavMeshes()
    {
        var navMeshRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.NavMeshRootTag);
        var navMeshGenerator = navMeshRoot.GetComponent<NavMeshGenerator>();
        navMeshGenerator.BuildNavMesh();
    }

    // public Color HexToColor(string hex)
    // {
    //     return Tools.HexToColor(hex);
    // }
    //
    // private void OnDrawGizmos()
    // {
    //     if (!drawGizmos)
    //     {
    //         return;
    //     }
    //
    //     if (_region != null)
    //     {
    //         // draw some floor
    //         Gizmos.color = HexToColor("#000000"); // black
    //         //Gizmos.DrawCube (new Vector3 (0, 0, 0), new Vector3 (10000, 0, 10000));
    //
    //         // set color and draw gizmos
    //         //int water_level = gameSession.mapGenerator.getRegion().getWaterLevelElevation();
    //         int order = 0;
    //         Color c;
    //         foreach (Vector3 pos in _region.GetTileVertices())
    //         {
    //             if (gizmoSkip + 1 != 0)
    //             {
    //                 order = ++order % (gizmoSkip + 1);
    //                 if (order != 0)
    //                 {
    //                     continue;
    //                 }
    //             }
    //             //if (tile.getTileType() != null)
    //             {
    //                 int elevation = (int)pos.y /*- water_level*/ ;
    //                 //if (tile.getTileType().GetType() == typeof(WaterTileType))
    //                 //{
    //                 //    //Debug.Log("water: elevation " + elevation);
    //                 //    if (elevation > -5)
    //                 //    {
    //                 //        c = hexToColor("#C2D2E7");
    //                 //    }
    //                 //    else if (elevation > -10)
    //                 //    {
    //                 //        c = hexToColor("#54B3F0");
    //                 //    }
    //                 //    else if (elevation > -25)
    //                 //    {
    //                 //        c = hexToColor("#067DED");
    //                 //    }
    //                 //    else if (elevation > -50)
    //                 //    {
    //                 //        c = hexToColor("#005F95");
    //                 //    }
    //                 //    else
    //                 //        c = hexToColor("#004176");
    //                 //}
    //                 //else if (tile.getTileType().GetType() == typeof(LandTileType))
    //                 float heighScaling = 0.1f;
    //                 {
    //                     //Debug.Log("water: elevation " + elevation);
    //                     if (elevation < 0)
    //                         c = HexToColor("#696300");
    //                     else if (elevation < 10 * heighScaling)
    //                         c = HexToColor("#00C103");
    //                     else if (elevation < 20 * heighScaling)
    //                         c = HexToColor("#59FF00");
    //                     else if (elevation < 30 * heighScaling)
    //                         c = HexToColor("#F2FF00");
    //                     else if (elevation < 40 * heighScaling)
    //                         c = HexToColor("#FFBE00");
    //                     else if (elevation < 50 * heighScaling)
    //                         c = HexToColor("#FF8C00");
    //                     else if (elevation < 60 * heighScaling)
    //                         c = HexToColor("#FF6900");
    //                     else if (elevation < 70 * heighScaling)
    //                         c = HexToColor("#E74900");
    //                     else if (elevation < 80 * heighScaling)
    //                         c = HexToColor("#E10C00");
    //                     else if (elevation < 90 * heighScaling)
    //                         c = HexToColor("#971C00");
    //                     else if (elevation < 100 * heighScaling)
    //                         c = HexToColor("#C24340");
    //                     else if (elevation < 115 * heighScaling)
    //                         c = HexToColor("#B9818A");
    //                     else if (elevation < 130 * heighScaling)
    //                         c = HexToColor("#988E8B");
    //                     else if (elevation < 160 * heighScaling)
    //                         c = HexToColor("#AEB5BD");
    //                     else // default
    //                         c = HexToColor("#FFFFFF");
    //                 }
    //                 //else
    //                 //    c = new Color(0, 0, 0, 0);
    //                 Gizmos.color = c;
    //                 //if (elevation < 0) {
    //                 //    pos.y = water_level; // if it's water, draw elevation as equal to water_level
    //                 //}
    //                 Gizmos.DrawSphere(pos, gizmoSize);
    //             }
    //         }
    //     }
    // }
}
