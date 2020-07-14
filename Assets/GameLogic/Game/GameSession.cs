using System;
using Brains;
using Entities;
using Entities.Materials;
using EntitySelection;
using UnityEngine;

using Players;

using Regions;
using SquareRegions;
using NoiseGenerators;
using HeightMapGenerators;

using Navigation;
using RegionModelGenerators;

namespace GameLogic
{
    [System.Serializable]
    public class GameSessionParams
    {
        public bool debug = false;
        public bool debugMapGen = false;

        [Header("Agent Config")] public bool spawnAgents = false;
        public int numAgentsToSpawn = 2000;
        public bool spawnAgentRandom = false;
        public float spawnAgentRandomDistance = 1000f;

        [Header("Region Config")] public bool generateRegion = true;
        [Range(1, 100000000)] public int seed = 0;
        public bool useRandomSeed;
        public bool regenerate = false;

        public RegionGenConfig regionGenConfig;
        public HeightMapConfig heightMapConfig;
        public FastPerlinNoiseConfig noiseConfig;
        public ErosionConfig erosionConfig;
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
                this._seed, _params.regionGenConfig, _params.heightMapConfig, _params.noiseConfig,
                _params.erosionConfig);
        }

        public void BuildRegionView()
        {
            var viewablesRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.ViewablesTag);

            foreach (Transform t in viewablesRoot.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            var regionView = new GameObject(StaticGameDefs.RegionViewObjectName);
            regionView.transform.parent = viewablesRoot.transform;

            var squareRegionModelGenerator =
                regionView.AddComponent<RegionModelGenerators.SquareRegionModelGenerator>();
            squareRegionModelGenerator.InitializeMesh(this.region);
        }

        public void BuildNavMeshes()
        {
            var navMeshRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.NavMeshRootTag);
            var navMeshGenerator = navMeshRoot.GetComponent<NavMeshGenerator>();
            navMeshGenerator.BuildNavMesh();
        }

        public void SpawnSimpleAgent(float distance, bool random = false)
        {
            Vector3 pos;
            if (random)
            {
                Vector2 rc = UnityEngine.Random.onUnitSphere;
                pos = distance * new Vector3(rc.x, 0, rc.y);
            }
            else
            {
                pos = Vector3.zero;
                pos.y = region.GetTileAt(new Vector3()).Pos.y;
            }

            SpawnSimpleAgent(pos);
        }

        public void SpawnSimpleAgent(Vector3 position)
        {
            var agentRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.AgentRootTag);

            var gameRoot = GameObject.FindGameObjectWithTag(StaticGameDefs.GameRootTag);
            var agentPrefab = gameRoot.GetComponent<PrefabManager>().agentPrefab;

            GameObject.Instantiate(agentPrefab, position, agentPrefab.transform.rotation, agentRoot.transform);
        }

        public void MoveSelected(Vector3 destination)
        {
            var selectedObjects = SelectionManager.GetSelectedObjects();
            foreach (var selectedObject in selectedObjects)
            {
                try
                {
                    var ai = EntityManager.GetComponent<AIComponent>(selectedObject);
                    ai.MoveTo(destination);
                }
                // these may occur when an object is destroyed or if the object does not have AIComponent
                catch (MissingReferenceException e)
                {
                }
                catch (NullReferenceException e)
                {
                }
            }
        }

        public void StopSelectedAgents()
        {
            var selectedObjects = SelectionManager.GetSelectedObjects();
            foreach (var selectedObject in selectedObjects)
            {
                try
                {
                    var ai = EntityManager.GetComponent<AIComponent>(selectedObject);
                    ai.StopMoving();
                }
                // these may occur when an object is destroyed or if the object does not have AIComponent
                catch (MissingReferenceException e)
                {
                }
                catch (NullReferenceException e)
                {
                }
            }
        }


    }
}
