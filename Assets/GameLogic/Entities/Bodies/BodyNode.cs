using System.Collections.Generic;

using Utilities.Misc;

namespace Entities.Bodies
{
    public struct BodyNode
    {
        public const int MaxChildrenCount = 16;

        public const int NullIndex = -1;
        public static BodyNode NullNode => new BodyNode(NullIndex);

        public int Index; // this nodes index
        public int Parent;

        public bool HasParent => Parent != NullIndex;
        public bool HasChildren => 0 < ChildrenCount;

        public int ChildrenCount { get; private set; }

        // enumerator over the used children from the children array
        public IEnumerable<int> Children => new SliceEnumerator<int>(_children, 0, this.ChildrenCount);

        // the underlying children array
        private int[] _children;

        public BodyNode(int index, int parent=NullIndex)
        {
            this._children = new int[MaxChildrenCount];
            this.ChildrenCount = 0;
            this.Index = index;
            this.Parent = parent;
        }

        // deep copy constructor
        public BodyNode(BodyNode node) : this(node.Index, node.Parent)
        {
            foreach (var i in node.Children)
                this.AddChild(i);
        }

        public bool AddChild(int child)
        {
            if (MaxChildrenCount <= ChildrenCount)
                return false;

            _children[ChildrenCount] = child;

            ChildrenCount++;

            return true;
        }
    }
}
