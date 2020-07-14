using System;
using System.Collections.Generic;
using Common;
using Entities.Bodies;
using Entities.Damageables;
using UnityEngine;
using Utilities.Events;

namespace Entities.Capacities
{
    public class CapacitiesComponentEvent : EntityComponentEvent
    {
        public CapacitiesComponent CapacitiesComponent => this.Component as CapacitiesComponent;

        public CapacitiesComponentEvent(EntityComponent component) : base(component)
        {
        }
    }

    public class CapacitiesEventGenerator : EventGenerator<CapacitiesComponentEvent>
    {
        private CapacitiesComponent _capacitiesComponent;

        public CapacitiesEventGenerator(CapacitiesComponent capacitiesComponent)
        {
            this._capacitiesComponent = capacitiesComponent;
        }

        public bool OnEvent()
        {
            var capacitiesEvent = new CapacitiesComponentEvent(this._capacitiesComponent);
            NotifyListeners(capacitiesEvent);

            return true;
        }
    }

    [RequireComponent(typeof(DamageableComponent))]
    public class CapacitiesComponent : EntityComponent, IEventListener<DamageableComponentEvent>,
        IEventGenerator<CapacitiesComponentEvent>
    {
        // these are the current capacities values given current health or other relevant factors
        public CapacityInfo capacityInfoCurrent;

        // these are the base capacities values when at full health; usually all capacities are 1.0, unless debuffs
        public CapacityInfo CapacityInfoBase { get; private set; }

        // list of current modifiers
        public List<CapacityInfoModifier> CapacitiesModifiers { get; set; }
        public bool CapacitiesDirty { get; set; }

        // public Queue<BodyDamageEvent> DamageEventQueue { get; set; }

        public override EntityComponentType ComponentType => EntityComponentType.Capacities;
        public override string Name => "Capacities Component";

        public DamageableComponent DamageableComponent { get; private set; }

        private CapacitiesEventGenerator _eventGenerator;

        public List<IEventListener<CapacitiesComponentEvent>> EventListeners => _eventGenerator.EventListeners;

        public void AddListener(IEventListener<CapacitiesComponentEvent> eventListener) =>
            _eventGenerator.AddListener(eventListener);

        public void NotifyListeners() => NotifyListeners(new CapacitiesComponentEvent(this));
        public void NotifyListeners(CapacitiesComponentEvent gameEvent) =>
            _eventGenerator.NotifyListeners(gameEvent);

        public override void Awake()
        {
            base.Awake();

            CapacitiesModifiers = new List<CapacityInfoModifier>();
            CapacitiesDirty = false;

            CapacityInfoBase = new CapacityInfo(1f);
            CapacityInfoBase.SetCapacity(ECapacityType.Pain, 0f);
            capacityInfoCurrent = this.CapacityInfoBase.Clone();

            _eventGenerator = new CapacitiesEventGenerator(this);
        }

        public void Start()
        {
            // listen on damageable component
            // this needs to be in Start, because Damageable component event system is initialized in Awake
            DamageableComponent = GetComponent<DamageableComponent>();
            DamageableComponent.AddListener(this);
        }

        public bool AddCapacitiesModifier(CapacityInfoModifier modifier, bool addOnlyIfUnique = false)
        {
            bool needAdd = true;

            if (addOnlyIfUnique)
            {
                // TODO: test if modifier is already present
                // needAdd = false
            }

            if (needAdd)
                this.CapacitiesModifiers.Add(modifier);

            return true;
        }

        public override void OnDestroy()
        {
            // do nothing
        }

        public bool OnEvent(DamageableComponentEvent gameEvent)
        {
            CapacitiesDirty = true;

            return true;
        }
    }

    public class CapacitiesSystem : EntityComponentSystem
    {
        private const string CapacitiesSystemName = "Capacities System";
        public override string Name => CapacitiesSystemName;

        private const string TimeBetweenUpdatesField = "Timed/CapacitiesSystem";
        protected static float _timeBetweenUpdatesDamageable = SystemsXml.GetFloat(TimeBetweenUpdatesField);

        protected override float TimeBetweenUpdates => _timeBetweenUpdatesDamageable;

