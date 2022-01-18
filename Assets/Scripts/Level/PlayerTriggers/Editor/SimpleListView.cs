
using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    internal sealed class SimpleListView : VisualElement
    {
        /* This implementation of a list solves to limitations of the Unity ListView implementation:
         * 1) Each element can have its custom and dynamic height, instead of using a fixed height.
         * 2) The list itself also has a dynamic height instead of a fixed heigth. */

        private SerializedProperty property;
        private Func<VisualElement> makeItem_;
        private Action<VisualElement, int> bindItem_;

        public Func<VisualElement> makeItem
        {
            get => makeItem_;
            set {
                if (makeItem_ == value)
                    return;
                makeItem_ = value;
                Clear();
                Refresh();
            }
        }

        public Action<VisualElement, int> bindItem
        {
            get => bindItem_;
            set
            {
                if (bindItem_ == value)
                    return;
                bindItem_ = value;
                Refresh();
            }
        }

        public SimpleListView()
        {
            style.flexGrow = 1;
        }

        public void BindProperty(SerializedProperty property)
        {
            this.property = property;
            Refresh();
        }

        public void Refresh()
        {
            if (property == null)
            {
                foreach (VisualElement node in Children())
                    node.SetEnabled(false);
                return;
            }

            int size = property.arraySize;
            int childrenCount = childCount;
            using (IEnumerator<VisualElement> children = Children().GetEnumerator())
            {
                int i = 0;
                while (children.MoveNext())
                {
                    VisualElement node = children.Current;

                    if (i >= size)
                    {
                        node.SetEnabled(false);
                        while (children.MoveNext())
                            children.Current.SetEnabled(false);
                        break;
                    }

                    node.SetEnabled(true);
                    bindItem(node, i++);
                }

                for (; i < size; i++)
                {
                    VisualElement node = makeItem();
                    Add(node);
                    bindItem(node, i);
                }
            }
        }

        public void TotalRefresh()
        {
            Clear();
            Refresh();
        }
    }
}