using Common;
using Utilities.Events;
using Utilities.XmlReader;

namespace Entities
{
    public abstract class EntityComponentSystemEvent : GameEvent
    {

    }

    public abstract class EntityComponentSystem : INamed
    {
        #region XmlDefs

        private const string SystemsXmlPath = "Assets/Defs/systems.xml";

        protected static XmlReader SystemsXml = new XmlReader(SystemsXmlPath);

        private const string VerbosityField = "Verbosity/EntityComponentSystem";
        protected static int VerbosityLevel = (int)SystemsXml.GetFloat(VerbosityField);

        private const string TimeBetweenUpdatesField = "Timed/AiSystem";
        protected static float TimeBetweenUpdatesGlobal = SystemsXml.GetFloat(TimeBetweenUpdatesField);

        #endregion XmlDefs

        public abstract string Name { get; }

        protected virtual float TimeBetweenUpdates => TimeBetweenUpdatesGlobal;

        private float _timeSinceLastUpdate;
        private bool _needUpdate;

        public EntityComponentSystem()
        {
            _timeSinceLastUpdate = 0;
            _needUpdate = true;
        }

        public void UpdateTimed(float time, float deltaTime)
        {
            _timeSinceLastUpdate += deltaTime;

            if (TimeBetweenUpdates < _timeSinceLastUpdate)
                _needUpdate = true;

            if (_needUpdate)
            {
                Update(time, deltaTime);
                _needUpdate = false;
            }
        }

        protected abstract void Update(float time, float deltaTime);
    }

    public abstract class EntityComponentSystem<T> : EntityComponentSystem, IEventListener<T>
        where T : EntityComponentSystemEvent
    {
        protected QueuedEventListener<T> _eventSystem;

        public EntityComponentSystem() : base()
        {
            _eventSystem = new QueuedEventListener<T>();
        }

        public virtual bool OnEvent(T gameEvent)
        {
            _eventSystem.OnEvent(gameEvent); // queue

            return true;
        }
    }
}
