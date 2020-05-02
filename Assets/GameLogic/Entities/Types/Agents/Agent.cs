using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.AI;

using Brains;
using Brains.Movement;
using Brains.Attack;
using EntitySelection;

using Entities.Bodies;
using Entities.Bodies.Damages;
using Entities.Bodies.Health;

using Utilities.Events;

namespace Entities
{
    public class AgentChangedEvent : EntityChangeEvent
    {
        public AgentChangedEvent(Agent agent) : base(agent) { }
    }

    public class AgentEventGenerator : EventGenerator<AgentChangedEvent>, IEventListener<BodyPartChangedEvent>
    {
        private Agent _agent;
        public AgentEventGenerator(Agent agent) : base() { this._agent = agent; }

        public bool OnEvent(BodyPartChangedEvent bodyChangedEvent)
        {
            // TODO: any processing on event

            this.Notify(new AgentChangedEvent(this._agent));

            return true;
        }
    }

    [RequireComponent(
        typeof(Selectable),
        typeof(NavMeshAgent)
     )]
    public class Agent : Entity, IEventGenerator<AgentChangedEvent>
    {
        public Body Body { get; private set; }

        public override string Name { get { return "Agent"; } }

        public override bool IsDamageable { get { return this.Body.IsDamageable; } }
        public override bool IsDamaged { get { return this.Body.IsDamaged; } }

        AgentBrain _brain;

        AgentEventGenerator _agentEventSystem;

        public void Awake()
        {
            this.entityType = EntityType.Agent;
        }

        public override void Start()
        {
            base.Start();

            this.Body = Body.HumanoidBody;

            Debug.Log($"Agent with body:\n{Body.ToString()}");

            var moveBrain = new MoveBrain(this.GetComponent<NavMeshAgent>());
            var attackBrain = new AttackBrain();
            _brain = new AgentBrainModerate(this.gameObject, moveBrain, attackBrain);

            _agentEventSystem = new AgentEventGenerator(this);
            this.Body.AddListener(_agentEventSystem);
        }

        public void MoveTo(Vector3 destination)
        {
            this._brain.MoveTo(destination);
        }

        public void Stop() {
            this._brain.StopMoving();
        }

        void FixedUpdate()
        {
            if (UnityEngine.Random.value < 0.005f)
            {
                this.TakeDamage(new Damage(DamageType.Blunt, 5, 0.1f));
            }

            _brain.ProcessTick();
        }

        public override void TakeDamage(Damage damage)
        {
            Body.TakeDamage(damage);
        }

        public override EDamageState GetDamageState()
        {
            return Body.GetDamageState();
        }

        public void AddListener(IEventListener<AgentChangedEvent> eventListener)
        {
            this._agentEventSystem.AddListener(eventListener);
        }

        public void Notify(AgentChangedEvent gameEvent)
        {
            // not intended to be called
            throw new System.NotImplementedException();
        }
    }
}

