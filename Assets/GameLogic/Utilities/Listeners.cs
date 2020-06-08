using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Events
{
    public abstract class GameEvent { }

    public interface IEventListener<T> where T : GameEvent
    {
        bool OnEvent(T gameEvent); // return false if no longer listening on the object
    }

    public interface IEventListener : IEventListener<GameEvent> { }

    public interface IWithListeners<T> where T : GameEvent
    {
        // this interface is for classes that have EventGenerators but aren't a generator
        List<IEventListener<T>> EventListeners { get; }
        void AddListener(IEventListener<T> eventListener);
    }

    public interface IEventGenerator<T> : IWithListeners<T> where T : GameEvent
    {
        void Notify(T gameEvent);
    }

    public interface IEventGenerator : IEventGenerator<GameEvent> { }

    public class EventGenerator<T> : IEventGenerator<T> where T : GameEvent
    {
        public List<IEventListener<T>> EventListeners { get; private set; }

        public EventGenerator()
        {
            EventListeners = new List<IEventListener<T>>();
        }

        public void AddListener(IEventListener<T> eventListener)
        {
            // check if already added
            foreach (var existingEventListener in EventListeners)
                if (existingEventListener == eventListener)
                    return;

            EventListeners.Add(eventListener);
        }

        public virtual void Notify(T gameEvent)
        {
            int nonActiveCount = 0;

            for (int i = 0; i < EventListeners.Count; i++)
            {
                var eventListener = EventListeners[i - nonActiveCount];

                bool active = false;
                // Listener may be destroyed or inactive
                try
                {
                    active = eventListener.OnEvent(gameEvent);
                }
                catch (NullReferenceException e)
                {
                    Debug.LogWarning("Attempted to Notify a null listener.");
                }

                if (!active)
                {
                    // remove if the listener is inactive, keep otherwise
                    EventListeners.RemoveAt(i - nonActiveCount);
                    nonActiveCount++;
                }
            }
        }
    }

    public abstract class QueuedEventGenerator<T> : EventGenerator<T> where T : GameEvent
    {
        private Queue<T> _eventQueue;

        public QueuedEventGenerator() : base()
        {
            Reset();
        }

        public void Reset()
        {
            _eventQueue = new Queue<T>();
        }

        public override void Notify(T gameEvent)
        {
            _eventQueue.Enqueue(gameEvent);
        }

        public virtual void Notify()
        {
            while (0 < _eventQueue.Count)
                base.Notify(_eventQueue.Dequeue());
        }
    }
}
