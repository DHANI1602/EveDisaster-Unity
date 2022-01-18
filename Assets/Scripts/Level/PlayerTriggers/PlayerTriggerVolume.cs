using Game.Player;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class PlayerTriggerVolume : MonoBehaviour
    {
        [SerializeField, Tooltip("If true, actions will only be triggered once.")]
        private bool triggerOnce;

        [SerializeReference, Tooltip("Actions to execute when player get in or out of the trigger.")]
        private PlayerTriggerAction[] actions;

        private State state;

        private enum State : byte
        {
            Uninitialized,
            In,
            Initialized,
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (state)
            {
                case State.Initialized:
                    if (other.transform.GetComponentInParent<PlayerBody>() == null)
                        return;
                    state = State.In;
                    foreach (PlayerTriggerAction action in actions)
                        action.OnEnter();
                    break;
                case State.Uninitialized:
                    state = State.Initialized;
                    foreach (PlayerTriggerAction action in actions)
                        action.Initialize(this);
                    goto case State.Initialized;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (state != State.In)
                return;

            if (other.transform.GetComponentInParent<PlayerBody>() == null)
                return;

            foreach (PlayerTriggerAction action in actions)
                action.OnExit();

            if (triggerOnce)
                Destroy(this);
            else
                state = State.Initialized;
        }
    }
}