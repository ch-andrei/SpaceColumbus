using System;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Common;

using Utilities.Misc;

using Entities.Bodies.Damages;
using Entities.Bodies.Health;

using Utilities.Events;

namespace Entities.Bodies
{
    public class BodyPartChangedEvent : EntityComponentChangedEvent
    {
        public BodyPart bodyPart { get; private set; }
        public EntityComponentChangedEvent bodyChangedEvent { get; private set; }

        public BodyPartChangedEvent(BodyPart bodyPart, EntityComponentChangedEvent bodyChangedEvent) : base()
        {
            this.bodyPart = bodyPart;
            this.bodyChangedEvent = bodyChangedEvent;
        }
    }

    public class BodyPartChangedEventGenerator :
    EventGenerator<BodyPartChangedEvent>,
    IEventListener<BodyPartChangedEvent>, // from other BodyParts
    IEventListener<HpSystemChangedEvent> // from HpSystem
    {
        BodyPart _bodyPart;

        public BodyPartChangedEventGenerator(BodyPart bodyPart) : base() { this._bodyPart = bodyPart; }

        public bool OnEvent(HpSystemChangedEvent hpSystemEvent)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(
                $"{_bodyPart.NameCustom} HpSystemEvent: {hpSystemEvent.HpSystem.HpPrev}->{hpSystemEvent.HpSystem.HpCurrent}HP:");
            foreach (var damage in hpSystemEvent.Damages)
                sb.Append(
                    $"\t{Damage.DamageType2Str(damage.DamageType)} damage with {damage.Amount} total damage amount;");
            Debug.Log(sb.ToString());

            // for external observers, e.g. UI
            Notify(new BodyPartChangedEvent(this._bodyPart, hpSystemEvent));

            return true;
        }

