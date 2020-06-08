using System.Collections.Generic;
using UnityEngine;

using Entities.Bodies;
using Entities.Health;

using UI.Utils;
using Utilities.Events;

namespace Entities.Health
{
    public interface IDamageable
    {
        bool IsDamageable { get; } // true if this object can be applied damage to
        bool IsDamaged { get; } // true if this object is at full health
        EDamageState GetDamageState();
        void TakeDamage(Damage damage);
    }

    public enum EDamageState : byte
    {
        None,
        Minor,
        Major,
        Critical,
        Terminal
    }

    public class DamageableEvent : EntityComponentEvent
    {
        public Entity Entity;

        public readonly List<Damage> Damages;

        public DamageableEvent(List<Damage> damages, Entity entity)
        {
            this.Damages = damages;
            this.Entity = entity;
        }
    }

    public class Damageable : EntityComponent, IDamageable, IWithListeners<DamageableEvent>
    {
        public EBodyType bodyType;

        public override EntityComponentType ComponentType => EntityComponentType.Damageable;
        public override string Name => "Damageable";

        public bool IsDamageable => this.Body.IsDamageable;
        public bool IsDamaged => this.Body.IsDamaged;
        public EDamageState GetDamageState() => this.Body.GetDamageState();

        public Body Body { get; private set; }

        public List<IEventListener<DamageableEvent>> EventListeners => Body.EventListeners;

        public void Start()
        {
            this.Body = BodyPartFactory.GetBody(bodyType);
        }

        public void TakeDamage(Damage damage)
        {
            this.Body.TakeDamage(damage);
        }

        public void AddListener(IEventListener<DamageableEvent> eventListener)
        {
            this.Body.AddListener(eventListener);
        }

        public override void OnDestroy()
        {
            // do nothing
        }
    }
}
