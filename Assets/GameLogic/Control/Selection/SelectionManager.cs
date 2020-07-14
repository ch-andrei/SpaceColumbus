using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Profiling;

using Entities;
using Players;

using Utilities.Events;

namespace EntitySelection
{
    public class SelectionCriteria
    {
        public enum ECondition
        {
            And,
            Or
        }

        public bool _isAgent;
        public bool _isBuilding;
        public bool _isControllable;
        public Func<bool, bool, bool> _op;
        OwnershipInfo _ownership;

        public SelectionCriteria(bool isAgent, bool isBuilding, bool isControllable, ECondition condition, OwnershipInfo ownership)
        {
            this._isAgent = isAgent;
            this._isBuilding = isBuilding;
            this._isControllable = isControllable;
            if (condition == ECondition.And) this._op = (a, b) => a & b; else this._op = (a, b) => a | b;
            this._ownership = ownership;
        }
    }

    public static class SelectionManager
    {
        #region Config
        public static float TimeBetweenSelectionUpdates = 0.05f; // in seconds, minimum allowed Period for updating selection
        #endregion Config

        private static List<SelectionListener> _currentlySelectedListeners;
        private static List<GameObject> _currentlySelectedGameObjects;

        private static Dictionary<int, SelectionListener> _selectionListeners;

        private static Vector3[] _selectionScreenCoords = new Vector3[2];

        private static float _timeSinceLastSelectionUpdate;

        public static GameObject MouseOverObject { get; private set; }

        public static bool Dirty { get; set; }

        public static void Initialize()
        {
            _selectionListeners = new Dictionary<int, SelectionListener>();
            _timeSinceLastSelectionUpdate = TimeBetweenSelectionUpdates;
            ProcessSelected();
        }

        public static void AddSelectable(SelectableComponent selectableComponent)
        {
            int id = selectableComponent.Guid;

            if (!_selectionListeners.ContainsKey(id))
                _selectionListeners.Add(id, selectableComponent.selectionListener);
        }

        public static void RemoveSelectable(SelectableComponent selectableComponent)
        {
            int id = selectableComponent.Guid;

            if (_selectionListeners.ContainsKey(id))
                _selectionListeners.Remove(id);
        }

        //public List<SelectionListener> GetSelectedListeners() { return this.currentlySelectedListeners; }

        public static List<GameObject> GetSelectedObjects() => _currentlySelectedGameObjects;

        //public List<GameObject> GetSelectedObjects(SelectionCriteria criteria)
        //{
        //    return this.currentlySelectedGameObjects;
        //}

        private static void ProcessSelected()
        {
            var selectedListeners = new List<SelectionListener>();
            var selectedObjects = new List<GameObject>();

            foreach (var selectionListener in _selectionListeners.Values)
            {
                var selectable = selectionListener.SelectableComponent;
                if (selectionListener.SelectableComponent.isSelected)
                {
                    selectedListeners.Add(selectionListener);
                    selectedObjects.Add(selectable.gameObject);
                }
            }

            _currentlySelectedListeners = selectedListeners;
            _currentlySelectedGameObjects = selectedObjects;
        }

        public static void CheckMissingSelected()
        {
            int removed = 0;
            for (int i = 0; i < _currentlySelectedListeners.Count; i++)
            {
                int index = i - removed;

                var listener = _currentlySelectedListeners[index];
                var go = _currentlySelectedGameObjects[index];

                if (listener is null || go is null)
                {
                    _currentlySelectedListeners.RemoveAt(index);
                    _currentlySelectedGameObjects.RemoveAt(index);
                }
            }
        }

        //public void SetDirty(bool dirty) { this.dirty = dirty; }
        //public void SetDirty() { SetDirty(true); }

        public static void DeselectAll()
        {
            MouseOverObject = null;

            foreach (var selectionListener in _selectionListeners.Values)
            {
                var selectable = selectionListener.SelectableComponent;
                if (selectable.isSelected)
                    selectable.Deselect();
            }

            ProcessSelected();
        }

