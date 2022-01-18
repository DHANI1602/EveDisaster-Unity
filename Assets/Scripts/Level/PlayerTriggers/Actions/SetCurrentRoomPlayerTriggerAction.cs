using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class SetCurrentRoomPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Name of the room.")]
        private string roomName;

        public override void OnEnter() => CurrentRoomText.SetRoomName(roomName);
    }
}