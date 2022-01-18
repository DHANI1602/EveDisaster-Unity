using Game.Level.Doors;

using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class PlayDialogPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Name of the dialog.")]
        private string name;

        public override void OnEnter() => Dialogs.Play(name);
    }
}