        public static List<SelectableComponent> GetSelectables(GameObject gameObject)
        {
            if (gameObject is null)
                return new List<SelectableComponent>();

            return EntityManager.GetComponents<SelectableComponent>(gameObject);
        }

        public static void Deselect(GameObject gameObject)
        {
            foreach (var selectable in GetSelectables(gameObject))
            {
                Deselect(selectable);
            }
        }

        public static void Deselect(SelectableComponent selectableComponent)
        {
            if (selectableComponent.isSelected)
                selectableComponent.Deselect();
        }

        public static void Select(SelectableComponent selectableComponent, SelectionCriteria selectionCriteria = null)
        {
            if (!selectableComponent.isSelected && IsValidSelection(selectionCriteria, selectableComponent))
                selectableComponent.Select();
        }

        public static void Select(GameObject gameObject, SelectionCriteria selectionCriteria=null)
        {
            foreach (var selectable in GetSelectables(gameObject))
            {
                Select(selectable, selectionCriteria);
            }
        }

        public static void UpdateMouseSelection(GameObject mouseOverObject, SelectionCriteria selectionCriteria)
        {
            // // TODO: optimize this
            // if (MouseOverObject == mouseOverObject)
            //     return;

            Deselect(MouseOverObject);
            MouseOverObject = mouseOverObject;
            Select(MouseOverObject, selectionCriteria);
        }

        public static void UpdateSelected(Vector3 s1, Vector3 s2, GameObject mouseOverObject, SelectionCriteria selectionCriteria = null)
        {
            if (Dirty || CheckDirty(s1, s2))
            {
                Dirty = false;

                // update controls vars
                _timeSinceLastSelectionUpdate = 0f;

                UpdateBoxSelection(s1, s2, selectionCriteria);
                UpdateMouseSelection(mouseOverObject, selectionCriteria);
                ProcessSelected();
            }
            else
            {
                CheckMissingSelected();
            }
        }

        public static bool IsValidSelection(SelectionCriteria criteria, SelectableComponent selectableComponent)
        {
            if (criteria is null)
                return true;

            bool valid = criteria._isAgent == selectableComponent.entity.isAgent;
            valid = criteria._op(valid, criteria._isBuilding == selectableComponent.entity.isStructure);
            //valid = criteria.op(valid, criteria.isControllable != selectable.gameObject.GetComponent<Owner>());

            return valid;
        }

        public static void UpdateBoxSelection(Vector3 s1, Vector3 s2, SelectionCriteria selectionCriteria = null)
        {
            Profiler.BeginSample("UpdateBoxSelection");

            foreach (var selectionListener in _selectionListeners.Values)
            {
                var selectable = selectionListener.SelectableComponent;

                var p = Camera.main.WorldToScreenPoint(selectable.position);
                var s1P = Vector2.Min(s1, s2);
                var s2P = Vector2.Max(s1, s2);
                bool selected = s1P.x <= p.x && p.x <= s2P.x && s1P.y <= p.y && p.y <= s2P.y;

                // update and notify only selection changes
                if (selected && IsValidSelection(selectionCriteria, selectable))
                    Select(selectable); // no need to check selectable again
                else
                    Deselect(selectable);
            }

            Profiler.EndSample();
        }

        private static bool CheckDirty(Vector3 s1, Vector3 s2)
        {
            bool dirty = false;

            _timeSinceLastSelectionUpdate += Time.deltaTime;

            var selectionScreenCoordsNew = new List<Vector3>() { s1, s2 };
            selectionScreenCoordsNew = selectionScreenCoordsNew.OrderBy(v => v.x).ToList();

            if (_selectionScreenCoords[0] != selectionScreenCoordsNew[0] || _selectionScreenCoords[1] != selectionScreenCoordsNew[1])
            {
                dirty = true;
                _selectionScreenCoords[0] = selectionScreenCoordsNew[0];
                _selectionScreenCoords[1] = selectionScreenCoordsNew[1];
            }

            // harder control
            dirty &= _timeSinceLastSelectionUpdate >= TimeBetweenSelectionUpdates;
            dirty |= _timeSinceLastSelectionUpdate >= TimeBetweenSelectionUpdates;

            return dirty;
        }
    }
}
