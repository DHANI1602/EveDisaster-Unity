using Enderlook.Unity.AudioManager;
using Enderlook.Unity.Toolset.Attributes;

using Game.Utility;

using System;

using UnityEngine;

namespace Game.Player.Weapons
{
    public abstract partial class Weapon
    {
        [Serializable]
        private struct ShootTypeConfiguration
        {
            [SerializeField, Min(0), Tooltip("Cooldown after shooting in seconds.")]
            private float cooldown;

            [SerializeField, Tooltip("Name of the shoot animation trigger.")]
            private string shootAnimationTrigger;

            [SerializeField, Tooltip("Name of the shake trigger in Camera.")]
            private string shakeAnimationTrigger;

            [SerializeField, Tooltip("Sound played when shooting.")]
            private AudioUnit shootSound;

            [SerializeField, Tooltip("Spawned particles in the cannon.")]
            private GameObject shootParticle;

            [field: SerializeField, IsProperty, Tooltip("Whenever the shoot button of this weapon can be held down.")]
            public bool CanBeHeldDown { get; private set; }

            public bool PlayShootAnimation(Animator animator)
                => Try.SetAnimationTrigger(animator, shootAnimationTrigger, "shoot animation");

            public float GetNextShoot()
                => Time.deltaTime + cooldown;

            public void Shoot(WeaponManager manager, Transform soundTransform, Transform shootParticleTransform)
            {
                manager.TrySetAnimationTriggerOnCamera(shakeAnimationTrigger, "shake");

                Try.PlayOneShoot(soundTransform.position, shootSound, "shoot");

                Vector3 position = shootParticleTransform.position;
                if (shootParticle != null)
                    ParticleSystemPool.GetOrInstantiate(shootParticle, position, Quaternion.identity, shootParticleTransform);
                else
                    Debug.LogWarning("Missing shoot particle prefab.");
            }
        }
    }
}