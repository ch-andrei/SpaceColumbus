using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Noises;
using HeightMapGenerators;

using Regions;
using SquareRegions;
using RegionModelGenerators;

using Animation;
using Animation.Systems;

using Entities;
using Navigation;

using Utilities.Misc;
using EntitySelection;

using Players;
using UnityEngine.Serialization;

public class GameSession : MonoBehaviour
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

    [Header("UI Config")]
    public bool drawGizmos = true;
    [Range(0.001f, 10)]
    public float gizmoSize = 1f;
    [Range(0, 100)]
    public int gizmoSkip = 0;

    private Region _region;

    public Player CurrentPlayer { get; private set; }

    public void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (debug)
            return;

        this.seed = useRandomSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : this.seed;

        Debug.Log($"Initializing GameSession with seed {this.seed}");

        if (generateRegion)
        {
            BuildRegion();
            BuildRegionView();
        }

        if (debugMapGen)
            return;

        BuildNavMeshes();

        EntityManager.Initialize();
        SelectionManager.Initialize();

        PlayerManager.Initialize();
        CurrentPlayer = PlayerManager.AddNewPlayer();

        if (spawnAgents)
            for (int i = 0; i < numAgentsToSpawn; i++)
                SpawnSimpleAgent(spawnAgentRandom);
    }

    public void BuildRegion()
    {
        this._region = new SquareRegion(this.seed, regionGenConfig, heightMapConfig, noiseConfig, erosionConfig);
    }

    public void BuildRegionView()
    {
        GameObject viewablesRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.ViewablesTag);

        foreach (Transform t in viewablesRoot.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        GameObject regionView = new GameObject(StaticGameDefs.RegionViewObjectName);
        regionView.transform.parent = viewablesRoot.transform;

        SquareRegionModelGenerator squareRegionModelGenerator = regionView.AddComponent<RegionModelGenerators.SquareRegionModelGenerator>();
        squareRegionModelGenerator.InitializeMesh(this._region);
    }

    public void BuildNavMeshes()
    {
        GameObject navMeshRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.NavMeshRootTag);
        NavMeshGenerator navMeshGenerator = navMeshRoot.GetComponent<NavMeshGenerator>();
        navMeshGenerator.BuildNavMesh();
    }

    public void SpawnSimpleAgent(Vector3 position)
    {
        GameObject agentRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.AgentRootTag);

        GameObject gameRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.GameRootTag);
        GameObject agentPrefab = gameRoot.GetComponent<PrefabManager>().agentPrefab;

        GameObject agent = GameObject.Instantiate(agentPrefab, position, agentPrefab.transform.rotation, agentRoot.transform);
    }

    public void SpawnSimpleAgent(bool random = false)
    {
        Vector3 pos;
        if (random)
        {
            Vector2 rc = UnityEngine.Random.onUnitSphere;
            pos = spawnAgentRandomDistance * new Vector3(rc.x, 0, rc.y);
        }
        else {
            pos = Vector3.zero;
            pos.y = this._region.GetTileAt(new Vector3()).Pos.y;
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

    public Region GetRegion()
    {
        return this._region;
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Starting game session.");
    }

    public void Update()
    {
        float time = Time.time;
        float deltaTime = Time.deltaTime;

        AnimationManager.Update(time, deltaTime);

        //SpawnSimpleAgent();
        if (regenerate)
        {
            regenerate = false;
            Initialize();
        }
    }

    private void OnDestroy()
    {
        AnimationManager.OnDestroy();
    }

    public Color HexToColor(string hex)
    {
        return Tools.HexToColor(hex);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }

        if (_region != null)
        {
            // draw some floor
            Gizmos.color = HexToColor("#000000"); // black
            //Gizmos.DrawCube (new Vector3 (0, 0, 0), new Vector3 (10000, 0, 10000));

            // set color and draw gizmos
            //int water_level = gameSession.mapGenerator.getRegion().getWaterLevelElevation();
            int order = 0;
            Color c;
            foreach (Vector3 pos in _region.GetTileVertices())
            {
                if (gizmoSkip + 1 != 0)
                {
                    order = ++order % (gizmoSkip + 1);
                    if (order != 0)
                    {
                        continue;
                    }
                }
                //if (tile.getTileType() != null)
                {
                    int elevation = (int)pos.y /*- water_level*/ ;
                    //if (tile.getTileType().GetType() == typeof(WaterTileType))
                    //{
                    //    //Debug.Log("water: elevation " + elevation);
                    //    if (elevation > -5)
                    //    {
                    //        c = hexToColor("#C2D2E7");
                    //    }
                    //    else if (elevation > -10)
                    //    {
                    //        c = hexToColor("#54B3F0");
                    //    }
                    //    else if (elevation > -25)
                    //    {
                    //        c = hexToColor("#067DED");
                    //    }
                    //    else if (elevation > -50)
                    //    {
                    //        c = hexToColor("#005F95");
                    //    }
                    //    else
                    //        c = hexToColor("#004176");
                    //}
                    //else if (tile.getTileType().GetType() == typeof(LandTileType))
                    float heighScaling = 0.1f;
                    {
                        //Debug.Log("water: elevation " + elevation);
                        if (elevation < 0)
                            c = HexToColor("#696300");
                        else if (elevation < 10 * heighScaling)
                            c = HexToColor("#00C103");
                        else if (elevation < 20 * heighScaling)
                            c = HexToColor("#59FF00");
                        else if (elevation < 30 * heighScaling)
                            c = HexToColor("#F2FF00");
                        else if (elevation < 40 * heighScaling)
                            c = HexToColor("#FFBE00");
                        else if (elevation < 50 * heighScaling)
                            c = HexToColor("#FF8C00");
                        else if (elevation < 60 * heighScaling)
                            c = HexToColor("#FF6900");
                        else if (elevation < 70 * heighScaling)
                            c = HexToColor("#E74900");
                        else if (elevation < 80 * heighScaling)
                            c = HexToColor("#E10C00");
                        else if (elevation < 90 * heighScaling)
                            c = HexToColor("#971C00");
                        else if (elevation < 100 * heighScaling)
                            c = HexToColor("#C24340");
                        else if (elevation < 115 * heighScaling)
                            c = HexToColor("#B9818A");
                        else if (elevation < 130 * heighScaling)
                            c = HexToColor("#988E8B");
                        else if (elevation < 160 * heighScaling)
                            c = HexToColor("#AEB5BD");
                        else // default
                            c = HexToColor("#FFFFFF");
                    }
                    //else
                    //    c = new Color(0, 0, 0, 0);
                    Gizmos.color = c;
                    //if (elevation < 0) {
                    //    pos.y = water_level; // if it's water, draw elevation as equal to water_level
                    //}
                    Gizmos.DrawSphere(pos, gizmoSize);
                }
            }
        }
    }
}