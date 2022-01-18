using Enderlook.Unity.AudioManager;

using Game.Player;
using Game.Player.Weapons;
using Game.Utility;

using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemies
{
    public sealed class Egger : MovableEnemy
    {
        [Header("Sight")]
        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature start shooting.")]
        private float startShootingRadius = 2;

        [SerializeField, Min(0), Tooltip("Determines at which distance from player the creature stop shooting.")]
        private float stopShootingRadius = 3;

        [Header("Escape")]
        [SerializeField, Min(0), Tooltip("If length of the largest escape route is lower than this value, the creature will prefer to fight back rathen than run away.")]
        private float fightOrEscapeThreshold = .5f;

        [Header("Hurt Escape")]
        [SerializeField, Min(0), Tooltip("Duration in seconds that last panic after loosing line of sight with player.")]
        private float panicDuration = 2;

        [SerializeField, Min(0), Tooltip("Width of the oscillation when producing zig-zag.")]
        private float oscillationWidth = 1.5f;

        [SerializeField, Min(0), Tooltip("Frequency of the oscillation when producing zig-zag.")]
        private float oscillationFrequency = 3;

        [SerializeField, Min(0), Tooltip("Speed multiplier when escaping.")]
        private float escapingSpeedMultiplier = 1;

        [Header("Light Escape")]
        [SerializeField, Min(0), Tooltip("Duration in seconds that light sensibility last after getting out of light range.")]
        private float lightSensibilityDuration = 1;

        [SerializeField, Min(0), Tooltip("Duration in seconds that light sensibility can't be triggered after recoving from light.")]
        private float lightImmunityDuration = 1;

        [Header("Shoot")]
        [SerializeField, Tooltip("Projectile prefab shooted.")]
        private AcidProjectile projectilePrefab;

        [SerializeField, Tooltip("Determines from which point projectiles are shooted.")]
        private Transform shootPosition;

        [SerializeField, Min(0), Tooltip("Amount of seconds of cooldown between each shoot.")]
        private float shootingCooldown = 1;

        [Header("Animation Triggers")]
        [SerializeField, Tooltip("Name of the animation trigger when is running towards the player to attack him.")]
        private string huntAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when lose line of sight to player and is running to its last position.")]
        private string chaseAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger used to shoot (the animation must execute `Shoot()` event at some point and `FromShoot()` at the end).")]
        private string shootAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when is running from the player to espace.")]
        private string escapeFromPlayerAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when is running from the light to espace.")]
        private string escapeFromLightAnimationTrigger;

        [Header("Sounds")]
        [SerializeField, Tooltip("Sound played on shoot.")]
        private AudioFile shootSound;

        private bool isInShootingAnimation;

        private float nextShoot;
        private float panicsEndsAt;
        private float lightImmunityEndsAt;

        private new sealed class State : Enemy.State
        {
            public const byte HuntingPlayer = 10;
            public const byte ChasingPlayer = 11;
            public const byte ShootingToPlayer = 12;
            public const byte EscapingFromPlayer = 13;
            public const byte EscapingFromLight = 14;
        }

        protected override void Awake()
        {
            base.Awake();
            NavAgent.AllowJump = true;

            // Square values to avoid applying square root when checking distance.
            startShootingRadius *= startShootingRadius;
            stopShootingRadius *= stopShootingRadius;
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
                    if (HasLightInRange() && TryGoToEscapeFromLightState())
                        break;
                    if (HasPlayerInSight())
                        GoToHuntState();
                    break;
                }
                case State.HuntingPlayer:
                {
                    if (HasLightInRange() && TryGoToEscapeFromLightState())
                        break;

                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }

                    NavAgent.SetDestination(LastPlayerPosition);

                    if ((LastPlayerPosition - transform.position).sqrMagnitude <= startShootingRadius)
                        GoToShootState();
                    break;
                }
                case State.ChasingPlayer:
                {
                    if (HasLightInRange() && TryGoToEscapeFromLightState())
                        break;
                    if (HasPlayerInSight())
                    {
                        if ((LastPlayerPosition - transform.position).sqrMagnitude <= startShootingRadius)
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
                case State.ShootingToPlayer:
                {
                    if (isInShootingAnimation)
                        break;

                    if (HasLightInRange() && TryGoToEscapeFromLightState())
                        break;

                    if (!HasPlayerInSight())
                    {
                        GoToChaseState();
                        break;
                    }
                    else if ((LastPlayerPosition - transform.position).sqrMagnitude > stopShootingRadius)
                    {
                        GoToHuntState();
                        break;
                    }

                    LookAtPlayer();

                    if (Time.fixedTime >= nextShoot)
                        ToShoot();

                    break;
                }
                case State.EscapingFromPlayer:
                {
                    if (HasPlayerInSight())
                    {
                        panicsEndsAt = Time.fixedTime + panicDuration;
                        SetEscapeDestinationOrFight(LastPlayerPosition);
                    }
                    else if (panicsEndsAt > Time.fixedTime)
                        SetEscapeDestinationOrFight(LastPlayerPosition);
                    else if (!isInShootingAnimation)
                        GoToIdleState();
                    break;
                }
                case State.EscapingFromLight:
                {
                    if (HasLightInRange())
                    {
                        panicsEndsAt = Time.fixedTime + lightSensibilityDuration;
                        SetEscapeDestinationOrFight(LastPlayerPosition);
                    }
                    else if (panicsEndsAt > Time.fixedTime)
                        SetEscapeDestinationOrFight(LastPlayerPosition);
                    else if (!isInShootingAnimation)
                    {
                        lightImmunityEndsAt = Time.fixedTime + lightImmunityDuration;
                        GoToIdleState();
                    }
                    break;
                }
            }
        }

        protected override void GoToChaseState()
        {
            state = State.ChasingPlayer;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(1);
            NavAgent.SetDestination(LastPlayerPosition);

            TrySetAnimationTrigger(chaseAnimationTrigger, "chase");
        }

        private bool TryGoToEscapeFromLightState()
        {
            if (Time.fixedTime < lightImmunityEndsAt)
                return false;

            state = State.EscapingFromLight;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(escapingSpeedMultiplier);
            SetEscapeDestinationOrFight(PlayerBody.Instance.transform.position);

            TrySetAnimationTrigger(escapeFromLightAnimationTrigger, "escape from light");
            return true;
        }

        private void GoToEscapeFromPlayerState()
        {
            state = State.EscapingFromPlayer;
            panicsEndsAt = Time.fixedTime + panicDuration;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(escapingSpeedMultiplier);
            SetEscapeDestinationOrFight(PlayerBody.Instance.transform.position);

            TrySetAnimationTrigger(escapeFromPlayerAnimationTrigger, "escape");
        }

        private void GoToHuntState()
        {
            state = State.HuntingPlayer;
            NavAgent.Resume();
            NavAgent.SetSpeedMultiplier(1);
            NavAgent.SetDestination(LastPlayerPosition);

            TrySetAnimationTrigger(huntAnimationTrigger, "hunt");
        }

        private void GoToShootState()
        {
            state = State.ShootingToPlayer;
            NavAgent.Stop();
            isInShootingAnimation = false; // Sometimes the flag may have a false positve if the animation was terminated abruptly.
        }

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
            projectileTransform.position = shootPosition.position;
            projectileTransform.LookAt(LastPlayerPosition);

            Try.PlayOneShoot(transform, shootSound, AudioPlays, "shoot");
        }

        private void FromShoot()
        {
            isInShootingAnimation = false;
            nextShoot = Time.time + shootingCooldown;
        }

        private bool HasLightInRange()
        {
            Light light = Lantern.ActiveLight;
            if (light == null)
                return false;

            Transform lightTranform = light.transform;

            Vector3 enemyPosition = EyePosition;
            Vector3 lightPosition = lightTranform.position;

            Vector3 lightDirection = enemyPosition - lightPosition;
            float distanceToConeOrigin = lightDirection.sqrMagnitude;
            float range = light.range;
            if (distanceToConeOrigin < range * range)
            {
                Vector3 coneDirection = lightTranform.forward;
                float angle = Vector3.Angle(coneDirection, lightDirection);
                if (angle < light.spotAngle * .5f && !Physics.Linecast(enemyPosition, lightPosition, BlockSight))
                    return true;
            }

            return false;
        }

        protected override void OnTakeDamage(float amount, bool isOnWeakspot)
        {
            base.OnTakeDamage(amount, isOnWeakspot);
            GoToEscapeFromPlayerState();
        }

        private void SetEscapeDestinationOrFight(Vector3 playerPosition)
        {
            if (NavAgent.IsJumping)
                return;

            Vector3 eyePosition = EyePosition;
            (Vector3 end, float distance, bool canOscillate) result = GroupCheck(EyePosition);

            if (NavAgent.CanJump && Physics.Raycast(eyePosition, transform.up, out RaycastHit raycastHit, BlockSight)
                && NavMesh.SamplePosition(raycastHit.point, out NavMeshHit navmeshHit, NavMeshAgentHelper.MAXIMUM_DISTANCE_SAMPLING, NavMesh.AllAreas))
            {
                (Vector3 end, float distance, bool canOscillate) second = GroupCheck(navmeshHit.position - (transform.position - eyePosition));
                if (second.distance < result.distance)
                {
                    result = second;
                    if (NavAgent.Jump(navmeshHit.position))
                        return;
                }
            }

            if (result.distance < fightOrEscapeThreshold)
            {
                if (NavAgent.IsJumping || isInShootingAnimation)
                    return;

                SetLastPlayerPosition();
                Debug.Assert((LastPlayerPosition - transform.position).sqrMagnitude <= stopShootingRadius);

                LookAtPlayer();

                if (Time.time >= nextShoot)
                    ToShoot();

                return;
            }

            if (result.canOscillate && oscillationFrequency != 0 && oscillationWidth != 0)
            {
                Vector3 direction = (result.end - eyePosition).normalized;
                Vector3 perpendicular = new Vector3(direction.z, direction.y, direction.x);
                float wave = Mathf.Sin(Time.fixedTime * oscillationFrequency) * oscillationWidth;
                Vector3 desviation = oscillationWidth * wave * perpendicular;
                Vector3 newDirection = ((direction * .5f) + desviation).normalized;

                Vector3 destination;
                if (Physics.Raycast(eyePosition, newDirection, out raycastHit, 1, BlockSight))
                    destination = raycastHit.point;
                else
                    destination = eyePosition + newDirection;

                if (NavMesh.SamplePosition(destination, out navmeshHit, 4, NavMesh.AllAreas))
                    destination = navmeshHit.position;

                if (!NavAgent.SetDestination(destination))
                {
                    if (NavMesh.SamplePosition(result.end, out navmeshHit, 4, NavMesh.AllAreas))
                        result.end = navmeshHit.position;
                    NavAgent.SetDestination(result.end);
                }
            }
            else
            {
                if (NavMesh.SamplePosition(result.end, out navmeshHit, 4, NavMesh.AllAreas))
                    result.end = navmeshHit.position;
                NavAgent.SetDestination(result.end);
            }

            (Vector3 end, float distance, bool canOscillate) GroupCheck(Vector3 eyePosition)
            {
                float distance = float.NegativeInfinity;
                Vector3 end = default;
                bool canOscillate = default;

                playerPosition.y = eyePosition.y;

                Vector3 back = (eyePosition - playerPosition).normalized;
                Vector3 forward = -back;

                Check(back, true);

                Vector3 left = new Vector3(back.z, back.y, back.x);
                Check(left, false);
                Check(back + (left * .25f), true);
                Check(back + (left * .5f), true);
                Check(back + (left * .75f), false);
                Check(back + (left * 1f), false);
                Check(back + (left * 1.5f), false);
                Check(back + (left * 2f), false);
                Check(back + (left * 4f), false);
                Check(back + (left * 8f), false);
                Check(forward + (left * 8f), false);

                Vector3 right = new Vector3(back.z, back.y, -back.x);
                Check(right, false);
                Check(back + (right * .25f), false);
                Check(back + (right * .5f), false);
                Check(back + (right * .75f), false);
                Check(back + (right * 1f), false);
                Check(back + (right * 1.5f), false);
                Check(back + (right * 2f), false);
                Check(back + (right * 4f), false);
                Check(back + (right * 8f), false);
                Check(forward + (right * 8f), false);

                return (end, distance, canOscillate);

                void Check(Vector3 direction, bool oscillate)
                {
                    if (Physics.Raycast(eyePosition, direction, out RaycastHit hit, BlockSight)
                        && hit.distance > distance)
                    {
                        distance = hit.distance;
                        end = hit.point;
                        canOscillate = oscillate;
                    }
                }
            }
        }

        protected override void OnEndBlind()
        {
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
            if (state == State.EscapingFromLight || state == State.EscapingFromPlayer)
                return escapingSpeedMultiplier;
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
            Gizmos.DrawWireSphere(eyePosition, fightOrEscapeThreshold);

            float distance = float.NegativeInfinity;
            Vector3 end = default;
            bool canOscillate = default;

            PlayerBody instance = PlayerBody.Instance;
            if (instance != null)
            {
                Vector3 playerPosition = instance.transform.position;
                playerPosition.y = eyePosition.y;

                Vector3 back = (eyePosition - playerPosition).normalized;
                Vector3 forward = -back;

                Check(back, true);

                Vector3 left = new Vector3(back.z, back.y, back.x);
                Check(left, false);
                Check(back + (left * .25f), true);
                Check(back + (left * .5f), true);
                Check(back + (left * .75f), false);
                Check(back + (left * 1f), false);
                Check(back + (left * 1.5f), false);
                Check(back + (left * 2f), false);
                Check(back + (left * 4f), false);
                Check(back + (left * 8f), false);
                Check(forward + (left * 8f), false);

                Vector3 right = new Vector3(back.z, back.y, -back.x);
                Check(right, false);
                Check(back + (right * .25f), false);
                Check(back + (right * .5f), false);
                Check(back + (right * .75f), false);
                Check(back + (right * 1f), false);
                Check(back + (right * 1.5f), false);
                Check(back + (right * 2f), false);
                Check(back + (right * 4f), false);
                Check(back + (right * 8f), false);
                Check(forward + (right * 8f), false);

                Check(transform.up, false);

                void Check(Vector3 direction, bool oscillate)
                {
                    if (Physics.Raycast(eyePosition, direction, out RaycastHit hit, BlockSight))
                    {
                        Gizmos.color = oscillate ? Color.red : Color.yellow;
                        Gizmos.DrawLine(eyePosition, hit.point);

                        if (hit.distance > distance)
                        {
                            distance = hit.distance;
                            end = hit.point;
                            canOscillate = oscillate;
                        }
                    }
                    else
                    {
                        Gizmos.color = oscillate ? Color.cyan : Color.blue;
                        Gizmos.DrawLine(eyePosition, eyePosition + direction);
                    }
                }
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePosition, end);

            if (canOscillate && oscillationFrequency != 0 && oscillationWidth != 0)
            {
                Gizmos.color = Color.magenta;
                Vector3 direction = (end - eyePosition).normalized;

                Vector3 perpendicular = new Vector3(direction.z, direction.y, direction.x);
                const int c = 5;
                for (int i = 0; i < c * 10; i++)
                {
                    Gizmos.DrawLine(Point(i), Point(i + 1));

                    Vector3 Point(float v)
                    {
                        v /= c;
                        float wave = Mathf.Sin((v + Time.fixedTime) * oscillationFrequency) * oscillationWidth;
                        Vector3 newDirection = oscillationWidth * wave * perpendicular;
                        return eyePosition + (direction * v) + newDirection;
                    }
                }

                float wave = Mathf.Sin(Time.fixedTime * oscillationFrequency) * oscillationWidth;
                Vector3 desviation = oscillationWidth * wave * perpendicular;
                Vector3 newDirection = (direction * .5f) + desviation;

                Vector3 destination;
                if (Physics.Raycast(eyePosition, newDirection, out RaycastHit hit, 1, BlockSight))
                    destination = hit.point;
                else
                    destination = eyePosition + newDirection;

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(eyePosition, destination);
                if (NavMesh.SamplePosition(destination, out NavMeshHit hit2, 4, NavMesh.AllAreas))
                    destination = hit2.position;

                Gizmos.color = Color.black;
                Gizmos.DrawLine(eyePosition, destination);
            }
            else
            {
                if (NavMesh.SamplePosition(end, out NavMeshHit hit, 4, NavMesh.AllAreas))
                    end = hit.position;

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(eyePosition, end);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            stopShootingRadius = Mathf.Min(SightRadius, stopShootingRadius);
            startShootingRadius = Mathf.Min(startShootingRadius, stopShootingRadius);
            fightOrEscapeThreshold = Mathf.Min(fightOrEscapeThreshold, stopShootingRadius);
        }
#endif
    }
}