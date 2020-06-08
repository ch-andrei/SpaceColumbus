using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Common;
using Entities.Health;

using Utilities.Events;
using Utilities.Misc;

namespace Entities.Bodies
{
    public class BodyEventSystem : EventGenerator<DamageableEvent>, IEventListener<HpSystemEvent>
    {
        public bool OnEvent(HpSystemEvent gameEvent)
        {
            Notify(gameEvent);

            return true;
        }
    }

    public struct BodyNode
    {
        public const int MaxChildrenCount = 16;

        public const int NullIndex = -1;
        public static BodyNode NullNode => new BodyNode(NullIndex);

        public int Index; // this nodes index
        public int Parent;

        public bool HasParent => Parent != NullIndex;
        public bool HasChildren => 0 < childrenCount;

        public IEnumerable<int> Children => new SliceEnumerator<int>(_children, 0, this.childrenCount);
        public List<int> ChildrenList => Children.ToList();

        public int childrenCount { get; private set; }

        private int[] _children;

        public BodyNode(int index, int parent=NullIndex)
        {
            this._children = new int[MaxChildrenCount];
            this.childrenCount = 0;
            this.Index = index;
            this.Parent = parent;
        }

        // deep copy constructor
        public BodyNode(BodyNode node) : this(node.Index, node.Parent)
        {
            this.childrenCount = node.childrenCount;
            for (int i = 0; i < this.childrenCount; i++)
                this._children[i] = node.ChildrenList[i];
        }

        public bool AddChild(int child)
        {
            if (childrenCount >= MaxChildrenCount)
                return false;

            _children[childrenCount++] = child;

            return true;
        }

    }

    public struct BodyPart : INamed, IDamageable, IWithListeners<HpSystemEvent>, ICloneable<BodyPart>
    {
        public HpSystem HpSystem { get; private set; }

        public string Name { get; set; }
        public string NameCustom { get; set; }

        public float Size { get; set; }

        // IDamageable functions
        public bool IsDamageable => true;
        public bool IsDamaged => this.HpSystem.IsDamaged;

        public BodyPart(HpSystem hpSystem, string name, float size=0)
        {
            this.HpSystem = hpSystem;
            this.Name = name;
            this.NameCustom = name;
            this.Size = size;
        }

        public BodyPart(BodyPart bodyPart) : this (
            new HpSystem(bodyPart.HpSystem),
            string.Copy(bodyPart.Name),
            bodyPart.Size
        )
        {
            this.NameCustom = string.Copy(bodyPart.NameCustom);
        }

        public EDamageState GetDamageState() => this.HpSystem.GetDamageState();

        public void TakeDamage(Damage damage) => HpSystem.TakeDamage(damage);

        public BodyPart Clone() => new BodyPart(this);

        public List<IEventListener<HpSystemEvent>> EventListeners => HpSystem.EventListeners;

        public void AddListener(IEventListener<HpSystemEvent> eventListener)
        {
            this.HpSystem.AddListener(eventListener);
        }

        public string GetHealthInfo()
        {
            if (this.IsDamaged)
                return this.HpSystem.AsText;
            else
                return "";
        }

        public string ToString()
        {
            return $"BODYPART {this.NameCustom}";
        }
    }

    public class Body : IDamageable, INamed, ICloneable<Body>,
        IWithListeners<DamageableEvent>, IEventListener<HpSystemEvent>
    {
        // NOTE: this value should be less than 255, see _count
        public static readonly int MaxBodyPartCount = 64;

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

        private BodyEventSystem _eventSystem;
        public List<IEventListener<DamageableEvent>> EventListeners => _eventSystem.EventListeners;

        public bool IsDamageable
        {
            get
            {
                foreach (var bodyPart in BodyParts)
                    if (bodyPart.IsDamageable)
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
            List<float> sizes = new List<float>();
            foreach (var bodyPart in bodyParts)
                sizes.Add(bodyPart.Size);
            return sizes;
        }

        public Body(EBodyType bodyType)
        {
            this.BodyPartCount = 0;
            this.BodyType = bodyType;
            this.Name = BodyTypes.BodyType(BodyType);
            this._bodyParts = new BodyPart[MaxBodyPartCount];
            this._bodyNodes = new BodyNode[MaxBodyPartCount];
            this._rootNode = new BodyNode(0);
            this._eventSystem = new BodyEventSystem();
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

        public BodyNode AddBodyPart(BodyPart bodyPart) => AddBodyPart(bodyPart, _rootNode);

        public BodyNode AddBodyPart(BodyPart bodyPart, BodyNode parent)
        {
            if (BodyPartCount >= MaxBodyPartCount)
                return BodyNode.NullNode;

            var node = new BodyNode(BodyPartCount, parent.Index);
            this._bodyNodes[BodyPartCount] = node;
            this._bodyParts[BodyPartCount] = bodyPart;
            parent.AddChild(BodyPartCount);

            // listen on body parts HpSystem
            bodyPart.AddListener(this);

            BodyPartCount++;
            return node;
        }

        public void AddBodyParts(List<BodyPart> bodyParts, BodyNode parent)
        {
            foreach (var bodyPart in bodyParts)
                AddBodyPart(bodyPart, parent);
        }

        public void TakeDamage(Damage damage) => TakeDamage(_rootNode, damage);

        private void TakeDamage(BodyNode node, Damage damage)
        {
            // apply damage to current node
            var bodyPart = _bodyParts[node.Index];
            bodyPart.TakeDamage(damage);

            // apply damage to node's children
            if (node.HasChildren)
            {
                var children = node.ChildrenList;

                // sample indices of children to pick which children will be applied damage to
                Vector2Int indices = Samplers.SampleFromPdf(
                    UnityEngine.Random.value,
                    GetBodyPartSizes(new IndexedEnumerator<BodyPart>(this._bodyParts, children)),
                    damage.Dispersion);

                // get the children
                var damagedNodes = new IndexedEnumerator<BodyNode>(this._bodyNodes,
                    new SliceEnumerator<int>(children.ToArray(), indices[0], indices[1]).ToList()
                );

                // compute the damage amount passed on to children of current body part

                // get a multiplier that applies to all damage types
                var damageMultiplier = DamageMultipliers.Multiplier(damage.Penetration);
                // apply multiplier to incoming damage
                Damage damageAfterMultiplier =
                    DamageMultipliers.GetDamageAfterMultiplier(damage, damageMultiplier);

                foreach (var damagedNode in damagedNodes)
                    TakeDamage(damagedNode, damageAfterMultiplier);
            }
        }

        public EDamageState GetDamageState()
        {
            EDamageState worstDamageState = EDamageState.None;
            foreach (var bodyPart in BodyParts)
                worstDamageState = HpSystemDamageStates.GetWorstDamageState(worstDamageState, bodyPart.GetDamageState());
            return worstDamageState;
        }

        public void AddListener(IEventListener<DamageableEvent> eventListener)
        {
            this._eventSystem.AddListener(eventListener);
        }

        public bool OnEvent(HpSystemEvent gameEvent)
        {
            _eventSystem.OnEvent(gameEvent);

            return true;
        }

    }

    // public class Body
    // {
    //     public static Body HumanoidBody => BodyPartFactory.HumanoidBody;
    //
    //     public Body(string name) : base(name) { }
    //
    //     public Body(Body body) : base(body as BodyPartContainer) { }
    //
    //     public override BodyPart Clone()
    //     {
    //         return new Body(this);
    //     }
    // }
}
