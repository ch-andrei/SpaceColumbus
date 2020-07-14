using System.Collections.Generic;

using Entities.Bodies;
using Entities.Capacities;

using Utilities.Events;

namespace Entities.Damageables
{
    // marker type for all damage events
    public abstract class DamageEvent : GameEvent
    {
        public List<Damage> Damages;

        public DamageEvent()
        {
            this.Damages = new List<Damage>();
        }

        public DamageEvent(Damage damage) : this()
        {
            this.Damages.Add(damage);
        }

        public DamageEvent(List<Damage> damages)
        {
            this.Damages = damages;
        }
    }

    public class DamageableComponentEvent : EntityComponentEvent
    {
        public DamageableComponent DamageableComponent => this.Component as DamageableComponent;

        public DamageableComponentEvent(DamageableComponent component) : base(component)
        {

        }
    }

    public class DamageableComponentEventGenerator : EventGenerator<DamageableComponentEvent>
    {
        private DamageableComponent _damageableComponent;

        public DamageableComponentEventGenerator(DamageableComponent damageableComponent)
        {
            this._damageableComponent = damageableComponent;
        }
    }

    public class DamageableComponent : EntityComponent,
        ICanBeDamaged,
        IWithListeners<DamageableComponentEvent>,
        IEventListener<CapacitiesComponentEvent>
    {
        public override EntityComponentType ComponentType => EntityComponentType.Damageable;
        public override string Name => "Damageable";

        public EBodyType bodyType; // TODO: have entity initializer set this
        public Body Body { get; private set; }

        public bool CanBeDamaged => this.Body.CanBeDamaged;
        public bool IsDamaged => this.Body.IsDamaged;
        public EDamageState GetDamageState() => this.Body.GetDamageState();

        public List<IEventListener<DamageableComponentEvent>> EventListeners => _eventGenerator.EventListeners;
        private DamageableComponentEventGenerator _eventGenerator;

        public bool CapacitiesDirty { get; set; }
        public CapacitiesComponent CapacitiesComponent { get; private set; }

        private float _healingAmount;

        public override void Awake()
        {
            base.Awake();

            this._eventGenerator = new DamageableComponentEventGenerator(this);
        }

        public void Start()
        {
            // Body init needs to be in Start() because BodyFactory is initialized in GameManager.Awake()
            this.Body = BodyFactory.GetBody(bodyType);

            CapacitiesComponent = EntityManager.GetComponent<CapacitiesComponent>(this.entity);
            CapacitiesComponent.AddListener(this);

            CapacitiesDirty = true;
        }

        public void AddListener(IEventListener<DamageableComponentEvent> eventListener)
        {
            this._eventGenerator.AddListener(eventListener);
        }

        public override void OnDestroy()
        {
            // do nothing
        }

        public bool OnEvent(CapacitiesComponentEvent gameEvent)
        {
            this.CapacitiesDirty = true;

            // always listen on capacities component
            return true;
        }
    }
}
