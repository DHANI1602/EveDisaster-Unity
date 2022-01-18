using UnityEngine;

namespace Game.Effects
{
    [RequireComponent(typeof(Animator))]
    public sealed class SteamParticleStateSetter : MonoBehaviour
    {
        [SerializeField, Tooltip("Determinates if the effect is Soft, Mid, or Aggressive.")]
        private Strength leakStrenght;

        private enum Strength
        {
            Soft,
            Mid,
            Aggressive,
        }

        private void Awake()
        {
            Animator animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogWarning($"{nameof(Animator)} not found.");
            else
            {
                switch (leakStrenght)
                {
                    case Strength.Soft:
                        animator.SetBool("Soft", true);
                        break;
                    case Strength.Mid:
                        animator.SetBool("Mid", true);
                        break;
                    case Strength.Aggressive:
                        animator.SetBool("Aggresive", true); // The typo of `Aggresive` instead of `Aggressive` is on purpose.
                        break;
                    default:
                        Debug.LogWarning($"Unsuported {nameof(leakStrenght)} value: {leakStrenght}.");
                        break;
                }
            }

            Destroy(this);
        }
    }
}