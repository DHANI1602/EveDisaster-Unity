using Enderlook.Enumerables;
using Enderlook.Unity.AudioManager;
using Enderlook.Unity.Toolset.Attributes;

using Game.Player;
using Game.Player.Weapons;
using Game.Utility;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Animator))]
    public abstract class Enemy : MonoBehaviour, IDamagable, IBlindable
    {
        [SerializeField, Min(0), Tooltip("Maximum health of enemy.")]
        private float maximumHealth;

        [SerializeField, Tooltip("Normalized float variable used by shaders to disable emission after death.")]
        private string emissiveFactorMaterialProperty;

        [SerializeField, ShowIf(nameof(emissiveFactorMaterialProperty), "", ComparisonMode.NotEqual), Tooltip("Time to fade emission after death.")]
        private float fadeEmissionTimeAfterDeath = 1;

        [SerializeField, Tooltip("Root of the ragdoll system.")]
        private Transform ragdollRoot;

        [field: Header("Sight")]
        [field: SerializeField, Min(0), Tooltip("Determines at which radius the creature can see the player.")]
        protected float SightRadius { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Layer that can block enemy sight.")]
        protected LayerMask BlockSight { get; set; }

        [SerializeField, Min(0), Tooltip("Eyes to check line of sight and play sounds from its position.")]
        private Transform Source;

        [Header("Animation Triggers")]
        [SerializeField, Tooltip("Name of the animation trigger when idle.")]
        private string idleAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when it is hurt.")]
        private string hurtAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when it is hurt on the weakspot.")]
        private string hurtWeakspotAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when it dies (the animation must execute `FromDeath()` event on the last frame).")]
        private string deathAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when it dies from an attack on the weakspot (the animation must execute `FromDeath()` event on the last frame).")]
        private string deathWeakspotAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when blinded (the animation must execute `FromBlind()` at the end).")]
        private string blindAnimationTrigger;

        [Header("Sounds")]
        [SerializeField, Tooltip("If true, sounds are not emitted by the creature until it's saw be the player.")]
        private bool muteSoundsUntilSeenByPlayer;

        [SerializeField, Tooltip("Sound played when creature is hurt.")]
        private AudioFile hurtSound;

        [SerializeField, Tooltip("Sound played when creature is hurt on the weakspot.")]
        private AudioFile hurtWeakspotSound;

        [SerializeField, Tooltip("Sound played when creature die.")]
        private AudioFile deathSound;

        [SerializeField, Tooltip("Sound played when creature die on the weakspot.")]
        private AudioFile deathWeakspotSound;

        protected Animator Animator { get; private set; }

        protected bool IsAlive => currentHealth > 0;

        protected Vector3 EyePosition => Source.position;

        protected Vector3 LastPlayerPosition { get; private set; }

        private float currentHealth;

        private string LastAnimationTrigger;

        protected bool IsInBlindAnimation { get; private set; }

        protected byte state;
        private byte previousState;

        protected List<AudioPlay> AudioPlays { get; private set; }

        protected class State
        {
            public const byte Idle = 0;
            public const byte Blinded = 1;

            public static string StateOf<T>(T enemy)
                where T : Enemy
                => Container<T>.StateOf(enemy);

            public static class Container<T>
            {
                private static Dictionary<byte, string> names;

                static Container()
                {
                    names = typeof(T)
                        .GetNestedType(nameof(State), BindingFlags.NonPublic)
                        .GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static)
                        .Where(e => e.IsLiteral && e.FieldType == typeof(byte))
                        .Select(e => (e.Name, (byte)e.GetValue(null)))
                        .ToDictionary(e => e.Item2, e => e.Name);
                }

                public static string StateOf(Enemy enemy) => names[enemy.state];
            }
        }

        protected virtual void Awake()
        {
            currentHealth = maximumHealth;

            if (muteSoundsUntilSeenByPlayer)
                AudioPlays = new List<AudioPlay>();

            Animator = GetComponent<Animator>();

            // Square values to avoid applying square root when checking distance.
            SightRadius *= SightRadius;

            if (Source == null)
            {
                Source = new GameObject("Eye").transform;
                Source.SetParent(transform);
                Source.position = transform.position + transform.up * .5f;
            }

            GoToIdleState();
        }

        protected virtual void FixedUpdate()
        {
            if (!IsAlive)
                return;

            if (!(AudioPlays is null) && GetPlayerInSight().HasValue)
            {
                for (int i = 0; i < AudioPlays.Count; i++)
                {
                    AudioPlay audioPlay = AudioPlays[i];
                    if (audioPlay.IsPlaying)
                        audioPlay.Volume = 1;
                }
                AudioPlays = null;
            }
        }

        protected virtual void GoToIdleState()
        {
            state = State.Idle;

            TrySetAnimationTrigger(idleAnimationTrigger, "idle");
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive)
                return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath(false);
            }
            else
                OnTakeDamage(amount, false);
        }

        public void TakeDamageWeakSpot(float amount)
        {
            if (!IsAlive)
                return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath(true);
            }
            else
                OnTakeDamage(amount, true);
        }

        protected bool HasPlayerInSight()
        {
            if (GetPlayerInSight() is Vector3 player)
            {
                LastPlayerPosition = player;
                return true;
            }
            return false;
        }

        private Vector3? GetPlayerInSight()
        {
            Vector3 eyePosition = EyePosition;
            Vector3 playerPosition = PlayerBody.Instance.transform.position;
            if ((playerPosition - eyePosition).sqrMagnitude < SightRadius
                && !Physics.Linecast(playerPosition, eyePosition, BlockSight, QueryTriggerInteraction.Ignore))
                return playerPosition;
            return null;
        }

        protected virtual void OnTakeDamage(float amount, bool isOnWeakspot)
        {
            SetLastPlayerPosition();

            if (isOnWeakspot)
            {
                if (!Try.PlayOneShoot(Source, hurtWeakspotSound, AudioPlays, "hurt weakspot"))
                    Try.PlayOneShoot(Source, hurtSound, AudioPlays, "hurt");

                if (!TrySetAnimationTrigger(hurtWeakspotAnimationTrigger, "hurt weakspot", false))
                    TrySetAnimationTrigger(hurtAnimationTrigger, "hurt", false);
            }
            else
            {
                Try.PlayOneShoot(Source, hurtSound, AudioPlays, "hurt");
                TrySetAnimationTrigger(hurtAnimationTrigger, "hurt", false);
            }
        }

        public virtual void MakeAwareOfPlayerLocation()
        {
            SetLastPlayerPosition();
            GoToChaseState();
        }

        protected void SetLastPlayerPosition() => LastPlayerPosition = PlayerBody.Instance.transform.position;

        protected abstract void GoToChaseState();

        protected virtual void FromHurt() => TrySetLastAnimationTrigger();

        protected virtual void OnDeath(bool isOnWeakspot)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>())
                collider.enabled = false;

            if (isOnWeakspot)
            {
                if (!Try.PlayOneShoot(Source, deathWeakspotSound, AudioPlays, "death weakspot"))
                    Try.PlayOneShoot(Source, deathSound, AudioPlays, "death");

                if (!TrySetAnimationTrigger(deathWeakspotAnimationTrigger, "death weakspot", true, true)
                    && !TrySetAnimationTrigger(deathAnimationTrigger, "death", true, true))
                    FromDeath();
            }
            else
            {
                Try.PlayOneShoot(Source, deathSound, AudioPlays, "death");
                if (!TrySetAnimationTrigger(deathAnimationTrigger, "death", true, true))
                    FromDeath();
            }

            if (!string.IsNullOrEmpty(emissiveFactorMaterialProperty) && fadeEmissionTimeAfterDeath > 0)
            {
                IEnumerable<(float, Material e)> array = GetComponentsInChildren<Renderer>(true)
                    .SelectMany(e => e.materials)
                    .Where(e => e.HasProperty(emissiveFactorMaterialProperty))
                    .Select(e => (e.GetFloat(emissiveFactorMaterialProperty), e));

                if (!array.Any())
                    Debug.LogWarning($"No {nameof(Renderer)} component with material with property {emissiveFactorMaterialProperty} was found.");
                else
                    StartCoroutine(Work());

                IEnumerator Work()
                {
                    (float, Material e)[] array_ = array.ToArray();
                    float v = fadeEmissionTimeAfterDeath;
                    while (v > 0)
                    {
                        v -= Time.deltaTime;
                        foreach ((float originalValue, Material material) in array_)
                            material.SetFloat(emissiveFactorMaterialProperty, originalValue * v);
                        yield return null;
                    }

                    foreach ((float originalValue, Material material) in array_)
                        material.SetFloat(emissiveFactorMaterialProperty, 0);
                }
            }
        }

        public void FromDeath()
        {
            StartCoroutine(Work());
            IEnumerator Work()
            {
                yield return null;
                ApplyDeath();
            }
        }

        protected virtual Vector3 OnDeathVelocity() => default;

        protected virtual void ApplyDeath()
        {
            Animator.enabled = false;

            if (ragdollRoot == null)
                return;

            Rigidbody rigidbody = ragdollRoot.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.velocity = OnDeathVelocity();

            ragdollRoot.gameObject.SetActive(true);

            int fM = transform.childCount;
            int tM = ragdollRoot.childCount;
            for (int fC = 0, tC = 0; fC < fM && tC < tM; tC++, fC++)
            {
                Transform tT = ragdollRoot.GetChild(tC);
                again:
                Transform fT = transform.GetChild(fC);
                if (fT == ragdollRoot)
                    continue;

                if (fT.name == tT.name)
                    TransferTransformData(fT, tT);
                else
                {
                    fC++;
                    if (fC >= fM)
                        break;
                    goto again;
                }
            }

            for (int i = 0; i < fM; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != ragdollRoot)
                    Destroy(child.gameObject);
            }

            void TransferTransformData(Transform from, Transform to)
            {
                to.SetPositionAndRotation(from.position, from.rotation);
                to.localScale = from.localScale;

                int fM_ = from.childCount;
                int tM_ = to.childCount;

                int fC_ = 0;
                int tC_ = 0;

                while (fC_ < fM_ && tC_ < tM_)
                {
                    Transform tT_ = to.GetChild(tC_);
                    again:
                    Transform fT_ = from.GetChild(fC_);

                    if (fT_.name == tT_.name)
                    {
                        TransferTransformData(fT_, tT_);
                        fC_++;
                        tC_++;
                    }
                    else
                    {
                        fC_++;
                        if (fC_ >= fM_)
                            break;
                        goto again;
                    }
                }
            }
        }

        protected bool TrySetAnimationTrigger(string triggerName, string triggerMetaName, bool recordAsLastAnimation = true, bool disableAllTriggers = false)
        {
            if (disableAllTriggers)
            {
                foreach (AnimatorControllerParameter parameter in Animator.parameters)
                {
                    if (parameter.type == AnimatorControllerParameterType.Trigger)
                        Animator.ResetTrigger(parameter.nameHash);
                }
            }

            if (Try.SetAnimationTrigger(Animator, triggerName, triggerMetaName))
            {
                if (recordAsLastAnimation)
                    LastAnimationTrigger = triggerName;
                return true;
            }
            else
            {
                LastAnimationTrigger = "";
                return false;
            }
        }

        protected void TrySetLastAnimationTrigger()
        {
            if (!string.IsNullOrEmpty(LastAnimationTrigger))
                Animator.SetTrigger(LastAnimationTrigger);
        }

        public void Blind()
        {
            SetLastPlayerPosition();

            if (state != State.Blinded)
            {
                SaveStateAsPrevious();
                state = State.Blinded;
            }

            IsInBlindAnimation = true;
            if (!TrySetAnimationTrigger(blindAnimationTrigger, "blind", false))
                FromBlind();
        }

        private void FromBlind()
        {
            IsInBlindAnimation = false;

            LoadPreviousState();

            TrySetLastAnimationTrigger();

            OnEndBlind();
        }

        protected abstract void OnEndBlind();

        protected void SaveStateAsPrevious() => previousState = state;

        protected void LoadPreviousState() => state = previousState;

        private void PlayOneShoot(AudioFile audio) => Try.PlayOneShoot(Source, audio, AudioPlays, "from animation");

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(EyePosition, SqrtOnPlay(SightRadius));

            if (PlayerBody.Instance != null)
            {
                Gizmos.color = GetPlayerInSight() is null ? Color.gray : Color.green;
                Gizmos.DrawLine(PlayerBody.Instance.transform.position, EyePosition);
            }
        }

        protected float SqrtOnPlay(float value) => Application.isPlaying ? Mathf.Sqrt(value) : value;

        protected virtual void OnValidate() => fadeEmissionTimeAfterDeath = Mathf.Max(fadeEmissionTimeAfterDeath, 0);
#endif
    }
}