using System;
using System.Collections.Generic;
using System.Drawing;

using System.Diagnostics;
using UnityEngine;


/// Code taken from:
/// https://bluetoque.ca/products/quadtree/
namespace Utilities.QuadTree
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasPoint
    {
        Vector2 Point { get; }
    }

    // wrapper around unity rectangle
    public class Rectangle
    {
        private Rect _rect;

        public Vector2 Location => _rect.position;
        public Vector2 Size => _rect.size;
        public float Height => _rect.height;
        public float Width => _rect.width;

        public float Bottom => _rect.yMin;
        public float Top => _rect.yMax;
        public float Left => _rect.xMin;
        public float Right => _rect.xMax;

        public bool IsEmpty => _rect.width == 0 || _rect.height == 0;

        public Rectangle(float x, float y, float sizeX, float sizeY) { _rect = new Rect(x, y, sizeX, sizeY); }
        public Rectangle(Vector2 point, Vector2 size) : this(point.x, point.y, size.x, size.y) { }

        public bool IntersectsWith(Rectangle other) => this._rect.Overlaps(other._rect);
        public bool Contains(Vector2 point) => this._rect.Contains(point);
        public bool Contains(Rectangle other) => Contains(other.Location) && Contains(other.Location + other.Size);
    }

    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        Rectangle Rectangle { get; }
    }

    /// <summary>
    /// A Quadtree is a structure designed to partition space so
    /// that it's faster to find out what is inside or outside a given
    /// area. See http://en.wikipedia.org/wiki/Quadtree
    /// This QuadTree contains items that have an area (Rectangle)
    /// it will store a reference to the item in the quad
    /// that is just big enough to hold it. Each quad has a bucket that
    /// contain multiple items.
    /// </summary>
    public class PointQuadTree<T> where T : IHasPoint
    {
        /// <summary>
        /// The root QuadTreeNode
        /// </summary>
        PointQuadTreeNode<T> m_root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>`
        Rectangle m_rectangle;

        /// <summary>
        /// An delegate that performs an action on a QuadTreeNode
        /// </summary>
        /// <param name="obj"></param>
        public delegate void QTAction(PointQuadTreeNode<T> obj);

        /// <summary>
        ///
        /// </summary>
        /// <param name="rectangle"></param>
        public PointQuadTree(Rectangle rectangle)
        {
            m_rectangle = rectangle;
            m_root = new PointQuadTreeNode<T>(m_rectangle);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count => m_root.Count;

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            m_root.Insert(item);
        }

        /// <summary>
        /// Query the QuadTree, returning the items that are in the given area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public List<T> Query(Rectangle area)
        {
            return m_root.Query(area);
        }

        /// <summary>
        /// Do the specified action for each item in the quad-tree
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(QTAction action)
        {
            m_root.ForEach(action);
        }
    }

    /// <summary>
    /// The QuadTreeNode
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PointQuadTreeNode<T> where T : IHasPoint
    {
        /// <summary>
        /// Construct a quadtree node with the given bounds
        /// </summary>
        /// <param name="area"></param>
        public PointQuadTreeNode(Rectangle bounds)
        {
            m_bounds = bounds;
        }

        /// <summary>
        /// The area of this node
        /// </summary>
        Rectangle m_bounds;

        /// <summary>
        /// The contents of this node.
        /// Note that the contents have no limit: this is not the standard way to implement a QuadTree
        /// </summary>
        List<T> m_contents = new List<T>();

        /// <summary>
        /// The child nodes of the QuadTree
        /// </summary>
        List<PointQuadTreeNode<T>> m_nodes = new List<PointQuadTreeNode<T>>(4);

        /// <summary>
        /// Is the node empty
        /// </summary>
        public bool IsEmpty => m_bounds.IsEmpty || m_nodes.Count == 0;

        /// <summary>
        /// Area of the quadtree node
        /// </summary>
        public Rectangle Bounds => m_bounds;

        /// <summary>
        /// Total number of nodes in the this node and all SubNodes
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                foreach (PointQuadTreeNode<T> node in m_nodes)
                    count += node.Count;

                count += this.Contents.Count;

                return count;
            }
        }

        /// <summary>
        /// Return the contents of this node and all sub-nodes in the tree below this one.
        /// </summary>
        public List<T> SubTreeContents
        {
            get
            {
                List<T> results = new List<T>();

                foreach (PointQuadTreeNode<T> node in m_nodes)
                    results.AddRange(node.SubTreeContents);

                results.AddRange(this.Contents);
                return results;
            }
        }

        public List<T> Contents => m_contents;

        /// <summary>
        /// Query the QuadTree for items that are in the given area
        /// </summary>
        /// <param name="queryArea"></pasram>
        /// <returns></returns>
        public List<T> Query(Rectangle queryArea)
        {
            // create a list of the items that are found
            List<T> results = new List<T>();

            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad
            // to see if they intersect.
            foreach (T item in this.Contents)
            {
                if (queryArea.Contains(item.Point))
                    results.Add(item);
            }

            foreach (PointQuadTreeNode<T> node in m_nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Bounds.Contains(queryArea))
                {
                    results.AddRange(node.Query(queryArea));
                    break;
                }

                // Case 2: Sub-quad completely contained by search area
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children
                // to the result set. You need to continue the loop to test
                // the other quads
                if (queryArea.Contains(node.Bounds))
                {
                    results.AddRange(node.SubTreeContents);
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Bounds.IntersectsWith(queryArea))
                {
                    results.AddRange(node.Query(queryArea));
                }
            }

            return results;
        }

        /// <summary>
        /// Insert an item to this node
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            // if the item is not contained in this quad, there's a problem
            if (!m_bounds.Contains(item.Point))
            {
                Trace.TraceWarning("feature is out of the bounds of this quadtree node");
                return;
            }

            // if the subnodes are null create them. may not be sucessfull: see below
            // we may be at the smallest allowed size in which case the subnodes will not be created
            if (m_nodes.Count == 0)
                CreateSubNodes();

            // for each subnode:
            // if the node contains the item, add the item to that node and return
            // this recurses into the node that is just large enough to fit this item
            foreach (PointQuadTreeNode<T> node in m_nodes)
            {
                if (node.Bounds.Contains(item.Point))
                {
                    node.Insert(item);
                    return;
                }
            }

            // if we make it to here, either
            // 1) none of the subnodes completely contained the item. or
            // 2) we're at the smallest subnode size allowed
            // add the item to this node's contents.
            this.Contents.Add(item);
        }

        public void ForEach(PointQuadTree<T>.QTAction action)
        {
            action(this);

            // draw the child quads
            foreach (PointQuadTreeNode<T> node in this.m_nodes)
                node.ForEach(action);
        }

        /// <summary>
        /// Internal method to create the subnodes (partitions space)
        /// </summary>
        private void CreateSubNodes()
        {
            // the smallest subnode has an area
            if ((m_bounds.Height * m_bounds.Width) <= 10)
                return;

            float halfWidth = (m_bounds.Width / 2f);
            float halfHeight = (m_bounds.Height / 2f);

            m_nodes.Add(new PointQuadTreeNode<T>(new Rectangle(m_bounds.Location, new Vector2(halfWidth, halfHeight))));
            m_nodes.Add(new PointQuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left, m_bounds.Top + halfHeight), new Vector2(halfWidth, halfHeight))));
            m_nodes.Add(new PointQuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left + halfWidth, m_bounds.Top), new Vector2(halfWidth, halfHeight))));
            m_nodes.Add(new PointQuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left + halfWidth, m_bounds.Top + halfHeight), new Vector2(halfWidth, halfHeight))));
        }
    }

    // BELOW IS AN IMPLEMENTATION THAT CONSIDERS OBJECTS WITH AN AREA

    // /// <summary>
    // /// A Quadtree is a structure designed to partition space so
    // /// that it's faster to find out what is inside or outside a given
    // /// area. See http://en.wikipedia.org/wiki/Quadtree
    // /// This QuadTree contains items that have an area (Rectangle)
    // /// it will store a reference to the item in the quad
    // /// that is just big enough to hold it. Each quad has a bucket that
    // /// contain multiple items.
    // /// </summary>
    // public class QuadTree<T> where T : IHasRect
    // {
    //     /// <summary>
    //     /// The root QuadTreeNode
    //     /// </summary>
    //     QuadTreeNode<T> m_root;
    //
    //     /// <summary>
    //     /// The bounds of this QuadTree
    //     /// </summary>
    //     Rectangle m_rectangle;
    //
    //     /// <summary>
    //     /// An delegate that performs an action on a QuadTreeNode
    //     /// </summary>
    //     /// <param name="obj"></param>
    //     public delegate void QTAction(QuadTreeNode<T> obj);
    //
    //     /// <summary>
    //     ///
    //     /// </summary>
    //     /// <param name="rectangle"></param>
    //     public QuadTree(Rectangle rectangle)
    //     {
    //         m_rectangle = rectangle;
    //         m_root = new QuadTreeNode<T>(m_rectangle);
    //     }
    //
    //     /// <summary>
    //     /// Get the count of items in the QuadTree
    //     /// </summary>
    //     public int Count { get { return m_root.Count; } }
    //
    //     /// <summary>
    //     /// Insert the feature into the QuadTree
    //     /// </summary>
    //     /// <param name="item"></param>
    //     public void Insert(T item)
    //     {
    //         m_root.Insert(item);
    //     }
    //
    //     /// <summary>
    //     /// Query the QuadTree, returning the items that are in the given area
    //     /// </summary>
    //     /// <param name="area"></param>
    //     /// <returns></returns>
    //     public List<T> Query(Rectangle area)
    //     {
    //         return m_root.Query(area);
    //     }
    //
    //     /// <summary>
    //     /// Do the specified action for each item in the quadtree
    //     /// </summary>
    //     /// <param name="action"></param>
    //     public void ForEach(QTAction action)
    //     {
    //         m_root.ForEach(action);
    //     }
    // }
    //
    // /// <summary>
    // /// The QuadTreeNode
    // /// </summary>
    // /// <typeparam name="T"></typeparam>
    // public class QuadTreeNode<T> where T : IHasRect
    // {
    //     /// <summary>
    //     /// Construct a quadtree node with the given bounds
    //     /// </summary>
    //     /// <param name="area"></param>
    //     public QuadTreeNode(Rectangle bounds)
    //     {
    //         m_bounds = bounds;
    //     }
    //
    //     /// <summary>
    //     /// The area of this node
    //     /// </summary>
    //     Rectangle m_bounds;
    //
    //     /// <summary>
    //     /// The contents of this node.
    //     /// Note that the contents have no limit: this is not the standard way to impement a QuadTree
    //     /// </summary>
    //     List<T> m_contents = new List<T>();
    //
    //     /// <summary>
    //     /// The child nodes of the QuadTree
    //     /// </summary>
    //     List<QuadTreeNode<T>> m_nodes = new List<QuadTreeNode<T>>(4);
    //
    //     /// <summary>
    //     /// Is the node empty
    //     /// </summary>
    //     public bool IsEmpty { get { return m_bounds.IsEmpty || m_nodes.Count == 0; } }
    //
    //     /// <summary>
    //     /// Area of the quadtree node
    //     /// </summary>
    //     public Rectangle Bounds { get { return m_bounds; } }
    //
    //     /// <summary>
    //     /// Total number of nodes in the this node and all SubNodes
    //     /// </summary>
    //     public int Count
    //     {
    //         get
    //         {
    //             int count = 0;
    //
    //             foreach (QuadTreeNode<T> node in m_nodes)
    //                 count += node.Count;
    //
    //             count += this.Contents.Count;
    //
    //             return count;
    //         }
    //     }
    //
    //     /// <summary>
    //     /// Return the contents of this node and all subnodes in the true below this one.
    //     /// </summary>
    //     public List<T> SubTreeContents
    //     {
    //         get
    //         {
    //             List<T> results = new List<T>();
    //
    //             foreach (QuadTreeNode<T> node in m_nodes)
    //                 results.AddRange(node.SubTreeContents);
    //
    //             results.AddRange(this.Contents);
    //             return results;
    //         }
    //     }
    //
    //     public List<T> Contents { get { return m_contents; } }
    //
    //     /// <summary>
    //     /// Query the QuadTree for items that are in the given area
    //     /// </summary>
    //     /// <param name="queryArea"></pasram>
    //     /// <returns></returns>
    //     public List<T> Query(Rectangle queryArea)
    //     {
    //         // create a list of the items that are found
    //         List<T> results = new List<T>();
    //
    //         // this quad contains items that are not entirely contained by
    //         // it's four sub-quads. Iterate through the items in this quad
    //         // to see if they intersect.
    //         foreach (T item in this.Contents)
    //         {
    //             if (queryArea.IntersectsWith(item.Rectangle))
    //                 results.Add(item);
    //         }
    //
    //         foreach (QuadTreeNode<T> node in m_nodes)
    //         {
    //             if (node.IsEmpty)
    //                 continue;
    //
    //             // Case 1: search area completely contained by sub-quad
    //             // if a node completely contains the query area, go down that branch
    //             // and skip the remaining nodes (break this loop)
    //             if (node.Bounds.Contains(queryArea))
    //             {
    //                 results.AddRange(node.Query(queryArea));
    //                 break;
    //             }
    //
    //             // Case 2: Sub-quad completely contained by search area
    //             // if the query area completely contains a sub-quad,
    //             // just add all the contents of that quad and it's children
    //             // to the result set. You need to continue the loop to test
    //             // the other quads
    //             if (queryArea.Contains(node.Bounds))
    //             {
    //                 results.AddRange(node.SubTreeContents);
    //                 continue;
    //             }
    //
    //             // Case 3: search area intersects with sub-quad
    //             // traverse into this quad, continue the loop to search other
    //             // quads
    //             if (node.Bounds.IntersectsWith(queryArea))
    //             {
    //                 results.AddRange(node.Query(queryArea));
    //             }
    //         }
    //
    //
    //         return results;
    //     }
    //
    //     /// <summary>
    //     /// Insert an item to this node
    //     /// </summary>
    //     /// <param name="item"></param>
    //     public void Insert(T item)
    //     {
    //         // if the item is not contained in this quad, there's a problem
    //         if (!m_bounds.Contains(item.Rectangle))
    //         {
    //             Trace.TraceWarning("feature is out of the bounds of this quadtree node");
    //             return;
    //         }
    //
    //         // if the subnodes are null create them. may not be sucessfull: see below
    //         // we may be at the smallest allowed size in which case the subnodes will not be created
    //         if (m_nodes.Count == 0)
    //             CreateSubNodes();
    //
    //         // for each subnode:
    //         // if the node contains the item, add the item to that node and return
    //         // this recurses into the node that is just large enough to fit this item
    //         foreach (QuadTreeNode<T> node in m_nodes)
    //         {
    //             if (node.Bounds.Contains(item.Rectangle))
    //             {
    //                 node.Insert(item);
    //                 return;
    //             }
    //         }
    //
    //         // if we make it to here, either
    //         // 1) none of the subnodes completely contained the item. or
    //         // 2) we're at the smallest subnode size allowed
    //         // add the item to this node's contents.
    //         this.Contents.Add(item);
    //     }
    //
    //     public void ForEach(QuadTree<T>.QTAction action)
    //     {
    //         action(this);
    //
    //         // draw the child quads
    //         foreach (QuadTreeNode<T> node in this.m_nodes)
    //             node.ForEach(action);
    //     }
    //
    //     /// <summary>
    //     /// Internal method to create the subnodes (partitions space)
    //     /// </summary>
    //     private void CreateSubNodes()
    //     {
    //         // the smallest subnode has an area
    //         if ((m_bounds.Height * m_bounds.Width) <= 10)
    //             return;
    //
    //         float halfWidth = (m_bounds.Width / 2f);
    //         float halfHeight = (m_bounds.Height / 2f);
    //
    //         m_nodes.Add(new QuadTreeNode<T>(new Rectangle(m_bounds.Location, new Vector2(halfWidth, halfHeight))));
    //         m_nodes.Add(new QuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left, m_bounds.Top + halfHeight), new Vector2(halfWidth, halfHeight))));
    //         m_nodes.Add(new QuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left + halfWidth, m_bounds.Top), new Vector2(halfWidth, halfHeight))));
    //         m_nodes.Add(new QuadTreeNode<T>(new Rectangle(new Vector2(m_bounds.Left + halfWidth, m_bounds.Top + halfHeight), new Vector2(halfWidth, halfHeight))));
    //     }
    // }
}
