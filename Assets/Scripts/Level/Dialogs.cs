using Enderlook.Unity.AudioManager;

using Game.Player;
using Game.Utility;

using System;

using UnityEngine;

namespace Game.Level
{
    [RequireComponent(typeof(Animator))]
    public sealed class Dialogs : MonoBehaviourSinglenton<Dialogs>
    {
        [SerializeField, Tooltip("List with all the possible dialogs.")]
        private Dialog[] dialogs;

        private Animator animator;

        protected override void Awake_() => animator = GetComponent<Animator>();

        public static void Play(string name)
        {
            Dialog[] dialogs = Instance.dialogs;
            Animator animator = Instance.animator;
            for (int i = 0; i < dialogs.Length; i++)
                if (dialogs[i].Play(name, animator))
                    return;

            Debug.LogError($"Dialog with name '{name}' was not found.");
        }

        [Serializable]
        private struct Dialog
        {
            [SerializeField, Tooltip("Name of the dialog.")]
            private string name;

            [SerializeField, Tooltip("Name of the animation trigger to raise.")]
            private string animationTriggerName;

            [SerializeField, Tooltip("Audio file to play.")]
            private AudioFile audio;

            public bool Play(string name, Animator animator)
            {
                if (this.name != name)
                    return false;

                Try.SetAnimationTrigger(animator, animationTriggerName, nameof(animationTriggerName));
                Try.PlayOneShoot(PlayerBody.Instance.transform, audio, nameof(audio));
                return true;
            }
        }
    }
}