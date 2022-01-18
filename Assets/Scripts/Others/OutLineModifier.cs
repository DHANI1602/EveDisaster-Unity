using Game.Level;

using UnityEngine;

namespace Game.Effects
{
    public sealed class OutLineModifier : MonoBehaviour, IInteractableFeedback
    {
        [Header("Selected")]
        [SerializeField, Tooltip("The colors that outline will swap between while selected")]
        private Color colorAselected;

        [SerializeField, Tooltip("The colors that outline will swap between while selected")]
        private Color colorBselected;

        [SerializeField, Tooltip("The frecuency between each color variation")]
        private float changeFrecuencySelected;

        [SerializeField, Range(0, 1), Tooltip("The saturation range that each color can reach")]
        private float changeSizeSelected;

        [SerializeField, Tooltip("The range of the outline while selected")]
        private float outlineRangeSelected;

        [Header("Unselected")]
        [SerializeField, Tooltip("The colors that outline will swap between while unselected")]
        private Color colorAunselected;

        [SerializeField, Tooltip("The colors that outline will swap between while unselected")]
        private Color colorBunselected;

        [SerializeField, Tooltip("The frecuency between each color variation while selected")]
        private float changeFrecuencyUnselected;

        [SerializeField, Range(0, 1), Tooltip("The saturation range that each color can reach while selected")]
        private float changeSizeUnselected;

        [SerializeField, Tooltip("The range of the outline while unselected")]
        private float outlineRangeUnselected;

        private Outline[] outlines;

        private bool isInSight;
        private bool isHightlight;

        private void Awake() => outlines = GetComponentsInChildren<Outline>();

        private void Update()
        {
            bool isEnabled = isInSight || isHightlight;
            Color color;
            float width;
            if (isHightlight)
            {
                width = outlineRangeSelected;
                color = Color.Lerp(colorAselected, colorBselected, (Mathf.Sin(Time.time * changeFrecuencySelected) + 1) / 2) * changeSizeSelected;
            }
            else
            {
                width = outlineRangeUnselected;
                color = Color.Lerp(colorAunselected, colorBunselected, (Mathf.Sin(Time.time * changeFrecuencyUnselected) + 1) / 2) * changeSizeUnselected;
            }

            foreach (Outline outline in outlines)
            {
                outline.enabled = isEnabled;
                outline.OutlineWidth = width;
                outline.OutlineColor = color;
            }
        }

        private void OnDisable()
        {
            foreach (Outline outline in outlines)
                outline.enabled = false;
        }

        private void OnDestroy()
        {
            foreach (Outline outline in outlines)
                Destroy(outline);
        }

        public void OutOfSight() => isInSight = false;

        public void InSight() => isInSight = true;

        public void Unhighlight() => isHightlight = false;

        public void Highlight() => isHightlight = true;
    }
}