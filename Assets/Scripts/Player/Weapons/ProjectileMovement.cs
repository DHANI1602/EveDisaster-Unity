using UnityEngine;

namespace Game.Player.Weapons
{
    [RequireComponent(typeof(Collider))]
    public sealed class ProjectileMovement : MonoBehaviour
    {
        [SerializeField, Tooltip("Speed of projectile.")]
        private float speed;

        private Vector3? stopAtPosition;
        private ParticleSystem[] particleSystems;

        private void Awake() => particleSystems = GetComponentsInChildren<ParticleSystem>();

        private void FixedUpdate()
        {
            if (stopAtPosition is Vector3 position)
            {
                transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.fixedDeltaTime);
                if (transform.position == position)
                {
                    stopAtPosition = default;
                    goto returnToPool;
                }
                return;
            }

            transform.position += speed * Time.fixedDeltaTime * transform.forward;

            return;
            returnToPool:
            foreach (ParticleSystem particleSystem in particleSystems)
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void SetDirection(Vector3 direction, float maximumDistance)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            stopAtPosition = transform.position + (direction * maximumDistance);
        }

        public void SetDestination(Vector3 point)
        {
            transform.rotation = Quaternion.LookRotation((point - transform.position).normalized);
            stopAtPosition = point;
        }
    }
}