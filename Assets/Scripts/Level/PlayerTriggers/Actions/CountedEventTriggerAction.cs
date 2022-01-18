using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class CountedEventTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Name of the variable to modify.")]
        private string variableName;

        [SerializeField, Tooltip("Value to add on enter.")]
        private float onEnterIncreaseValueBy;

        [SerializeField, Tooltip("Value to add on exit.")]
        private float onExitIncreaseValueBy;

        public override void OnEnter()
        {
            if (onEnterIncreaseValueBy != 0)
                CountersManager.Mutate(variableName, onEnterIncreaseValueBy);
        }

        public override void OnExit()
        {
            if (onExitIncreaseValueBy != 0)
                CountersManager.Mutate(variableName, onExitIncreaseValueBy);
        }
    }
}