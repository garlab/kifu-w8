using System;
using System.Collections.Generic;

namespace GoLib.Utils
{
    public class Tree<T>
    {
        private T _data;
        private Tree<T> _parent;
        private List<Tree<T>> _children;
        private int _lastIndex = 0;

        public Tree()
            : this(default(T), null)  // TODO: perhaps use a Move.Empty sentinel ?
        {
        }

        public Tree(T data, Tree<T> parent)
        {
            _data = data;
            _parent = parent;
            _children = new List<Tree<T>>(1);
        }

        public bool HasPrev
        {
            get { return _parent != null; }
        }

        public bool HasNext
        {
            get { return _children.Count > 0; }
        }

        public Tree<T> Prev
        {
            get { return _parent; }
        }

        public Tree<T> Next
        {
            get { return _children[_lastIndex]; }
        }

        public void Clear()
        {
            _children.Clear();
            _lastIndex = 0;
        }

        public void AddTree(Tree<T> tree)
        {
            tree._parent = this;
            _children.Add(tree);
        }

        public void Add(T data)
        {
            _children.Add(new Tree<T>(data, this));
            _lastIndex = _children.Count - 1;
        }

        public Tree<T> Get(T data)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                if (_children[i]._data.Equals(data))
                {
                    _lastIndex = i;
                    return _children[i];
                }
            }
            return null;
        }

        public bool Contains(T data)
        {
            return Get(data) != null;
        }

        public void Traverse(Tree<T> node, Action<T> visitor)
        {
            visitor(node._data);
            foreach (Tree<T> kid in node._children)
                Traverse(kid, visitor);
        }
    }
}
