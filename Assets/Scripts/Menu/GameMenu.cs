using Game.Level.Events;
using Game.Menu;
using Game.Player;
using Game.Utility;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Level
{
    public sealed class GameMenu : MonoBehaviourSinglenton<GameMenu>
    {
        [SerializeField, Tooltip("GameObject that holds all panels.")]
        private GameObject root;

        [SerializeField, Tooltip("GameObject that holds main panel.")]
        private GameObject mainPanel;

        [SerializeField, Tooltip("GameObject that holds options panel.")]
        private GameObject optionsPanel;

        private bool IsPausedInstance => root.activeSelf;

        public static bool IsPaused => Instance.IsPausedInstance;

        private bool blocked;

        private void Update()
        {
            if (!PlayerBody.IsAlive || blocked)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPausedInstance)
                {
                    if (optionsPanel.activeSelf)
                    {
                        optionsPanel.SetActive(false);
                        mainPanel.SetActive(true);
                    }
                    else
                        Continue();
                }
                else
                {
                    Time.timeScale = 0;
                    root.SetActive(true);
                    EventManager.Raise(new GameSpeedChanged(true));
                }
            }
        }

        public void Continue()
        {
            root.SetActive(false);
            Time.timeScale = 1;
            EventManager.Raise(new GameSpeedChanged(false));
        }

        public void GoToMainMenu()
        {
            blocked = true;
            AsyncOperation operation = SceneManager.LoadSceneAsync("Menu");
            operation.completed += _ => Time.timeScale = 1;
            AsyncOperationBar.Enqueue(operation);
        }
    }
}