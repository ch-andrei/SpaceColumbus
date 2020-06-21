using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using UnityEngine;
using UnityEngine.AI;

using Entities;
using Entities.Bodies;
using Entities.Health;
using Entities.Materials;
using Navigation;

using Utilities.Misc;
using EntitySelection;

using Players;
using UnityEngine.Serialization;

namespace GameLogic
{
    // maintains and controls all other subsystems of the game
    public class GameManager : MonoBehaviour
    {
        public GameSessionParams gameSessionParams;
        public GameSession gameSession { get; private set; }

        public void Awake()
        {
            // managers
            EntityManager.Initialize();
            PlayerManager.Initialize();
            SelectionManager.Initialize();

            // factories
            EntityMaterialFactory.Initialize(); // must be initialized before BodyPartFactory due to dependencies
            BodyPartFactory.Initialize();

            // initialize game
            InitializeGameSession();
        }

        public void FixedUpdate()
        {
            float time = Time.time;
            float deltaTime = Time.deltaTime;

            // must be done every fixed frame
            EntityManager.Update(time, deltaTime);
        }

        public void Update()
        {
            float time = Time.time;
            float deltaTime = Time.deltaTime;

            // must be done every frame
            AnimationManager.Update(time, deltaTime);
        }

        public void OnDestroy()
        {
            AnimationManager.OnDestroy();
        }

        public void InitializeGameSession()
        {
            this.gameSession = new GameSession(gameSessionParams);

            if (gameSessionParams.spawnAgents)
                for (int i = 0; i < gameSessionParams.numAgentsToSpawn; i++)
                    gameSession.SpawnSimpleAgent(
                        gameSessionParams.spawnAgentRandomDistance,
                        gameSessionParams.spawnAgentRandom);
        }
    }
}

