using System;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    [CustomEditor(typeof(CountersManager))]
    internal sealed class CountersManagerEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo variablesFieldInfo = typeof(CountersManager).GetField("variables", BindingFlags.NonPublic | BindingFlags.Instance);

        public override VisualElement CreateInspectorGUI()
        {
            if (variablesFieldInfo.GetValue(target) is null)
                variablesFieldInfo.SetValue(target, Array.CreateInstance(variablesFieldInfo.FieldType.GetElementType(), 0));

            VisualElement root = new VisualElement();
            {
                root.style.flexGrow = 1;

                ObjectField objectField = new ObjectField("Script");
                {
                    objectField.SetEnabled(false);
                    objectField.value = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
                }
                root.Add(objectField);

                ListPanel variablesPanel = new ListPanel();
                {
                    variablesPanel.text = "Variables";
                    variablesPanel.tooltip = "Variables to track.";
                    variablesPanel.style.flexGrow = 1;
                    SerializedProperty variables = serializedObject.FindProperty("variables");
                    variablesPanel.BindProperty(variables);
                    variablesPanel.makeItem = () =>
                    {
                        Foldout foldout = new Foldout();
                        {
                            foldout.name = "Foldout";
                            foldout.style.flexGrow = 1;

                            TextField name = new TextField("Name");
                            {
                                name.name = "Name";
                                name.tooltip = "Name of the variable";
                            }
                            foldout.Add(name);

                            FloatField value = new FloatField("Value");
                            {
                                value.name = "Value";
                                value.tooltip = "Initial value of the variable.";
                            }
                            foldout.Add(value);

                            ListPanel conditionsPanel = new ListPanel();
                            {
                                conditionsPanel.text = "Conditions";
                                conditionsPanel.tooltip = "Conditions to check for.";
                                conditionsPanel.name = "Conditions";
                                conditionsPanel.style.flexGrow = 1;
                                conditionsPanel.makeItem = () =>
                                {
                                    VisualElement root_ = new VisualElement();
                                    {
                                        EnumField comparand = new EnumField("Comparand");
                                        {
                                            comparand.tooltip = "Comparison mode between variable value and compared value.";
                                            comparand.name = "Comparand";
                                        }
                                        root_.Add(comparand);

                                        FloatField comparedValue = new FloatField("Compared Value");
                                        {
                                            comparedValue.tooltip = "Value to compare with the variable value.";
                                            comparedValue.name = "ComparedValue";
                                        }
                                        root_.Add(comparedValue);

                                        PlayerTriggerActionsList actions = new PlayerTriggerActionsList();
                                        {
                                            actions.text = "Actions";
                                            actions.tooltip = "Actions to execute if value is comparison is expected.";
                                            actions.name = "Actions";
                                        }
                                        root_.Add(actions);
                                    }
                                    return root_;
                                };
                            }
                            foldout.Add(conditionsPanel);
                        }
                        return foldout;
                    };
                    variablesPanel.bindItem = (e, i) =>
                    {
                        Foldout foldout = e.Q<Foldout>("Foldout");
                        foldout.value = false;

                        SerializedProperty variableProperty = variables.GetArrayElementAtIndex(i);
                        SerializedProperty nameProperty = variableProperty.FindPropertyRelative("name");

                        TextField name = e.Q<TextField>("Name");
                        name.UnregisterValueChangedCallback((EventCallback<ChangeEvent<string>>)name.userData);
                        EventCallback<ChangeEvent<string>> callback = e => foldout.text = string.IsNullOrEmpty(e.newValue) ? $"Element {i}" : e.newValue;
                        name.RegisterValueChangedCallback(callback);
                        name.userData = callback;
                        name.Unbind();
                        name.BindProperty(nameProperty);

                        FloatField value = e.Q<FloatField>("Value");
                        value.Unbind();
                        value.BindProperty(variableProperty.FindPropertyRelative("value"));

                        SerializedProperty conditionsProperty = variableProperty.FindPropertyRelative("conditions");
                        ListPanel conditions = e.Q<ListPanel>("Conditions");
                        conditions.bindItem = (e, i) =>
                        {
                            SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);

                            EnumField comparand = e.Q<EnumField>("Comparand");
                            comparand.Unbind();
                            comparand.BindProperty(conditionProperty.FindPropertyRelative("comparand"));

                            FloatField comparedValue = e.Q<FloatField>("ComparedValue");
                            comparedValue.Unbind();
                            comparedValue.BindProperty(conditionProperty.FindPropertyRelative("comparedValue"));

                            PlayerTriggerActionsList actions = e.Q<PlayerTriggerActionsList>("Actions");
                            actions.Unbind();
                            actions.BindProperty(conditionProperty.FindPropertyRelative("actions"));
                        };
                        conditions.Unbind();
                        conditions.BindProperty(conditionsProperty);
                    };
                }
                root.Add(variablesPanel);
            }
            return root;
        }
    }
}