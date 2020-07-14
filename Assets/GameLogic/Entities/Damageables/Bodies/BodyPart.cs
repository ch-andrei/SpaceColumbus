using Common;
using Utilities.Events;

using Entities.Damageables;
using Entities.Capacities;

namespace Entities.Bodies
{
    public class BodyPartHpEvent : GameEvent
    {
        public HpSystemEvent HpSystemEvent { get; private set; }
        public BodyPart BodyPart { get; private set; }

        public BodyPartHpEvent(BodyPart bodyPart, HpSystemEvent hpSystemEvent)
        {
            this.BodyPart = bodyPart;
            this.HpSystemEvent = hpSystemEvent;
        }
    }

    public struct BodyPart : INamed, ICanBeDamaged, ICanTakeDamage<HpSystemDamageEvent>, ICloneable<BodyPart>
    {
        public HpSystem HpSystem { get; private set; }

        public string Name { get; set; }
        public string NameCustom { get; set; }

        public EBodyType BodyType { get; private set; }

        public float Size { get; set; }

        // IDamageable functions
        public bool CanBeDamaged => true;
        public bool IsDamaged => this.HpSystem.IsDamaged;

        public CapacityInfo CapacityInfo;

        public BodyPart(EBodyType bodyType, HpSystem hpSystem, string name, float size, CapacityInfo capacityInfo)
        {
            this.BodyType = bodyType;
            this.HpSystem = hpSystem;
            this.Name = name;
            this.NameCustom = name;
            this.Size = size;
            this.CapacityInfo = capacityInfo;
        }

        public BodyPart(BodyPart bodyPart) : this (
            bodyPart.BodyType,
            new HpSystem(bodyPart.HpSystem),
            string.Copy(bodyPart.Name),
            bodyPart.Size,
            bodyPart.CapacityInfo.Clone())
        {
            this.NameCustom = string.Copy(bodyPart.NameCustom);
        }

        public EDamageState GetDamageState() => this.HpSystem.GetDamageState();

        public HpSystemDamageEvent TakeDamage(Damage damage) => HpSystem.TakeDamage(damage);

        public BodyPart Clone() => new BodyPart(this);

        public override string ToString() => $"BODYPART {this.NameCustom}";
    }
}
