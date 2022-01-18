using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Game.Effects
{
    public class RumbleShaderController : MonoBehaviour
    {
        [SerializeField, Tooltip("The center position where rumble will reach maximun apperture.")]
        private Transform boilerRoomPosition;

        [SerializeField, Tooltip("The range that boiler room rumble reaches.")]
        private float range;

        [SerializeField, Tooltip("How fast explosion rumble will grow down.")]
        private float growDownExplosionSpeed;

        [Tooltip("The max rumble apperture.")]
        public float apperture;

        private FloatParameter rumbleApertureNatural;

        private void Awake()
        {
            PostProcessVolume postProcessVolume = GetComponentInChildren<PostProcessVolume>();
            if (postProcessVolume == null)
                Debug.LogWarning($"Component of type {nameof(PostProcessVolume)} was not found in children.");
            else if (!postProcessVolume.profile.TryGetSettings(out RumblePPSSettings hurtShader))
                Debug.LogWarning($"Settings for {nameof(RumblePPSSettings)} was not found in {nameof(PostProcessVolume)}.");
            else
                rumbleApertureNatural = hurtShader._Rumble_Aprerture_Natural;
        }

        private void Update()
        {
            ResetExplosionRumbleApperture();

            rumbleApertureNatural.value = apperture * Mathf.Clamp(1 - (Vector3.Distance(boilerRoomPosition.position, transform.position) / range), 0, 1);
        }

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(boilerRoomPosition.position, range);

        public void Explosion(float value) => rumbleApertureNatural.value += value;

        private void ResetExplosionRumbleApperture()
        {
            if (rumbleApertureNatural.value > 0)
                rumbleApertureNatural.value -= Time.deltaTime * growDownExplosionSpeed;
            else if (rumbleApertureNatural.value < 0)
                rumbleApertureNatural.value = 0;
        }
    }
}