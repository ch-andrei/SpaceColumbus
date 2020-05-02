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
        bool _isControlable;
        Func<bool, bool, bool> _op;
        OwnershipInfo _ownership;

        public SelectionCriteria(bool isAgent, bool isBuilding, bool isControlable, ECondition condition, OwnershipInfo ownership)
        {
            this._isAgent = isAgent;
            this._isBuilding = isBuilding;
            this._isControlable = isControlable;
            if (condition == ECondition.And) this._op = (a, b) => a & b; else this._op = (a, b) => a | b;
            this._ownership = ownership;
        }

        public static bool IsValidSelection(SelectionCriteria criteria, Selectable selectable)
        {
            if (criteria is null)
                return true;

            bool valid = criteria._isAgent == StaticGameDefs.IsAgent(selectable.gameObject);
            valid = criteria._op(valid, criteria._isBuilding == StaticGameDefs.IsStructure(selectable.gameObject));
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
        public Selectable selectable { get; }

        public SelectionListener(GameObject gameObject)
        {
            this.selectable = gameObject.GetComponent<Selectable>();
        }

        public bool OnEvent(SelectionEvent selectionEvent)
        {
            if (selectionEvent.IsSelected)
                selectable.Select();
            else
                selectable.Deselect();

            return true;
        }
    }

}
