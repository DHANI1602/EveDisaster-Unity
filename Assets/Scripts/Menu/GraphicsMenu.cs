using Enderlook.Text;

using System;
using System.Linq;

using TMPro;

using UnityEngine;

namespace Game.Menu
{
    public sealed class GraphicsMenu : MonoBehaviour
    {
        [SerializeField, Tooltip("Dropdown menu for quality settings.")]
        private TMP_Dropdown quality;

        [SerializeField, Tooltip("Dropdown menu for resolution settings.")]
        private TMP_Dropdown resolution;

        [SerializeField, Tooltip("Dropdown menu for fullscreen settings.")]
        private TMP_Dropdown fullscreen;

        private void Awake()
        {
            quality.ClearOptions();
            quality.AddOptions(QualitySettings.names.ToList());
            quality.value = QualitySettings.GetQualityLevel();
            quality.onValueChanged.AddListener(e => QualitySettings.SetQualityLevel(e, true));

            resolution.ClearOptions();
            resolution.AddOptions(Screen.resolutions.Select(e => $"{e.height}x{e.width} {e.refreshRate}Hz").ToList());
            resolution.value = resolution.options.FindIndex(e =>
            {
                Resolution r = Screen.currentResolution;
                return e.text == $"{r.height}x{r.width} {r.refreshRate}Hz";
            });
            resolution.onValueChanged.AddListener(e =>
            {
                Resolution resolution_ = Screen.resolutions[e];
                Screen.SetResolution(resolution_.width, resolution_.width, Screen.fullScreenMode, resolution_.refreshRate);
            });

            fullscreen.ClearOptions();
            fullscreen.AddOptions(Enum.GetNames(typeof(FullScreenMode)).Select(e => e.SplitByPascalCase(true)).ToList());
            fullscreen.value = fullscreen.options.FindIndex(e => e.text.Replace(" ", "") == Screen.fullScreenMode.ToString());
            fullscreen.onValueChanged.AddListener(e => Screen.fullScreenMode = (FullScreenMode)Enum.Parse(typeof(FullScreenMode), fullscreen.options[e].text.Replace(" ", "")));
        }
    }
}