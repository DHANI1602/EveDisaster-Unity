using Enderlook.Unity.AudioManager;

using Game.Player;
using Game.Utility;

using System;
using System.Collections;

using UnityEngine;

namespace Game.Enemies
{
    public sealed class Crawler : MovableEnemy
    {
        [Header("Sight")]
        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature start shooting.")]
        private float startShootingRadius = 3;

        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature stop shooting.")]
        private float stopShootingRadius = 4;

        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature start charging.")]
        private float startChargeRadius = 2;

        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature start melee.")]
        private float startMeleeRadius = .75f;

        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature stop melee.")]
        private float stopMeleeRadius = 1.5f;

        [Header("Melee")]
        [SerializeField, Min(0), Tooltip("Movement speed multiplier when charging to player.")]
        private float chargingSpeedMultiplier = 1;

        [SerializeField, Min(0), Tooltip("Amount of damage produced on melee strike.")]
        private float meleeDamage = 20;

        [SerializeField, Tooltip("Determines from which collider melee damage is done.")]
        private Collider meleePosition;

        [Header("Stun")]
        [SerializeField, Tooltip("Amount of accumulated damage required from weakspot to stun the creature.")]
        private float stunDamageRequired = 20;

        [SerializeField, Tooltip("Time required to lose accumulated damage for stun.")]
        private float stunClearTimer = 5;

        [SerializeField, Tooltip("Duration after an stun where creature doesn't accumulate damage.")]
        private float stunImmunityDuration = 2;

        [Header("Animation Triggers")]
        [SerializeField, Tooltip("The mouth object with animation of acid puke.")]
        private Animator mouthAnimator;

        [SerializeField, Tooltip("Name of the animation trigger for mouth.")]
        private string mouthAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when stunned (the animation must execute `FromStun()` at the end).")]
        private string stunAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when is running towards the player to attack him.")]
        private string huntAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when lose line of sight to player and is running to its last position.")]
        private string chaseAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when is charging towards player.")]
        private string chargeAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger used to shoot (the animation must execute `Shoot()` event at some point and `FromShoot()` at the end).")]
        private string shootAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger used to melee (the animation must execute `Melee()` event at some point and `FromMelee()` at the end).")]
        private string meleeAnimationTrigger;

        [Header("Sounds")]
        [SerializeField, Tooltip("Sound played on shoot.")]
        private AudioFile shootSound;

        [SerializeField, Tooltip("Sound played on melee.")]
        private AudioFile meleeSound;

        private MeleeAttack meleeAttack;

        private bool isInShootingAnimation;
        private bool isInMeleeAnimation;
        private bool isInStunAnimation;

        private float accumulatedDamage;
        private float accumulatedDamageCleanAt;
        private float accumulatedImmunityUntil;

        private new sealed class State : Enemy.State
        {
            public const byte HuntingPlayer = 10;
            public const byte ChasingPlayer = 11;
            public const byte ChargeToPlayer = 12;
            public const byte Shooting = 13;
            public const byte Melee = 14;
            public const byte Stunned = 15;
        }

        protected override void Awake()
        {
            base.Awake();

            meleePosition.enabled = false;
            meleeAttack = meleePosition.gameObject.AddComponent<MeleeAttack>();
            meleeAttack.crawler = this;
            meleeAttack.enabled = false;

            // Square values to avoid applying square root when checking distance.
            startShootingRadius *= startShootingRadius;
            stopShootingRadius *= stopShootingRadius;
            startMeleeRadius *= startMeleeRadius;
            stopMeleeRadius *= stopMeleeRadius;
            startChargeRadius *= startChargeRadius;
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
                        GoToHuntState();
                    break;
                }
                case State.HuntingPlayer:
                {
                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }

                    NavAgent.SetDestination(LastPlayerPosition);

