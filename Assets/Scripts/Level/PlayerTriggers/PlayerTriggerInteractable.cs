using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class PlayerTriggerInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField, Tooltip("If true, actions will only be triggered once and the object will be no longer interactable.")]
        private bool triggerOnce;

        [SerializeField, Tooltip("If true, interactions will switch between turn on and off. Otherwise, interactions will always turn on.")]
        private bool toggleable;

        [SerializeReference, Tooltip("Actions to execute when player get in or out of the trigger.")]
        private PlayerTriggerAction[] actions;

        private State state;

        private enum State : byte
        {
            Uninitialized,
            In,
            Initialized,
        }

        void IInteractable.Interact()
        {
            switch (state)
            {
                case State.Initialized:
                    bool spent = triggerOnce && !toggleable;
                    foreach (PlayerTriggerAction action in actions)
                        action.OnEnter();
                    if (spent)
                        Destroy(this);
                    else
                        state = State.In;
                    break;
                case State.Uninitialized:
                    state = State.Initialized;
                    foreach (PlayerTriggerAction action in actions)
                        action.Initialize(this);
                    goto case State.Initialized;
                case State.In:
                    if (toggleable)
                        goto case State.Initialized;
                    foreach (PlayerTriggerAction action in actions)
                        action.OnExit();
                    if (triggerOnce)
                        Destroy(this);
                    else
                        state = State.Initialized;
                    break;
            }
        }
    }
}