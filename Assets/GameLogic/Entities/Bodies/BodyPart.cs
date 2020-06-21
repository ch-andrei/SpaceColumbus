using Common;
using Utilities.Events;

using Entities.Health;

namespace Entities.Bodies
{
    public class BodyPartHpEvent : GameEvent
    {
        public HpSystemEvent HpEvent { get; private set; }
        public BodyPart BodyPart { get; private set; }

        public BodyPartHpEvent(BodyPart bodyPart, HpSystemEvent hpEvent)
        {
            this.BodyPart = bodyPart;
            this.HpEvent = hpEvent;
        }
    }

    public struct BodyPart : INamed, IDamageable, ICloneable<BodyPart>
    {
        public HpSystem HpSystem { get; private set; }

        public string Name { get; set; }
        public string NameCustom { get; set; }

        public float Size { get; set; }

        // public EntityMaterial Material {get => HpSystem.ma}

        // IDamageable functions
        public bool CanBeDamaged => true;
        public bool IsDamaged => this.HpSystem.IsDamaged;

        public BodyPart(HpSystem hpSystem, string name, float size=0)
        {
            this.HpSystem = hpSystem;
            this.Name = name;
            this.NameCustom = name;
            this.Size = size;
        }

        public BodyPart(BodyPart bodyPart) : this (
            new HpSystem(bodyPart.HpSystem),
            string.Copy(bodyPart.Name),
            bodyPart.Size)
        {
            this.NameCustom = string.Copy(bodyPart.NameCustom);
        }

        public EDamageState GetDamageState() => this.HpSystem.GetDamageState();

        public DamageEvent TakeDamage(Damage damage) => HpSystem.TakeDamage(damage);

        public BodyPart Clone() => new BodyPart(this);

        public string ToString() => $"BODYPART {this.NameCustom}";
    }
}
