using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    internal sealed class ListPanel : VisualElement
    {
        private const string LIST_NAME = "List";
        private const string FOLDOUT_NAME = "Foldout";
        private const string SIZE_NAME = "Size";
        private const string ADD_PANNEL_NAME = "AddPanel";
        private const string ADD_BUTTON_NAME = "AddButton";
        private const string REMOVE_BUTTON_NAME = "Remove";
        private const string LIST_ELEMENT_NAME = "Element";
        private const string LIST_ELEMENT_PROPERTY_NAME = "Property";
        private const string LIST_ELEMENT_SLOT_NAME = "Slot";
        private static readonly object DUMMY = new object();

        private SerializedProperty property;

        private SimpleListView List => this.Q<SimpleListView>(LIST_NAME);
        private Foldout Foldout => this.Q<Foldout>(FOLDOUT_NAME);
        private IntegerField Size => this.Q<IntegerField>(SIZE_NAME);
        private VisualElement AddPanel => this.Q<VisualElement>(ADD_PANNEL_NAME);
        private Button AddButton => this.Q<Button>(ADD_BUTTON_NAME);

        private bool HasAddPanel => AddPanel.childCount == 2;

        private Func<VisualElement> makeItem_;
        public Func<VisualElement> makeItem {
            get => makeItem_;
            set
            {
                if (makeItem_ == value)
                    return;
                makeItem_ = value;
                List.TotalRefresh();
            }
        }

        private Action<VisualElement, int> bindItem_;
        public Action<VisualElement, int> bindItem {
            get => bindItem_;
            set
            {
                if (bindItem_ == value)
                    return;
                bindItem_ = value;
                List.TotalRefresh();
            }
        }

        public VisualElement addPanel {
            get {
                VisualElement panel = AddPanel;
                if (panel.childCount == 2)
                    return panel.Children().First();
                return null;
            }
            set {
                VisualElement panel = AddPanel;
                VisualElement button;
                if (panel.childCount == 2)
                {
                    button = panel.Children().ElementAt(1);
                    panel.RemoveAt(1);
                    panel.RemoveAt(0);
                }
                else
                    button = panel.Children().First();
                panel.Add(value);
                panel.Add(button);
            }
        }

        public string text {
            get => Foldout.text;
            set => Foldout.text = value;
        }

        public ListPanel()
        {
            Foldout foldout = new Foldout();
            {
                foldout.name = FOLDOUT_NAME;
                foldout.value = false;

                IntegerField size;
                Button addButton;
                VisualElement div = new VisualElement();
                {
                    div.style.flexDirection = FlexDirection.Row;

                    size = new IntegerField(SIZE_NAME);
                    {
                        size.name = SIZE_NAME;
                        size.style.flexGrow = 1;
                    }
                    div.Add(size);

                    addButton = new Button();
                    {
                        addButton.name = ADD_BUTTON_NAME;
                        addButton.text = "+";
                        addButton.tooltip = "Add new element to collection.";
                        addButton.SetEnabled(false);
                    }
                    div.Add(addButton);
                }
                foldout.Add(div);

                Button closeButton;
                VisualElement addPanel = new VisualElement();
                {
                    addPanel.name = ADD_PANNEL_NAME;
                    addPanel.style.flexGrow = 1;
                    addPanel.style.display = DisplayStyle.None;

                    closeButton = new Button();
                    {
                        closeButton.text = "Stop";
                        closeButton.tooltip = "Stop adding a new action.";
                    }
                    addPanel.Add(closeButton);
                }
                foldout.Add(addPanel);

                SimpleListView list = new SimpleListView();
                {
                    list.name = LIST_NAME;
                    list.style.flexGrow = 1;
                    list.makeItem += () =>
                    {
                        VisualElement root = new VisualElement();
                        {
                            root.name = LIST_ELEMENT_NAME;
                            root.style.flexDirection = FlexDirection.Row;

                            if (makeItem is null)
                            {
                                PropertyField propertyField = new PropertyField();
                                {
                                    propertyField.name = LIST_ELEMENT_PROPERTY_NAME;
                                    propertyField.style.flexGrow = 1;
                                }
                                root.Add(propertyField);
                            }
                            else
                            {
                                VisualElement slot = new VisualElement();
                                {
                                    slot.name = LIST_ELEMENT_SLOT_NAME;
                                    slot.style.flexGrow = 1;
                                    slot.Add(makeItem());
                                }
                                root.Add(slot);
                            }

                            Button removeButton = new Button();
                            {
                                removeButton.name = REMOVE_BUTTON_NAME;
                                removeButton.text = "X";
                                removeButton.tooltip = "Remove this action.";
                            }
                            root.Add(removeButton);
                        }
                        return root;
                    };
                    list.bindItem += (e, i) =>
                    {
                        if (i >= property.arraySize)
                        {
                            e.Q<VisualElement>(LIST_ELEMENT_NAME).style.display = DisplayStyle.None;
                            return;
                        }

                        e.Q<VisualElement>(LIST_ELEMENT_NAME).style.display = DisplayStyle.Flex;

                        SerializedProperty element = property.GetArrayElementAtIndex(i);

                        Button button = e.Q<Button>(REMOVE_BUTTON_NAME);
                        if (button.userData is Action callback)
                            button.clicked -= callback;
                        callback = () => Remove(i, true);
                        button.userData = callback;
                        button.clicked += callback;

                        VisualElement node;
                        if (makeItem is null || bindItem is null)
                        {
                            PropertyField propertyField = e.Q<PropertyField>(LIST_ELEMENT_PROPERTY_NAME);
                            if (!(propertyField is null)) // TODO: Why this can be null?
                            {
                                propertyField.Unbind();
                                propertyField.BindProperty(element);
                                node = propertyField;
                            }
                            else
                                node = null;
                        }
                        else
                            bindItem(node = e.Q<VisualElement>(LIST_ELEMENT_SLOT_NAME), i);

                        if (node is null)
                            e.style.height = 0; // TODO: Is this fine?
                        else
                            e.style.height = node.style.height;
                    };
                }

                addButton.clicked += OnAdd;
                closeButton.clicked += CloseAddPanel;
                size.RegisterValueChangedCallback(OnSizeChange);

                foldout.Add(list);
            }
            hierarchy.Add(foldout);
        }

        private void OnAdd()
        {
            VisualElement addPanel = AddPanel;
            if (addPanel.childCount == 1)
                Add(null, DUMMY);
            else
            {
                addPanel.style.display = DisplayStyle.Flex;
                Size.style.display = DisplayStyle.None;
                AddButton.style.display = DisplayStyle.None;
                List.style.display = DisplayStyle.None;
            }
        }

        private void CloseAddPanel()
        {
            AddPanel.style.display = DisplayStyle.None;
            Size.style.display = DisplayStyle.Flex;
            AddButton.style.display = DisplayStyle.Flex;
            List.style.display = DisplayStyle.Flex;
        }

        private void OnSizeChange(ChangeEvent<int> e)
        {
            if (e.previousValue < e.newValue)
            {
                if (HasAddPanel)
                    Size.SetValueWithoutNotify(e.previousValue);
                else
                    Add(e.newValue, null);
            }
            else if (e.previousValue > e.newValue)
                Remove(e.newValue, false);
        }

        private void Remove(int value, bool removeSpecificIndex)
        {
            // `tuple.property.DeleteArrayElementAtIndex(...)` doesn't work.
            // So as a work-around we use the following code:

            IList content = property.GetValue<IList>();
            if (content is Array array)
            {
                Array newArray;
                if (removeSpecificIndex)
                {
                    newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length - 1);
                    if (value > 0)
                        Array.Copy(array, 0, newArray, 0, value);
                    if (value < array.Length - 1)
                        Array.Copy(array, value + 1, newArray, value, array.Length - value - 1);
                }
                else
                {
                    Debug.Assert(content.Count > value);
                    newArray = Array.CreateInstance(array.GetType().GetElementType(), value);
                    Array.Copy(array, newArray, value);
                }
                content = newArray;
                property.SetValue(content);
            }
            else
            {
                if (removeSpecificIndex)
                    content.RemoveAt(value);
                else
                {
                    Debug.Assert(content.Count > value);
                    for (int i = content.Count; i > value; i--)
                        content.RemoveAt(i - 1);
                }
            }

            Save(content);
        }

        private void Add(int? target, object value)
        {
            // `property.InsertArrayElementAtIndex(...)` nor `property.arraySize += ...` works.
            // So as a work-around we use the following code:

            IList content = property.GetValue<IList>();
            if (content is null)
            {
                Type listType = property.GetPropertyType();
                if (typeof(Array).IsAssignableFrom(listType))
                {
                    Array array = Array.CreateInstance(listType.GetElementType(), target ?? 1);
                    if (value != DUMMY)
                    {
                        Debug.Assert(!target.HasValue);
                        array.SetValue(value, 0);
                    }
                    property.SetValue(content = array);
                }
                else
                {
                    IList list = (IList)Activator.CreateInstance(listType);
                    if (value != DUMMY)
                    {
                        Debug.Assert(!target.HasValue);
                        list[0] = value;
                    }
                    property.SetValue(content = list);
                }
            }
            else if (content is Array array)
            {
                int oldLength = array.Length;
                Debug.Assert(!(target is int t) || t > oldLength);
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), target is int t_ ? t_ : oldLength + 1);
                Array.Copy(array, newArray, oldLength);
                content = newArray;
                if (value != DUMMY)
                {
                    Debug.Assert(!target.HasValue);
                    newArray.SetValue(value, oldLength);
                }
                property.SetValue(content);
            }
            else
            {
                if (target is int t)
                {
                    Debug.Assert(value == DUMMY);
                    Debug.Assert(t > content.Count);
                    for (int i = content.Count; i < t; i++)
                        content.Add(default);
                }
                else
                    content.Add(value == DUMMY ? default : value);
            }

            Save(content);
        }

        private void Save(IList content)
        {
            SerializedObject serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            serializedObject.UpdateIfRequiredOrScript();
            Size.SetValueWithoutNotify(content.Count);
            List.Refresh();
        }

        public void BindProperty(SerializedProperty obj)
        {
            property = obj;

            this.Q<Button>(ADD_BUTTON_NAME).SetEnabled(true);

            SimpleListView list = List;
            list.BindProperty(obj);

            Refresh();
        }

        public void Refresh()
        {
            List.Refresh();
            Size.SetValueWithoutNotify(property.arraySize);
        }

        public void Add(object value)
        {
            if (AddPanel.style.display == DisplayStyle.None)
                throw new InvalidOperationException("Can't add element if addition was not in progress.");
            Add(null, value);
            CloseAddPanel();
        }
    }
}