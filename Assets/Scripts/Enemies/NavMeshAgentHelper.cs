using System;
using System.Collections;

using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemies
{
    public partial class MovableEnemy
    {
        [Serializable]
        public struct NavMeshAgentHelper
        {
            public const float MAXIMUM_DISTANCE_SAMPLING = 4;
            private const float STOPPING_ACCELERATION_MULTIPLIER = 5;
            private const float JUMPING_SPEED = 3;
            private const float JUMP_COOLDOWN = 2;

            private static WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

            [SerializeField, Tooltip("Mass of the agent.")]
            private float mass;

            [NonSerialized]
            public bool AllowJump;
            private NavMeshAgent agent;
            private MovableEnemy enemy;
            private Transform transform;
            private NavMeshPath path;
            private float defaultSpeed;
            private float defaultAcceleration;
            private float multiplier;
            private Vector3? jumpDestination;
            private float nextJumpAt;

            public bool IsJumping => jumpDestination.HasValue;
            public bool CanJump => AllowJump && Time.fixedTime > nextJumpAt;
            public Vector3 Velocity => agent.velocity;

            public void Initialize(MovableEnemy enemy)
            {
                multiplier = 1;

                if (enemy == null)
                    Debug.LogWarning($"{nameof(enemy)} was not found.");
                else
                {
                    this.enemy = enemy;
                    transform = enemy.transform;

                    if ((agent = enemy.GetComponent<NavMeshAgent>()) == null)
                    {
                        agent = null; // Convert to real null.
                        Debug.LogWarning($"{nameof(NavMeshAgent)} was not found.");
                    }
                    else
                    {
                        defaultSpeed = agent.speed;
                        defaultAcceleration = agent.acceleration;
                    }
                }
            }

            public bool SetDestination(Vector3 target)
            {
                if (agent is null)
                    return false;

                if (jumpDestination is Vector3 jump)
                {
                    if (path is null)
                        path = new NavMeshPath();
                    return NavMesh.CalculatePath(jump, target, 0, path);
                }

                if (!agent.SetDestination(target))
                {
                    if (AllowJump)
                    {
                        if (Physics.Raycast(enemy.EyePosition, transform.up, out RaycastHit raycastHit, enemy.BlockSight)
                            && NavMesh.SamplePosition(raycastHit.point, out NavMeshHit navmeshHit, MAXIMUM_DISTANCE_SAMPLING, NavMesh.AllAreas))
                        {
                            if (path is null)
                                path = new NavMeshPath();
                            if (NavMesh.CalculatePath(navmeshHit.position, target, 0, path))
                            {
                                enemy.StartCoroutine(JumpCoroutine(enemy, navmeshHit.position));
                                return true;
                            }
                        }
                        Debug.Log($"Path to {target} from {transform.position} in gameobject {transform.name} not found, even when trying to use ceil/floor walking.", agent);
                    }
                    else
                        Debug.Log($"Path to {target} from {transform.position} in gameobject {transform.name} not found.", agent);
                    return false;
                }
                return true;
            }

            public bool HasReachedDestination()
            {
                // NavMeshAgent.remainingDistance doesn't work fine.
                if (IsJumping || agent is null)
                    return false;

                if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                    return true; // Is this fine for us?

                float stopingDistance = agent.stoppingDistance;
                Vector3[] corners = agent.path.corners;

                if (corners.Length == 0)
                    return true;

                if (corners.Length == 1)
                    return Vector3.Distance(transform.position, corners[0]) <= stopingDistance;

                float distance = 0.0f;
                for (int i = 0; i < corners.Length - 1; ++i)
                {
                    distance += Vector3.Distance(corners[i], corners[i + 1]);
                    if (distance <= stopingDistance)
                        return true;
                }

                distance += Vector3.Distance(corners[corners.Length - 1], transform.position);

                return distance <= stopingDistance;
            }

            public void Disable()
            {
                if (!(agent is null))
                    agent.enabled = false;
            }

            public void Stop()
            {
                if (agent is null)
                    return;
                agent.speed = 0;
                agent.acceleration = defaultAcceleration * STOPPING_ACCELERATION_MULTIPLIER;
            }

            public void Resume()
            {
                if (agent is null)
                    return;
                SetSpeedMultiplier(multiplier);
                agent.acceleration = defaultAcceleration;
            }

            public void SetSpeedMultiplier(float multiplier)
            {
                if (agent is null)
                    return;
                this.multiplier = multiplier;
                agent.speed = defaultSpeed * multiplier;
            }

            public void ApplyForce(Vector3 force)
            {
                if (!(IsJumping || agent is null))
                    agent.velocity += force / mass;
            }

            public bool Jump(Vector3 newPosition)
            {
                if (!IsJumping)
                {
                    if (AllowJump)
                    {
                        enemy.StartCoroutine(JumpCoroutine(enemy, newPosition));
                        return true;
                    }
                    else
                        Debug.LogError("Enemy is not allowed to jump.");
                }

                return false;
            }

            public void LookAt(Vector3 location)
            {
                Vector3 direction = location - transform.position;
                direction.y = 0;
                enemy.StartCoroutine(LookAtCoroutine(enemy, direction));
            }

            private static IEnumerator LookAtCoroutine(MovableEnemy enemy, Vector3 direction)
            {
                enemy.NavAgent.agent.updateRotation = false;
                while (enemy.NavAgent.Rotate(direction))
                    yield return fixedUpdate;
                enemy.NavAgent.agent.updateRotation = true;
            }

            private static IEnumerator JumpCoroutine(MovableEnemy enemy, Vector3 newPosition)
            {
                const float firstSection = .2f;
                const float secondSection = .8f;

                enemy.NavAgent.jumpDestination = newPosition;
                Transform transform = enemy.transform;
                NavMeshAgent agent = enemy.NavAgent.agent;
                agent.enabled = false;

                Vector3 start = transform.position;
                float delta = 0;
                while (delta < firstSection)
                {
                    yield return fixedUpdate;
                    delta += Time.fixedDeltaTime * JUMPING_SPEED;
                    transform.position = Vector3.Lerp(start, newPosition, delta);
                }
                delta = firstSection;
                transform.position = Vector3.Lerp(start, newPosition, delta);

                Vector3 up = transform.up;
                Vector3 down = -up;
                while (delta < secondSection)
                {
                    yield return fixedUpdate;
                    delta += Time.fixedDeltaTime * JUMPING_SPEED;
                    transform.position = Vector3.Lerp(start, newPosition, delta);
                    transform.up = Vector3.Lerp(up, down, (delta - firstSection) / (secondSection - firstSection));
                }
                delta = secondSection;
                transform.position = Vector3.Lerp(start, newPosition, delta);
                transform.up = down;

                while (delta < 1f)
                {
                    yield return fixedUpdate;
                    delta += Time.fixedDeltaTime * JUMPING_SPEED;
                    transform.position = Vector3.Lerp(start, newPosition, delta);
                }
                transform.position = newPosition;

                agent.enabled = true;
                bool success = agent.Warp(newPosition);
                Debug.Assert(success);
                NavMeshPath path_ = agent.path;
                agent.SetPath(agent.path);
                path_.ClearCorners();
                agent.path = path_;
                enemy.NavAgent.jumpDestination = null;
                enemy.NavAgent.nextJumpAt = Time.fixedTime + JUMP_COOLDOWN;
            }

            private bool Rotate(Vector3 direction)
            {
                Quaternion to = Quaternion.LookRotation(direction);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, to, (agent?.angularSpeed ?? 360) * Time.fixedDeltaTime);
                transform.rotation = newRotation;
                return newRotation == to;
            }

            public void TryDrawPathInGizmos()
            {
                if (agent == null)
                    return;

                if ((IsJumping ? path : (agent.hasPath ? agent.path : null))?.corners is Vector3[] corners)
                {
                    Vector3 previousPoint = agent.transform.position;
                    Gizmos.color = Color.red;
                    foreach (Vector3 point in corners)
                    {
                        Gizmos.DrawLine(previousPoint, point);
                        previousPoint = point;
                    }
                }
            }
        }
    } }