        public bool OnEvent(BodyPartChangedEvent bodyPartChangedEvent)
        {
            // for external observers, e.g. UI
            Notify(bodyPartChangedEvent);

            return true;
        }
    }

    public class BodyPart : INamed, IDamageable, IWithListeners<BodyPartChangedEvent>
    {
        public HpSystem HpSystem = null;

        public string Name { get; set; }

        public string NameCustom { get; set; }

        public virtual float Size { get; set; }

        public bool HasHpSystem { get { return this.HpSystem != null; } }

        public virtual bool IsDamageable { get { return HasHpSystem; } } // can be defined differently
        public virtual bool IsDamaged { get { return HasHpSystem && this.HpSystem.IsDamaged; } } // can be defined differently

        protected BodyPartChangedEventGenerator BodyPartChangedSystem;

        public BodyPart(string name, float size=0)
        {
            this.Name = name;
            this.NameCustom = name;
            this.Size = size;

            this.BodyPartChangedSystem = new BodyPartChangedEventGenerator(this);
        }

        public BodyPart(string name, float size, HpSystem hpSystem) : this(name, size)
        {
            this.HpSystem = hpSystem;

            if (this.HasHpSystem)
                this.HpSystem.AddListener(this.BodyPartChangedSystem);
        }

        public BodyPart(BodyPart bodyPart) : this (
            new string(bodyPart.Name.ToCharArray()),
            bodyPart.Size,
            (bodyPart.HpSystem == null) ? null : new HpSystem(bodyPart.HpSystem)
            )
        {
            this.NameCustom = bodyPart.NameCustom;
        }

        public virtual EDamageState GetDamageState()
        {
            return (HasHpSystem) ? this.HpSystem.GetDamageState() : EDamageState.None;
        }

        public virtual void TakeDamage(Damage damage)
        {
            if (this.HasHpSystem)
                HpSystem.TakeDamage(damage);
        }

        public virtual BodyPart Clone()
        {
            return new BodyPart(this);
        }

        public void AddListener(IEventListener<BodyPartChangedEvent> eventListener)
        {
            this.BodyPartChangedSystem.AddListener(eventListener);
        }

        public virtual string GetHealthInfo()
        {
            if (this.IsDamaged)
                return this.HpSystem.AsText;
            else
                return "";
        }

        public virtual string ToString()
        {
            return $"BODYPART {this.NameCustom}";
        }
    }

    public class BodyPartContainer : BodyPart
    {
        public List<BodyPart> BodyParts { get; private set; }

        public override bool IsDamageable
        { 
            get 
            { 
                bool damageable = this.HasHpSystem;

                foreach (var bodyPart in this.BodyParts)
                    damageable |= bodyPart.IsDamageable;

                return damageable;
            } 
        }

        public override bool IsDamaged 
        {
            get
            {
                bool damaged = base.IsDamaged;

                foreach (var bodyPart in this.BodyParts)
                    damaged |= bodyPart.IsDamaged;

                return damaged;
            }
        }

        public List<float> GetBodyPartSizes
        {
            get
            {
                List<float> sizes = new List<float>();
                foreach (var bodyPart in BodyParts)
                {
                    sizes.Add(bodyPart.Size);
                }
                return sizes;
            }
        }

        public BodyPartContainer(string name) : base(name)
        {
            this.BodyParts = new List<BodyPart>();
        }

        public BodyPartContainer(string name, float size, HpSystem hpSystem) : base(name, size, hpSystem)
        {
            this.BodyParts = new List<BodyPart>();
        }

        public BodyPartContainer(BodyPart bodyPart) : base(bodyPart)
        {
            this.BodyParts = new List<BodyPart>();
        }

        public BodyPartContainer(BodyPartContainer bodyPartContainer) : this(bodyPartContainer as BodyPart)
        {
            foreach (var bodyPart in bodyPartContainer.BodyParts) 
            {
                AddBodyPart(bodyPart.Clone());
            }
        }

        public void AddBodyPart(BodyPart bodyPart)
        {
            this.BodyParts.Add(bodyPart);

            if (bodyPart.HasHpSystem)
                bodyPart.AddListener(this.BodyPartChangedSystem);
        }

        public void AddBodyParts(List<BodyPart> bodyParts)
        {
            foreach (var bodyPart in bodyParts)
                AddBodyPart(bodyPart);
        }

        public override void TakeDamage(Damage damage)
        {
            base.TakeDamage(damage);

            if (this.BodyParts.Count == 1)
                this.BodyParts[0].TakeDamage(damage);
            else
            {
                Vector2Int indices = Samplers.SampleFromPdf(UnityEngine.Random.value, this.GetBodyPartSizes, damage.Dispersion);

                foreach (var bodyPart in this.BodyParts.ToArray().Slice(indices.x, indices.y + 1))
                {
                    bodyPart.TakeDamage(damage);
                }
            }
        }

        public override EDamageState GetDamageState()
        {
            EDamageState worstDamageState = base.GetDamageState();
            foreach (var bodyPart in this.BodyParts)
                worstDamageState = DamageStates.GetWorstDamageState(worstDamageState, bodyPart.GetDamageState());
            return worstDamageState;
        }

        public override BodyPart Clone()
        {
            return new BodyPartContainer(this);
        }

        public override string GetHealthInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (this.HasHpSystem)
                sb.Append($"HP: {this.HpSystem.AsText}\n");

            foreach (var bodyPart in this.BodyParts)
            {
                if (bodyPart.IsDamaged)
                    sb.Append($"{bodyPart.NameCustom} {bodyPart.GetHealthInfo()}\n");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"BODYPARTCONTAINER: {this.NameCustom}");

            foreach (var bodyPart in this.BodyParts)
            {
                sb.Append($",{bodyPart.NameCustom}");
            }

            return sb.ToString();
        }
    }
    
    public class Body : BodyPartContainer
    {
        public static Body HumanoidBody { get { return BodyPartFactory.HumanoidBody; } }

        public Body(string name) : base(name) { }

        public Body(Body body) : base(body as BodyPartContainer) { }

        public override BodyPart Clone()
        {
            return new Body(this);
        }
    }
}
