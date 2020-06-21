using System;
using System.Collections.Generic;
using System.Text;
using Common;
using UnityEngine;

using Entities.Bodies;
using Entities.Health;
using Packages.Rider.Editor;
using UI.Utils;
using Utilities.Events;
using Utilities.Misc;

namespace Entities.Health
{
    public interface IDamageable
    {
        bool CanBeDamaged { get; } // true if this object can be applied damage to
        bool IsDamaged { get; } // true if this object is at full health
        EDamageState GetDamageState();
        DamageEvent TakeDamage(Damage damage);
    }

    public abstract class DamageEvent : GameEvent { } // marker type for all damage events

    public enum EDamageState : byte
    {
        None,
        Minor,
        Major,
        Critical,
        Terminal
    }

    public class DamageableComponentEvent : EntityComponentEvent
    {
        public DamageableComponent DamageableComponent => this.Component as DamageableComponent;
        public Damage Damage;

        public DamageableComponentEvent(DamageableComponent component, Damage damage) : base(component)
        {
            this.Damage = damage;
        }
    }

    public class DamageableEventGenerator : EventGenerator<DamageableComponentEvent>
    {
        private DamageableComponent _damageableComponent;

        public DamageableEventGenerator(DamageableComponent damageableComponent)
        {
            this._damageableComponent = damageableComponent;
        }

        public bool OnEvent(BodyDamageEvent dmgEvent, Damage damage)
        {
            // process events
            foreach (var hpEvents in dmgEvent.BodyPartHpEvents)
            {
                var hpEvent = hpEvents.HpEvent;
                var bodyPart = hpEvents.BodyPart;
                Debug.Log($"[HP event] {bodyPart.NameCustom}: {hpEvent.HpPrev}->{hpEvent.HpCurrent}HP:");
            }

            var damageableEvent = new DamageableComponentEvent(this._damageableComponent, damage);
            NotifyListeners(damageableEvent);

            return true;
        }
    }

    public class DamageableSystem : EntityComponentSystem
    {
        private const string DamageableSystemName = "Damageable System";
        public override string Name => DamageableSystemName;

        public override void Update(float time, float deltaTime)
        {
            var damageables = EntityManager.GetComponents<DamageableComponent>();

            // EntityManager.GetComponents(EntityComponentType.Damageable);

            Debug.Log($"Damageable System update got damageables count [{damageables.Count}]");

            Test(damageables);
        }

        public static DamageableSystem GetInstance()
        {
            if (_instance is null)
                _instance = new DamageableSystem();

            return _instance;
        }

        private static DamageableSystem _instance;

        private DamageableSystem()
        {

        }

        private void Test(List<DamageableComponent> damageables)
        {
            foreach (var damageable in damageables)
            {
                if (UnityEngine.Random.value < 0.005f)
                {
                    damageable.TakeDamage(Damage.PiercingDamage(5));
                }
            }
        }

        private void UpdateInstance()
        {

        }
    }


    public class DamageableComponent : EntityComponent, IDamageable, IWithListeners<DamageableComponentEvent>
    {
        public EBodyType bodyType;

        public override EntityComponentType ComponentType => EntityComponentType.Damageable;
        public override string Name => "Damageable";

        public bool CanBeDamaged => this.Body.CanBeDamaged;
        public bool IsDamaged => this.Body.IsDamaged;
        public EDamageState GetDamageState() => this.Body.GetDamageState();

        public Body Body { get; private set; }

        public List<IEventListener<DamageableComponentEvent>> EventListeners => _eventGenerator.EventListeners;
        private DamageableEventGenerator _eventGenerator;

        public void Start()
        {
            this._eventGenerator = new DamageableEventGenerator(this);
            this.Body = BodyPartFactory.GetBody(bodyType);

            Debug.Log(Body);
        }

        public DamageEvent TakeDamage(Damage damage)
        {
            Debug.Log("Damageable taking damage: " + Body);

            var dmgEvent = this.Body.TakeDamage(damage) as BodyDamageEvent;
            _eventGenerator.OnEvent(dmgEvent, damage);
            return dmgEvent;
        }

        public void AddListener(IEventListener<DamageableComponentEvent> eventListener)
        {
            this._eventGenerator.AddListener(eventListener);
        }

        public override void OnDestroy()
        {
            // do nothing
        }
    }
}
