using Game.Level.Events;
using Game.Menu;
using Game.Player;
using Game.Utility;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Level
{
    public sealed class GameManager : MonoBehaviourSinglenton<GameManager>
    {
        [SerializeField, Tooltip("Object enabled on lose.")]
        private GameObject lose;

        [SerializeField, Tooltip("Aim image.")]
        private Image aim;

        public static bool IsGameRunning => PlayerBody.IsAlive && !GameMenu.IsPaused;

        protected override void Awake_()
        {
            EventManager.Subscribe<PlayerHealthChanged>(OnPlayerHealthChanged);
            EventManager.Subscribe<GameSpeedChanged>(OnGameSpeedChanged);
        }

        private static void OnPlayerHealthChanged(PlayerHealthChanged @event)
        {
            if (@event.IsAlive)
                return;
            Instance.lose.SetActive(true);
            Instance.aim.enabled = false;
            Time.timeScale = 0;
        }

        private static void OnGameSpeedChanged(GameSpeedChanged @event) => Instance.aim.enabled = !@event.IsPaused;

        public void Retry()
        {
            AsyncOperation[] operations = new AsyncOperation[2];
            AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            operations[0] = operation;
            operation.completed += _ =>
            {
                Time.timeScale = 1;
                operations[1] = Resources.UnloadUnusedAssets();
            };
            AsyncOperationBar.Enqueue(operations);
        }

        public void MainMenu()
        {
            AsyncOperation[] operations = new AsyncOperation[2];
            AsyncOperation operation = SceneManager.LoadSceneAsync("Menu");
            operations[0] = operation;
            operation.completed += _ => operations[1] = Resources.UnloadUnusedAssets();
            AsyncOperationBar.Enqueue(operations);
        }

        public void Quit() => Application.Quit();
    }
}