                    float sqrDistance = (LastPlayerPosition - transform.position).sqrMagnitude;
                    if (sqrDistance <= startMeleeRadius)
                        GoToMeleeState();
                    else if (sqrDistance <= startChargeRadius)
                        GoToChargeState();
                    else if (sqrDistance <= startShootingRadius)
                        GoToShootState();
                    break;
                }
                case State.ChasingPlayer:
                {
                    if (HasPlayerInSight())
                    {
                        float sqrDistance = (LastPlayerPosition - transform.position).sqrMagnitude;
                        if (sqrDistance <= startMeleeRadius)
                            GoToMeleeState();
                        else if (sqrDistance <= startChargeRadius)
                            GoToChargeState();
                        else if (sqrDistance <= startShootingRadius)
                            GoToShootState();
                        else
                            GoToHuntState();
                    }
                    else if (NavAgent.HasReachedDestination())
                        GoToIdleState();
                    else
                    {
                        if (Time.frameCount % 10 == 0)
                            // Relcalculate path just in case.
                            NavAgent.SetDestination(LastPlayerPosition);
                    }
                    break;
                }
                case State.Shooting:
                {
                    if (isInShootingAnimation)
                        break;

                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }

                    float sqrDistance = (LastPlayerPosition - transform.position).sqrMagnitude;
                    if (sqrDistance > stopShootingRadius)
                    {
                        GoToHuntState();
                        break;
                    }
                    else if (sqrDistance <= startMeleeRadius)
                    {
                        GoToMeleeState();
                        break;
                    }
                    else if (sqrDistance <= startChargeRadius)
                    {
                        GoToChargeState();
                        break;
                    }

                    LookAtPlayer();

                    ToShoot();

