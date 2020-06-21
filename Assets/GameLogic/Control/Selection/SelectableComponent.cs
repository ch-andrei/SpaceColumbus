using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities.Events;

using Common;
using Entities;

namespace EntitySelection
{
    public class SelectableComponent : EntityComponent, ISelectable, IIdentifiable
    {
        public override EntityComponentType ComponentType => EntityComponentType.Selectable;

        public int Guid => this.entity.GetInstanceID();

        public override string Name => "Selectable";

        public SelectionListener selectionListener { get; private set; }
        public bool isSelected { get; private set; }

        public Vector3 position => this.entity.Position;
        public GameObject gameObject => this.entity.gameObject;
        public GameObject selectionIndicator;

        public void Start()
        {
            selectionListener = new SelectionListener(this);

            isSelected = true;
            Deselect();

            SelectionManager.AddSelectable(this);
        }

        public void Select()
        {
            if (!isSelected)
            {
                selectionIndicator.SetActive(true);
            }
            isSelected = true;
        }

        public void Deselect()
        {
            if (isSelected)
            {
                selectionIndicator.SetActive(false);
            }
            isSelected = false;
        }

        public override void OnDestroy()
        {
            SelectionManager.RemoveSelectable(this);
        }
    }
}

