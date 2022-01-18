using UnityEngine;

namespace Game.Effects
{
    public sealed class InstructionPanelSetter : MonoBehaviour
    {
        [SerializeField]
        private Renderer panelShader;

        [SerializeField]
        private Transform center;

        private Material material;

        private void Awake()
        {
            if (panelShader == null)
                Debug.LogWarning($"{nameof(panelShader)} is null.");
            else
            {
                material = panelShader.material;
                if (material == null)
                    Debug.LogWarning($"{nameof(panelShader)} has a null material.");
            }

            if (center == null)
                Debug.LogWarning($"{nameof(center)} is null.");
        }

        private void Update()
        {
            if (material != null && center != null)
                material.SetVector("_PanelPosition", transform.position);
        }
    }
}