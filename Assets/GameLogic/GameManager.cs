using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using UnityEngine;
using UnityEngine.AI;

using Entities;
using Entities.Bodies;
using Entities.Damageables;
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
            EntitySystemManager.Initialize();
            PlayerManager.Initialize();
            SelectionManager.Initialize();

            // factories
            EntityMaterialFactory.Initialize(); // must be initialized before BodyPartFactory due to dependencies
            BodyFactory.Initialize();

            // initialize game
            InitializeGameSession();
        }

        public void Update()
        {
            float time = Time.time;
            float deltaTime = Time.deltaTime;

            // must be done every frame
            AnimationManager.Update(time, deltaTime);
            EntityManager.Update(time, deltaTime);
            EntitySystemManager.Update(time, deltaTime);
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

