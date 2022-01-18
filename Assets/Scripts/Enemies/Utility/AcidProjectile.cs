using Game.Player;

using System.Collections;

using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class AcidProjectile : MonoBehaviour
    {
        [SerializeField, Tooltip("Amount of damage per second realized to player.")]
        private float damagePerSecond = 5;

        [SerializeField, Tooltip("Amount of seconds the acid damage last.")]
        private float damageDuration = 2;

        [SerializeField, Tooltip("Ticks per second.")]
        private float ticksPerSecond = 2;

        [SerializeField, Tooltip("Movement speed of this projectile.")]
        private float speed = 5;

        [SerializeField, Tooltip("Destruction countdown of this object.")]
        private float destructionCountdown = 15;

        [SerializeField, Tooltip("Destruction countdown of this object after hit.")]
        private float destructionCountdownOnHit = .2f;

        [SerializeField, Tooltip("Layers to ignore on trigger.")]
        private LayerMask layersToIgnoreOnTrigger;

        private Enemy owner;
        private new Rigidbody rigidbody;
        private WaitForSeconds wait;
        private bool done;

        private void Awake()
        {
            Destroy(gameObject, destructionCountdown);
            wait = new WaitForSeconds(1 / ticksPerSecond);
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = null;
                Debug.LogError($"Component of type {nameof(Rigidbody)} was not found.");
            }
        }

        private void Start() => rigidbody?.AddForce(transform.forward * speed, ForceMode.VelocityChange);

        public void SetOwner(Enemy owner) => this.owner = owner;

        private void OnCollisionEnter(Collision collision) => Work(collision.gameObject);

        private void OnTriggerEnter(Collider other)
        {
            if ((layersToIgnoreOnTrigger | (1 << other.gameObject.layer)) != layersToIgnoreOnTrigger)
                Work(other.gameObject);
        }

        private void Work(GameObject other)
        {
            if (done)
                return;

            if (other.GetComponentInParent<Enemy>() == owner)
                return;

            if (!(rigidbody is null))
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.isKinematic = true;
            }

            Destroy(gameObject, destructionCountdownOnHit);

            PlayerBody playerBody = other.GetComponentInParent<PlayerBody>();
            if (playerBody != null)
            {
                done = true;
                playerBody.StartCoroutine(Work());
            }

            IEnumerator Work()
            {
                float damagePerTick = damagePerSecond / ticksPerSecond;
                int ticks = Mathf.CeilToInt(damageDuration * ticksPerSecond);

                for (int i = 0; i < ticks; i++)
                {
                    playerBody.TakeDamage(damagePerTick);
                    yield return wait;
                }
            }
        }
    }
}