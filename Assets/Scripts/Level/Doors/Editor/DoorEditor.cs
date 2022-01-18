using Game.Level.Triggers;

using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Level.Doors
{
    [CustomEditor(typeof(Door))]
    internal sealed class DoorEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo actionsFieldInfo = typeof(Door).GetField("unlockActions", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly string[] exclude = new string[] { "Configuration", "Sounds" };

        public override VisualElement CreateInspectorGUI()
        {
            if (actionsFieldInfo.GetValue(target) is null)
                actionsFieldInfo.SetValue(target, new PlayerTriggerAction[0]);

            VisualElement root = new VisualElement();
            {
                root.style.flexGrow = 1;

                ObjectField objectField = new ObjectField("Script");
                {
                    objectField.SetEnabled(false);
                    objectField.value = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
                }
                root.Add(objectField);

                foreach (FieldInfo field in typeof(Door).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if ((field == actionsFieldInfo || field.IsStatic || field.DeclaringType != typeof(Door))
                        && (field.IsDefined(typeof(SerializeField)) || field.IsDefined(typeof(SerializeReference))))
                        continue;

                    if (field.GetCustomAttribute<HeaderAttribute>() is HeaderAttribute header)
                    {
                        if (exclude.Contains(header.header))
                            goto outside;

                        Label label = new Label(header.header);
                        {
                            label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        }
                        root.Add(label);
                    }

                    outside:
                    root.Add(new PropertyField(serializedObject.FindProperty(field.Name)));
                }

                Label unlockActionsabel = new Label("Unlock Actions");
                {
                    unlockActionsabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                root.Add(unlockActionsabel);

                PlayerTriggerActionsList list = new PlayerTriggerActionsList();
                {
                    list.BindProperty(serializedObject.FindProperty("unlockActions"));
                }
                root.Add(list);
            }
            return root;
        }
    }
}