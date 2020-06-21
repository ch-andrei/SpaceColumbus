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

    public class SelectionCriteria
    {
        public enum ECondition
        {
            And,
            Or
        }

        bool _isAgent;
        bool _isBuilding;
        bool _isControllable;
        Func<bool, bool, bool> _op;
        OwnershipInfo _ownership;

        public SelectionCriteria(bool isAgent, bool isBuilding, bool isControllable, ECondition condition, OwnershipInfo ownership)
        {
            this._isAgent = isAgent;
            this._isBuilding = isBuilding;
            this._isControllable = isControllable;
            if (condition == ECondition.And) this._op = (a, b) => a & b; else this._op = (a, b) => a | b;
            this._ownership = ownership;
        }

        public static bool IsValidSelection(SelectionCriteria criteria, SelectableComponent selectableComponent)
        {
            if (criteria is null)
                return true;

            bool valid = criteria._isAgent == StaticGameDefs.IsAgent(selectableComponent.gameObject);
            valid = criteria._op(valid, criteria._isBuilding == StaticGameDefs.IsStructure(selectableComponent.gameObject));
            //valid = criteria.op(valid, criteria.isControlable != selectable.gameObject.GetComponent<Owner>());

            return valid;
        }
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
