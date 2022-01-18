using Enderlook.Unity.AudioManager;

using Game.Utility;

using UnityEngine;

namespace Game.Enemies
{
    public sealed class Boss : Enemy
    {
        [Header("Sight")]
        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature starts producing a poisonous cloud.")]
        private float cloudRadius = 2;

        [Header("Shoot")]
        [SerializeField, Tooltip("Projectile prefab shooted.")]
        private AcidProjectile projectilePrefab;

        [SerializeField, Tooltip("Determines from which points projectiles are shooted.")]
        private Transform[] shootPositions;

        [SerializeField, Min(0), Tooltip("Amount of seconds of cooldown between each shoot.")]
        private float shootingCooldown = 1;

        [Header("Animation Trigger")]
        [SerializeField, Tooltip("Name of the animation trigger used to shoot (the animation must execute `Shoot()` event at some point and `FromShoot()` at the end).")]
        private string shootAnimationTrigger;

        [Header("Sounds")]
        [SerializeField, Tooltip("Sound played on shoot.")]
        private AudioFile shootSound;

        private bool isInShootingAnimation;
        private float nextShoot;

        private new sealed class State : Enemy.State
        {
            public const byte ShootingToPlayer = 11;
            public const byte SprayingCloud = 12;
        }

        protected override void Awake()
        {
            base.Awake();

            cloudRadius *= cloudRadius;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsAlive)
                return;

            switch (state)
            {
                case State.Idle:
                {
                    if (HasPlayerInSight())
                    {
                        float sqrDistance = (LastPlayerPosition - EyePosition).sqrMagnitude; // Use EyePosition instead of transform.position because the Boss mesh has the offset broken.
                        if (sqrDistance < cloudRadius)
                            GoToSprayState();
                        else if (sqrDistance < SightRadius)
                            GoToShootState();
                    }
                    break;
                }
                case State.ShootingToPlayer:
                {
                    if (isInShootingAnimation)
                        break;

                    if (!HasPlayerInSight())
                        GoToIdleState();
                    else
                    {
                        float sqrDistance = (LastPlayerPosition - EyePosition).sqrMagnitude; // Use EyePosition instead of transform.position because the Boss mesh has the offset broken.
                        if (sqrDistance < cloudRadius)
                        {
                            GoToSprayState();
                            break;
                        }
                        else if (sqrDistance > SightRadius)
                        {
                            GoToIdleState();
                            break;
                        }
                    }

                    if (Time.fixedTime >= nextShoot)
                        ToShoot();

                    break;
                }
                case State.SprayingCloud:
                {
                    if (!HasPlayerInSight())
                        GoToIdleState();
                    else
                    {
                        float sqrDistance = (LastPlayerPosition - EyePosition).sqrMagnitude; // Use EyePosition instead of transform.position because the Boss mesh has the offset broken.
                        if (sqrDistance > cloudRadius)
                        {
                            if (sqrDistance > SightRadius)
                                GoToIdleState();
                            else
                                GoToShootState();
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private void GoToShootState()
        {
            state = State.ShootingToPlayer;
            isInShootingAnimation = false; // Sometimes the flag may have a false positve if the animation was terminated abruptly.
        }

        private void GoToSprayState()
        {
            Debug.LogError("Spray state not implemented");
        }

        protected override void GoToChaseState() => Debug.LogError($"{nameof(Boss)} doesn't have a chase state");

        private void ToShoot()
        {
            isInShootingAnimation = true;

            if (!TrySetAnimationTrigger(shootAnimationTrigger, "shoot"))
            {
                Shoot();
                FromShoot();
            }
        }

        private void Shoot()
        {
            AcidProjectile projectile = Instantiate(projectilePrefab);
            projectile.SetOwner(this);
            Transform projectileTransform = projectile.transform;

            Vector3 lastPlayerPosition = LastPlayerPosition;
            Vector3 position = shootPositions[0].position;
            float minSqrDistance = (lastPlayerPosition - position).sqrMagnitude;
            for (int i = 1; i < shootPositions.Length; i++)
            {
                Vector3 position_ = shootPositions[i].position;
                float sqrDistance = (lastPlayerPosition - position_).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    position = position_;
                    minSqrDistance = sqrDistance;
                }
            }

            projectileTransform.position = position;
            projectileTransform.LookAt(lastPlayerPosition);

            Try.PlayOneShoot(transform, shootSound, AudioPlays, "shoot");
        }

        private void FromShoot()
        {
            isInShootingAnimation = false;
            nextShoot = Time.time + shootingCooldown;
        }

        protected override void OnEndBlind() { }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(EyePosition, SqrtOnPlay(cloudRadius));
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            cloudRadius = Mathf.Min(SightRadius, cloudRadius);
        }
#endif
    }
}