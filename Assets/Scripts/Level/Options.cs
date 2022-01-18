using Enderlook.Unity.AudioManager;

using Game.Player;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    public sealed class Options : MonoBehaviour
    {
        [SerializeField]
        private Slider masterVolume;

        [SerializeField]
        private Slider soundVolume;

        [SerializeField]
        private Slider musicVolume;

        [SerializeField]
        private Slider mouseSensibility;

        private void Awake()
        {
            masterVolume.value = AudioController.MasterVolume;
            masterVolume.onValueChanged.AddListener(e => AudioController.MasterVolume = e);

            soundVolume.value = AudioController.SoundVolume;
            soundVolume.onValueChanged.AddListener(e => AudioController.SoundVolume = e);

            musicVolume.value = AudioController.MusicVolume;
            musicVolume.onValueChanged.AddListener(e => AudioController.MusicVolume = e);

            mouseSensibility.value = PlayerController.MouseSensibility;
            mouseSensibility.onValueChanged.AddListener(e => PlayerController.MouseSensibility = e);
        }
    }
}