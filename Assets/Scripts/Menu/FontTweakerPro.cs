using System.Reflection;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    [RequireComponent(typeof(Button))]
    public sealed class FontTweakerPro : MonoBehaviour
    {
        [SerializeField, Tooltip("Font used when button is normal.")]
        private TMP_FontAsset normalFont;

        [SerializeField, Tooltip("Font used when button is highlighted.")]
        private TMP_FontAsset highlightedFont;

        [SerializeField, Tooltip("Font used when button is pressed.")]
        private TMP_FontAsset pressedFont;

        [SerializeField, Tooltip("Font used when button is selected.")]
        private TMP_FontAsset selectedFont;

        [SerializeField, Tooltip("Font used when button is disabled.")]
        private TMP_FontAsset disabledFont;

        private Button button;
        private TMP_Text text;
        private TMP_FontAsset originalFont;

        private bool currentSelectionStateInfo;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = null;
                Debug.LogWarning($"No component of type {nameof(Button)} found in gameobject.");
            }

            text = GetComponentInChildren<TMP_Text>();
            if (text == null)
            {
                text = null;
                Debug.LogWarning($"No component of type {nameof(TMP_Text)} found in gameobject not children.");
            }
            else
                originalFont = text.font;
        }

        private sealed class Helper : Selectable
        {
            private static PropertyInfo currentSelectionStatePropertyInfo = typeof(Selectable)
                .GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);

            public static void Set(FontTweakerPro tweaker)
            {
                switch (currentSelectionStatePropertyInfo.GetValue(tweaker.button))
                {
                    case SelectionState.Normal:
                        tweaker.text.font = tweaker.normalFont == null ? tweaker.originalFont : tweaker.normalFont;
                        break;
                    case SelectionState.Highlighted:
                        tweaker.text.font = tweaker.highlightedFont == null ? tweaker.originalFont : tweaker.highlightedFont;
                        break;
                    case SelectionState.Pressed:
                        tweaker.text.font = tweaker.pressedFont == null ? tweaker.originalFont : tweaker.pressedFont;
                        break;
                    case SelectionState.Selected:
                        tweaker.text.font = tweaker.selectedFont == null ? tweaker.originalFont : tweaker.selectedFont;
                        break;
                    case SelectionState.Disabled:
                        tweaker.text.font = tweaker.disabledFont == null ? tweaker.originalFont : tweaker.disabledFont;
                        break;
                }
            }
        }

        private void Update()
        {
            if (button is null || text is null)
                return;

            Helper.Set(this);
        }
    }
}