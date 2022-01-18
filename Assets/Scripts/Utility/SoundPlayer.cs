using Enderlook.Unity.AudioManager;

using Game.Level;

using System;

using UnityEngine;

namespace Game.Utility
{
    public sealed class SoundPlayer : MonoBehaviour
    {
        public void PlayOneShoot(AudioFile audio)
        {
            if (enabled)
                AudioController.PlayOneShoot(audio, transform.position);
        }

        [Obsolete]
        public void PlayAudioOneShootBag(AudioFile audio)
        {
            Debug.LogWarning($"This method is obsolete. Use {nameof(PlayOneShoot)} instead.");
            AudioController.PlayOneShoot(audio, transform.position);
        }

        public void ChangeMusic() => BackgroundMusic.StopOtherPlay();
    }
}