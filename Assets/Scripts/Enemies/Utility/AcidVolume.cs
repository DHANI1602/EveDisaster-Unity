using Game.Player;

using System.Collections;

using UnityEngine;

namespace Game.Enemies
{
    public sealed class AcidVolume : MonoBehaviour
    {
        [SerializeField, Tooltip("Amount of damage per second realized to player.")]
        private float damagePerSecond = 5;

        [SerializeField, Tooltip("Ticks per second.")]
        private float ticksPerSecond = 2;

        private Coroutine coroutine;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.GetComponentInParent<PlayerBody>() == null)
                return;

            coroutine = StartCoroutine(Work());

            IEnumerator Work()
            {
                WaitForSeconds wait = new WaitForSeconds(1 / ticksPerSecond);
                PlayerBody player = PlayerBody.Instance;
                while (true)
                {
                    player.TakeDamage(damagePerSecond / ticksPerSecond);
                    yield return wait;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.GetComponentInParent<PlayerBody>() == null)
                return;

            StopCoroutine(coroutine);
            coroutine = null;
        }

        private void OnDisable()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }
}