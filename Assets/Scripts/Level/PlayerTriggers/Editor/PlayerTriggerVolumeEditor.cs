using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    [CustomEditor(typeof(PlayerTriggerVolume))]
    internal sealed class PlayerTriggerVolumeEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo actionsFieldInfo = typeof(PlayerTriggerVolume).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);

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

                PropertyField triggerOnce = new PropertyField(serializedObject.FindProperty("triggerOnce"));
                root.Add(triggerOnce);

                PlayerTriggerActionsList list = new PlayerTriggerActionsList();
                {
                    list.text = "Actions";
                    list.tooltip = "Actions to execute when player get in or out of the trigger.";
                    list.BindProperty(serializedObject.FindProperty("actions"));
                }
                root.Add(list);
            }
            return root;
        }
    }
}