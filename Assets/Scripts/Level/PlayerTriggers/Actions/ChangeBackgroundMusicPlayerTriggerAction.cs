using Enderlook.Unity.AudioManager;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class ChangeBackgroundMusicPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Audio to play in background.")]
        private AudioFile audio;

        [SerializeField, Tooltip("If true, the audio will loop.")]
        private bool loop;

        [SerializeField, Tooltip("If true, the background music will change to default when player get out of trigger.")]
        private bool stopWhenGetOut;

        public override void OnEnter()
        {
            if (loop)
                BackgroundMusic.PlayLoop(audio, "audio");
            else
                BackgroundMusic.PlayOneShoot(audio, "audio");
        }

        public override void OnExit()
        {
            if (stopWhenGetOut)
                BackgroundMusic.StopOtherPlay();
        }
    }
}