using Game.Level.Events;
using Game.Utility;

using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(HurtShaderController))]
    public sealed class PlayerBody : MonoBehaviourSinglenton<PlayerBody>, IDamagable
    {
        public static bool IsAlive => Instance.currentHealth > 0;

        [SerializeField, Tooltip("Maximum amount of health the player has.")]
        private float health = 100;

        [Header("Setup")]
        [SerializeField, Tooltip("The animator trigger that goes to ShakeHard animation.")]
        private string shakeHardTrigger;

        [SerializeField, Tooltip("The animator trigger that goes to ShakeLight animation.")]
        private string shakeLightTrigger;

        private Animator playerCameraAnimator;

        private HurtShaderController hurtShaderController;

        private float currentHealth;

        public float HealthPercentage => currentHealth / health;

        protected override void Awake_()
        {
            currentHealth = health;
            hurtShaderController = GetComponent<HurtShaderController>();
            playerCameraAnimator = GetComponentInChildren<Animator>();

            EventManager.Raise(new PlayerHealthChanged(currentHealth, currentHealth, health));
        }

        public void TakeDamage(float amount)
        {
            hurtShaderController.SetBlood(amount / 5); // We set the feedback according in how much damage we took.

            if (amount < 10)
                playerCameraAnimator.SetTrigger(shakeLightTrigger);
            else
                playerCameraAnimator.SetTrigger(shakeHardTrigger);

            float oldHealth = currentHealth;
            float newHealth = currentHealth - amount;
            if (newHealth <= 0)
            {
                if (oldHealth == 0)
                    return;
                newHealth = 0;
            }
            currentHealth = newHealth;
            EventManager.Raise(new PlayerHealthChanged(oldHealth, newHealth, health));
        }

        public void TakeHealing(float amount)
        {
            float oldHealth = currentHealth;
            float maximumHealth = health;
            float newHealth = Mathf.Min(currentHealth + amount, maximumHealth);
            currentHealth = newHealth;
            EventManager.Raise(new PlayerHealthChanged(oldHealth, newHealth, maximumHealth));
        }
    }
}
