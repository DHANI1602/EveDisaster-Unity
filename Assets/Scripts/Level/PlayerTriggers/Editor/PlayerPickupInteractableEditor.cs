using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    [CustomEditor(typeof(PlayerPickupInteractable))]
    internal sealed class PlayerPickupInteractableEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo actionsFieldInfo = typeof(PlayerPickupInteractable).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);

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

                root.Add(new PropertyField(serializedObject.FindProperty("triggerOnce")));

                PlayerTriggerActionsList list = new PlayerTriggerActionsList();
                {
                    list.text = "Actions";
                    list.tooltip = "Actions to execute when object is interacted.";
                    list.BindProperty(serializedObject.FindProperty("actions"));
                }
                root.Add(list);
            }
            return root;
        }
    }
}