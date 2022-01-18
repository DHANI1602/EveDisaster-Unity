using Enderlook.Unity.AudioManager;

using Game.Player;
using Game.Utility;

using UnityEngine;

namespace Game.Level
{
    public sealed class BackgroundMusic : MonoBehaviourSinglenton<BackgroundMusic>
    {
        [SerializeField, Tooltip("Initial background sound.")]
        private AudioFile backgroundSound;

        private AudioPlay backgroundSoundPlay;
        private AudioPlay otherPlay;

        protected override void Awake_()
            => Try.PlayLoop(PlayerBody.Instance.transform, backgroundSound, out backgroundSoundPlay, "backgroundSound", "player");

        private void Update()
        {
            if (!otherPlay.IsDefault && !otherPlay.IsPlaying && !backgroundSoundPlay.IsDefault && !backgroundSoundPlay.IsPlaying)
                backgroundSoundPlay.Play();
        }

        public static void PlayLoop(AudioFile audio, string name)
        {
            BackgroundMusic instance = Instance;
            if (Try.PlayLoop(PlayerBody.Instance.transform, audio, out instance.otherPlay, name, "player") && !instance.backgroundSoundPlay.IsDefault)
                instance.backgroundSoundPlay.Stop();
        }

        public static void PlayOneShoot(AudioFile audio, string name)
        {
            BackgroundMusic instance = Instance;
            if (Try.PlayOneShoot(PlayerBody.Instance.transform, audio, out instance.otherPlay, name, "player") && !instance.backgroundSoundPlay.IsDefault)
                instance.backgroundSoundPlay.Stop();
        }

        public static void StopOtherPlay()
        {
            BackgroundMusic instance = Instance;
            if (!instance.otherPlay.IsDefault && instance.otherPlay.IsPlaying)
                instance.otherPlay.Stop();
            if (!instance.backgroundSoundPlay.IsDefault && !instance.backgroundSoundPlay.IsPlaying)
                instance.backgroundSoundPlay.Play();
        }
    }
}