using Game.Level.Triggers;

using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game
{
    [CustomEditor(typeof(DebugAction))]
    public sealed class DebugActionEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo actionsFieldInfo = typeof(DebugAction).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);

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

                root.Add(new PropertyField(serializedObject.FindProperty("key")));

                SerializedProperty teleportSerializedProperty = serializedObject.FindProperty("teleport");
                Toggle teleport = new Toggle(teleportSerializedProperty.displayName);
                {
                    teleport.BindProperty(teleportSerializedProperty);
                }
                root.Add(teleport);

                PropertyField newLocation = new PropertyField(serializedObject.FindProperty("newLocation"));
                {
                    newLocation.visible = teleport.value;
                }
                root.Add(newLocation);

                teleport.RegisterCallback<ChangeEvent<bool>>(e => newLocation.visible = e.newValue);

                PlayerTriggerActionsList list = new PlayerTriggerActionsList();
                {
                    list.BindProperty(serializedObject.FindProperty("actions"));
                }
                root.Add(list);
            }
            return root;
        }
    }
}