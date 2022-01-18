using Game.Level.Events;
using Game.Player;

using System.Linq;

using UnityEngine;

namespace Game.Effects
{
    public sealed class SetBracerColor : MonoBehaviour
    {
        private const string COLOR_FIELD = "_EmissionColor";

        [SerializeField, Tooltip("Renderer whose materials will be modified.")]
        private Renderer[] renderers;

        [SerializeField, Tooltip("Gradient color used depending on health.")]
        private Gradient colorGradient;

        private Material[] materials;

        private void Awake()
        {
            materials = renderers
                .SelectMany(renderer => renderer.materials.Select(material => (material, renderer)))
                .Where(e =>
                {
                    if (e.material == null)
                    {
                        Debug.LogWarning($"{e.material} from {e.renderer.gameObject} has not material assigned.");
                        return false;
                    }

                    return true;
                })
                .Select(e => e.material)
                .ToArray();

            Color value = colorGradient.Evaluate(PlayerBody.Instance.HealthPercentage);
            foreach (Material material in materials)
                material.SetColor(COLOR_FIELD, value);

            EventManager.Subscribe<PlayerHealthChanged>(OnPlayerHealthChanged);
        }

        private void OnPlayerHealthChanged(PlayerHealthChanged @event)
        {
            if (@event.IsAlive)
            {
                Color value = colorGradient.Evaluate(@event.NewHealthPercentage);
                foreach (Material material in materials)
                    material.SetColor(COLOR_FIELD, value);
            }
            else
            {
                foreach (Material material in materials)
                    material.SetColor(COLOR_FIELD, Color.black);
            }
        }
    }
}