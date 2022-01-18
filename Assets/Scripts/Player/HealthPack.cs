using Enderlook.Unity.AudioManager;

using Game.Effects;
using Game.Player;
using Game.Utility;

using System.Collections;

using UnityEngine;

namespace Game.Level
{
    [RequireComponent(typeof(OutLineModifier), typeof(Animator))]
    public sealed class HealthPack : MonoBehaviour, IPickup, IInteractable
    {
        [SerializeField, Tooltip("Animation trigger name when pack is picked up.")]
        private string pickupTriggerName;

        [SerializeField, Tooltip("Sound played when pack is picked up.")]
        private AudioFile pickupSound;

        [SerializeField, Tooltip("Particles spawned when pack is picked up")]
        private GameObject pickupParticles;

        [SerializeField, Tooltip("Spawn point for particles and sounds.")]
        private Transform spawnPoint;

        [SerializeField, Min(0), Tooltip("Amount of hitpoint healed on pick up.")]
        private float healingAmount;

        [SerializeField, Min(0), Tooltip("Health recover per second on pick up. If 0, health is restored immediately.")]
        private float healingPerSecond;

        [SerializeField, Tooltip("Animation trigger name when player is full of health and try to pick up.")]
        private string fullHealthTriggerName;

        [SerializeField, Tooltip("Sound player when player is full of health and try to pick up.")]
        private AudioFile fullHealthSound;

        private OutLineModifier outline;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            outline = GetComponent<OutLineModifier>();
            if (outline == null)
                outline = null;
        }

        public void Pickup()
        {
            Transform point = spawnPoint == null ? transform : spawnPoint;

            if (PlayerBody.Instance.HealthPercentage == 1)
            {
                Try.SetAnimationTrigger(animator, fullHealthTriggerName, nameof(fullHealthTriggerName));
                Try.PlayOneShoot(point, fullHealthSound, nameof(fullHealthSound));
                HUD.FullHealthEffect();
                return;
            }

            if (!(outline is null))
            {
                if (!outline.enabled)
                    return;
                outline.enabled = false;
            }

            Try.SetAnimationTrigger(animator, pickupTriggerName, nameof(pickupTriggerName));
            Try.PlayOneShoot(point.position, pickupSound, nameof(pickupSound));
            if (pickupParticles != null)
                ParticleSystemPool.GetOrInstantiate(pickupParticles, point.position, point.rotation);

            if (healingPerSecond == 0)
            {
                PlayerBody.Instance.TakeHealing(healingAmount);
                Destroy(this);
            }
            else
                StartCoroutine(Work());

            IEnumerator Work()
            {
                while (true)
                {
                    float amount = healingPerSecond * Time.deltaTime;
                    if (amount >= healingAmount)
                    {
                        PlayerBody.Instance.TakeHealing(healingAmount);
                        break;
                    }
                    else
                    {
                        PlayerBody.Instance.TakeHealing(amount);
                        healingAmount -= amount;
                    }
                    yield return null;
                }
                Destroy(this);
            }
        }

        void IInteractable.Interact() => Pickup();
    }
}