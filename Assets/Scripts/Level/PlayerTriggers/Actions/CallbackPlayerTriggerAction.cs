using UnityEngine;
using UnityEngine.Events;

namespace Game.Level.Triggers
{
    public sealed class CallbackPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Callback executed on enter.")]
        private UnityEvent onEnter;

        [SerializeField, Tooltip("Callback executed on exit.")]
        private UnityEvent onExit;

        public override void OnEnter() => onEnter?.Invoke();

        public override void OnExit() => onExit?.Invoke();
    }
}