        private const string CapacitiesModifiersField = "CapacitiesModifiers";
        private const string TerminalDamageStateModifierAmountField = "TerminalDamageState";
        private const string CriticalDamageStateModifierAmountField = "CriticalDamageState";
        private const string MajorDamageStateModifierAmountField = "MajorDamageState";
        private const string MinorDamageStateModifierAmountField = "MinorDamageState";
        private const string NoneDamageStateModifierAmountField = "NoneDamageState";

        private static float TerminalDamageStateModifierAmount =
            SystemsXml.GetFloat(new List<string>() { CapacitiesModifiersField, TerminalDamageStateModifierAmountField });
        private static float CriticalDamageStateModifierAmount =
            SystemsXml.GetFloat(new List<string>() { CapacitiesModifiersField, CriticalDamageStateModifierAmountField });
        private static float MajorDamageStateModifierAmount =
            SystemsXml.GetFloat(new List<string>() { CapacitiesModifiersField, MajorDamageStateModifierAmountField });
        private static float MinorDamageStateModifierAmount =
            SystemsXml.GetFloat(new List<string>() { CapacitiesModifiersField, MinorDamageStateModifierAmountField });
        private static float NoneDamageStateModifierAmount =
            SystemsXml.GetFloat(new List<string>() { CapacitiesModifiersField, NoneDamageStateModifierAmountField });

        public CapacitiesSystem() : base()
        {

        }

        public static float DamageStateToModifierAmount(EDamageState damageState)
        {
            switch (damageState)
            {
                case EDamageState.Terminal:
                    return TerminalDamageStateModifierAmount;
                case EDamageState.Critical:
                    return CriticalDamageStateModifierAmount;
                case EDamageState.Major:
                    return MajorDamageStateModifierAmount;
                case EDamageState.Minor:
                    return MinorDamageStateModifierAmount;
                default:
                    return NoneDamageStateModifierAmount;
            }
        }

        protected override void Update(float time, float deltaTime)
        {
            var capacitiesComponents = EntityManager.GetComponents<CapacitiesComponent>();

            foreach (var capacitiesComponent in capacitiesComponents)
            {
                // first: check existing modifiers, remove if duration has expired
                int removedCount = 0;
                int count = capacitiesComponent.CapacitiesModifiers.Count;
                for (int i = 0; i < count; i++)
                {
                    // check expiration time and remove if needed
                    var modifier = capacitiesComponent.CapacitiesModifiers[i - removedCount];

                    float timeSinceSpawn = time - modifier.timed.TimeAtSpawn;
                    if (modifier.timed.Duration <= timeSinceSpawn)
                    {
                        capacitiesComponent.CapacitiesModifiers.RemoveAt(i);
                        removedCount++;
                    }
                }

                // if modifiers were changed, mark component to be updated
                capacitiesComponent.CapacitiesDirty |= 0 < removedCount;

                if (capacitiesComponent.CapacitiesDirty)
                {
                    // get the base value of capacities
                    var capacities = capacitiesComponent.CapacityInfoBase.Clone();

                    // apply modifiers
                    foreach (var modifier in capacitiesComponent.CapacitiesModifiers)
                    {
                        CapacityInfoModifier.Apply(modifier, ref capacities);
                    }

                    // apply modifiers given bodyPart's damage state
                    foreach (var bodyPart in capacitiesComponent.DamageableComponent.Body.BodyParts)
                    {
                        // get bodyPart's capacities influence
                        var capacitiesBaseBp = bodyPart.CapacityInfo;

                        // convert bodyPart's damage state to impact multiplier
                        var damageState = bodyPart.GetDamageState();
                        float capacityImpact = DamageStateToModifierAmount(damageState);

                        // apply modifier
                        capacitiesBaseBp *= capacityImpact;

                        // subtract from base component capacities
                        capacities -= capacitiesBaseBp;
                    }

                    // write back
                    capacitiesComponent.capacityInfoCurrent = capacities;

                    capacitiesComponent.CapacitiesDirty = false;

                    capacitiesComponent.NotifyListeners();
                }
            }
        }
    }
}
