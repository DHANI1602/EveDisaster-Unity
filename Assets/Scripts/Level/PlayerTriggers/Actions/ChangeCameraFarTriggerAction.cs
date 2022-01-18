using Game.Level.Events;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class ChangeCameraFarTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Min(0), Tooltip("Target value for Far property in Camera.")]
        private float target;

        [SerializeField, Min(0), Tooltip("Speed per second at which value is modified. Use 0 for an instantaneous change.")]
        private float speed;

        public override void OnEnter() => EventManager.Raise(new CameraFarChanged(target, speed));
    }
}