                    break;
                }
                case State.Melee:
                {
                    if (isInMeleeAnimation)
                        break;

                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }

                    float sqrDistance = (LastPlayerPosition - transform.position).sqrMagnitude;
                    if (sqrDistance > startMeleeRadius)
                    {
                        GoToHuntState();
                        break;
                    }

                    LookAtPlayer();

                    ToMelee();

                    break;
                }
                case State.ChargeToPlayer:
                {
                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }

                    float sqrDistance = (LastPlayerPosition - transform.position).sqrMagnitude;
                    if (sqrDistance <= startMeleeRadius)
                    {
                        GoToMeleeState();
                        break;
                    }

                    NavAgent.SetDestination(LastPlayerPosition);
                    break;
                }
            }
        }

        public void TriggerMouthAttackAnimation()
        {
            if (string.IsNullOrEmpty(mouthAnimationTrigger))
                Debug.LogWarning("Missing mouth animation trigger.");
            else
                mouthAnimator.SetTrigger(mouthAnimationTrigger);
        }

        private void GoToChargeState()
        {
            state = State.ChasingPlayer;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(chargingSpeedMultiplier);
            NavAgent.SetDestination(LastPlayerPosition);

            TrySetAnimationTrigger(chargeAnimationTrigger, "charge");
        }

        protected override void GoToChaseState()
        {
            state = State.ChasingPlayer;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(1);
            NavAgent.SetDestination(LastPlayerPosition);

            TrySetAnimationTrigger(chaseAnimationTrigger, "chase");
        }

        private void GoToHuntState()
        {
            state = State.HuntingPlayer;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(1);
            NavAgent.SetDestination(LastPlayerPosition);

            TrySetAnimationTrigger(huntAnimationTrigger, "hunt");
        }

        private void GoToMeleeState()
        {
            state = State.Melee;
            isInMeleeAnimation = false; // Sometimes the flag may have a false positive if the animation was terminated abruptly.
            NavAgent.Stop();
        }

        private void GoToShootState()
        {
            state = State.Shooting;
            isInShootingAnimation = false; // Sometimes the flag may have a false positive if the animation was terminated abruptly.
            NavAgent.Stop();
        }

        private void ToShoot()
        {
            isInShootingAnimation = true;

            Try.PlayOneShoot(transform, shootSound, AudioPlays, "shoot");

            if (!TrySetAnimationTrigger(shootAnimationTrigger, "shoot"))
                FromShoot();
        }

        private void FromShoot() => isInShootingAnimation = false;

        private void ToMelee()
        {
            isInMeleeAnimation = true;

            Try.PlayOneShoot(transform, meleeSound, AudioPlays, "melee");

            if (!TrySetAnimationTrigger(meleeAnimationTrigger, "melee"))
            {
                Melee();
                FromMelee();
            }
        }

        private void Melee()
        {
            StartCoroutine(Work());
            IEnumerator Work()
            {
                meleePosition.enabled = true;
                yield return null;
                meleePosition.enabled = false;
            }
        }

        private void FromMelee() => isInMeleeAnimation = false;

        protected override void OnTakeDamage(float amount, bool isOnWeakspot)
        {
            base.OnTakeDamage(amount, isOnWeakspot);
            if (isOnWeakspot)
            {
                if (isInStunAnimation || IsInBlindAnimation || accumulatedImmunityUntil > Time.time)
                    return;
                if (Time.time >= accumulatedDamageCleanAt)
                    accumulatedDamage = 0;
                accumulatedDamage += amount;
                if (accumulatedDamage > stunDamageRequired)
                {
                    SaveStateAsPrevious();
                    state = State.Stunned;
                    accumulatedImmunityUntil = Time.time + stunImmunityDuration;

                    TrySetAnimationTrigger(stunAnimationTrigger, "stun");
                }
                else
                    accumulatedDamageCleanAt = Time.time + stunClearTimer;
            }
            if (state == State.Idle)
                GoToChaseState();
        }

        private void FromStun()
        {
            isInStunAnimation = false;
            accumulatedDamage = 0;
            accumulatedImmunityUntil = Time.time + stunImmunityDuration;
            LoadPreviousState();
            TrySetLastAnimationTrigger();
        }

        protected override void OnEndBlind()
        {
            isInStunAnimation = false; // Sometimes the flag may have a false positive if the animation was terminated abruptly.
            if (state == State.Idle)
                GoToChaseState();
        }

        public override void MakeAwareOfPlayerLocation()
        {
            base.MakeAwareOfPlayerLocation();
            switch (state)
            {
                case State.ChasingPlayer:
                    NavAgent.SetDestination(LastPlayerPosition);
                    break;
                case State.Idle:
                    GoToChaseState();
                    break;
            }
        }

        protected override float GetStateSpeedMultiplier()
        {
            if (state == State.ChasingPlayer || state == State.HuntingPlayer)
                return chargingSpeedMultiplier;
            return 1;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector3 eyePosition = EyePosition;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(eyePosition, SqrtOnPlay(startShootingRadius));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyePosition, SqrtOnPlay(stopShootingRadius));

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(eyePosition, SqrtOnPlay(startChargeRadius));

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(eyePosition, SqrtOnPlay(startMeleeRadius));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(eyePosition, SqrtOnPlay(stopMeleeRadius));
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (meleePosition != null)
                meleePosition.isTrigger = true;
            stopShootingRadius = Mathf.Min(SightRadius, stopShootingRadius);
            startShootingRadius = Mathf.Min(startShootingRadius, stopShootingRadius);
            startChargeRadius = Mathf.Min(startChargeRadius, startShootingRadius);
            stopMeleeRadius = Mathf.Min(stopMeleeRadius, startChargeRadius);
            stopMeleeRadius = Mathf.Min(SightRadius, stopMeleeRadius);
            startMeleeRadius = Mathf.Min(startMeleeRadius, stopMeleeRadius);
        }
#endif

        private sealed class MeleeAttack : MonoBehaviour
        {
            [NonSerialized]
            public Crawler crawler;

            private void OnTriggerEnter(Collider other)
            {
                PlayerBody player = other.GetComponentInParent<PlayerBody>();
                if (player != null)
                    player.TakeDamage(crawler.meleeDamage);
            }
        }
    }
}