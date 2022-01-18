using Game.Player;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class PlayerPickupInteractable : MonoBehaviour, IInteractable, IPickup
    {
        [SerializeReference, Tooltip("Actions to execute when object is interacted.")]
        private PlayerTriggerAction[] actions;

        void IInteractable.Interact() => Pickup();

        public void Pickup()
        {
            foreach (PlayerTriggerAction action in actions)
                action.OnEnter();

            Destroy(gameObject);
        }
    }
}