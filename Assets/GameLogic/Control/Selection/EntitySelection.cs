using UnityEngine;
using System;
using System.Collections;

using Utilities.Events;
using Players;

namespace EntitySelection
{
    public interface ISelectable
    {
        void Select();
        void Deselect();
    }

    public class SelectionEvent : GameEvent
    {
        public bool IsSelected;

        public SelectionEvent(bool isSelected)
        {
            this.IsSelected = isSelected;
        }
    }

    public class SelectionListener : IEventListener<SelectionEvent>
    {
        public SelectableComponent SelectableComponent { get; }

        public SelectionListener(SelectableComponent selectableComponent)
        {
            this.SelectableComponent = selectableComponent;
        }

        public bool OnEvent(SelectionEvent selectionEvent)
        {
            if (selectionEvent.IsSelected)
                SelectableComponent.Select();
            else
                SelectableComponent.Deselect();

            // selection listener always listens
            return true;
        }
    }
}
