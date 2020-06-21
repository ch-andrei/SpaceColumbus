﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Common;
using Entities.Health;
using Entities.Materials;
using Utilities.Events;
using Utilities.Misc;

namespace Entities.Bodies
{
    public class BodyDamageEvent : DamageEvent
    {
        public Body Body { get; private set; }
        public List<BodyPartHpEvent> BodyPartHpEvents { get; private set; }

        public BodyDamageEvent(Body body)
        {
            this.Body = body;
            BodyPartHpEvents = new List<BodyPartHpEvent>();
        }

        public void AddEvent(BodyPartHpEvent bodyPartHpEvent) => BodyPartHpEvents.Add(bodyPartHpEvent);
    }

    public class Body : IDamageable, INamed, ICloneable<Body>
    {
        public static readonly int MaxBodyPartCount = 32;

        public EBodyType BodyType { get; private set; }

        public string Name { get; private set; }

        public Body Clone() => new Body(this);

        // get a slice only over the used nodes
        public IEnumerable<BodyPart> BodyParts => new SliceEnumerator<BodyPart>(_bodyParts, 0, this.BodyPartCount);
        public IEnumerable<BodyNode> BodyNodes => new SliceEnumerator<BodyNode>(_bodyNodes, 0, this.BodyPartCount);

        public IEnumerable<BodyPart> BodyPartsChildrenOf(int index) =>
            new IndexedEnumerator<BodyPart>(_bodyParts, _bodyNodes[index].Children.ToArray());

        public IEnumerable<BodyNode> BodyNodesChildrenOf(int index) =>
            new IndexedEnumerator<BodyNode>(_bodyNodes, _bodyNodes[index].Children.ToArray());

        public int BodyPartCount { get; private set; }

        private BodyNode _rootNode;
        private BodyPart[] _bodyParts;
        private BodyNode[] _bodyNodes;

        public bool CanBeDamaged
        {
            get
            {
                foreach (var bodyPart in BodyParts)
                    if (bodyPart.CanBeDamaged)
                        return true;
                return false;
            }
        }

        public bool IsDamaged
        {
            get
            {
                foreach (var bodyPart in BodyParts)
                    if (bodyPart.IsDamaged)
                        return true;
                return false;
            }
        }

        public List<float> GetBodyPartSizes(IEnumerable<BodyPart> bodyParts)
        {
            var sizes = new List<float>();
            foreach (var bodyPart in bodyParts)
                sizes.Add(bodyPart.Size);
            return sizes;
        }

        public Body(EBodyType bodyType)
        {
            this.BodyPartCount = 0;
            this.BodyType = bodyType;
            this.Name = BodyTypes.BodyType2String(BodyType);
            this._bodyParts = new BodyPart[MaxBodyPartCount];
            this._bodyNodes = new BodyNode[MaxBodyPartCount];
            this._rootNode = new BodyNode(-1);
        }

        public Body(Body body) : this(body.BodyType)
        {
            this.BodyPartCount = body.BodyPartCount;

            for (int i = 0; i < this.BodyPartCount; i++)
            {
                // deep copy
                this._rootNode = new BodyNode(body._rootNode);
                this._bodyNodes[i] = new BodyNode(body._bodyNodes[i]);
                this._bodyParts[i] = new BodyPart(body._bodyParts[i]);
            }
        }

        public BodyNode AddBodyPart(BodyPart bodyPart) => AddBodyPart(bodyPart, ref _rootNode);
        public BodyNode AddBodyPart(BodyPart bodyPart, ref BodyNode parent)
        {
            if (BodyPartCount >= MaxBodyPartCount)
                return BodyNode.NullNode;

            var node = new BodyNode(BodyPartCount, parent.Index);
            this._bodyNodes[BodyPartCount] = node;
            this._bodyParts[BodyPartCount] = bodyPart;
            parent.AddChild(BodyPartCount);

            BodyPartCount++;
            return node;
        }

        public void AddBodyParts(List<BodyPart> bodyParts, ref BodyNode parent)
        {
            foreach (var bodyPart in bodyParts)
                AddBodyPart(bodyPart, ref parent);
        }

        public void SetNode(BodyNode node) => this._bodyNodes[node.Index] = node;

        /*
         * Distribute damage across body parts.
         * Damage Penetration and Dispersion affect how damage is applied.
         * Penetration = proportion of damage carried on the next depth of body parts (applicable for containers)
         * Dispersion = degree of sharing of the damage across the damaged components.
         * Example:
         *     Attempting to damage N objects with dispersion=0 will only apply damage to 1 of the
         *         components picked at random.
         *     Attempting to damage N objects with dispersion=1 will damage all of the components.
        */
        public DamageEvent TakeDamage(Damage damage)
        {
            // setup new body event
            var bodyEvent = new BodyDamageEvent(this);

            TakeDamage(_rootNode, damage, bodyEvent);

            return bodyEvent;
        }

        // will recursively add events to BodyDamageEvent event list (per body part)
        private void TakeDamage(BodyNode node, Damage damage, BodyDamageEvent bodyEvent)
        {
            Debug.Log("TAKE DAMAGE: index " + node.Index);

            float r1 = UnityEngine.Random.value; // unused
            float r2 = UnityEngine.Random.value;

            if (0 <= node.Index)
            {
                var bodyPart = _bodyParts[node.Index];

                // get damage event from body part hp system
                var dmgEvent = bodyPart.TakeDamage(damage) as HpSystemEvent;
                var bodyPartHpEvent = new BodyPartHpEvent(bodyPart, dmgEvent);

                // add event to the body event list
                bodyEvent.AddEvent(bodyPartHpEvent);
            }

            // Debug.Log($"bodyPart [{node.Index}] {bodyPart.NameCustom}, children: " + node.ChildrenCount);

            // apply damage to node's children
            if (node.HasChildren)
            {
                var children = node.Children.ToArray();

                var sb = new StringBuilder();
                foreach (var i in children)
                    sb.Append(i + ", ");
                // Debug.Log("children: " + sb.ToString());

                // sample indices of children to pick which children will be applied damage to
                Vector2Int indices = Samplers.SampleFromPdf(
                    r2,
                    GetBodyPartSizes(new IndexedEnumerator<BodyPart>(this._bodyParts, children)),
                    damage.Dispersion);

                // get the children
                var damagedNodes = new IndexedEnumerator<BodyNode>(
                    this._bodyNodes,
                    new SliceEnumerator<int>(children, indices[0], indices[1] + 1).ToArray()
                );

                // Debug.Log("damaged nodes count " + damagedNodes.Count()); damagedNodes.Reset(); // need to reset since its an iterator

                // compute the damage amount passed on to children of current body part
                var damageAfterMultiplier = damage * damage.Penetration;

                foreach (var damagedNode in damagedNodes)
                    TakeDamage(damagedNode, damageAfterMultiplier, bodyEvent);
            }
        }

        public EDamageState GetDamageState()
        {
            var dmgState = EDamageState.None;
            foreach (var bodyPart in BodyParts)
                dmgState = HpSystemDamageStates.GetWorstDamageState(dmgState, bodyPart.GetDamageState());
            return dmgState;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(this.Name + "\n");
            foreach (var bp in this.BodyParts)
                sb.Append(bp.Name + ": " + bp.NameCustom + " " + bp.HpSystem.AsText + "\n");

            return sb.ToString();
        }
    }
}
