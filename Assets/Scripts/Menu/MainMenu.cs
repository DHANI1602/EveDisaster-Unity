using System.Diagnostics;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Menu
{
    public sealed class MainMenu : MonoBehaviour
    {
        private const string up = "Up";
        private const string down = "Down";

        [SerializeField]
        private Animator animator;

        private static bool notFirstTime;

        private void Awake()
        {
            if (!notFirstTime)
            {
                notFirstTime = true;
                Application.quitting += () => Process.GetCurrentProcess().Kill();
            }
        }

        public void Up()
        {
            animator.SetBool(up, true);
            animator.SetBool(down, false);
        }

        public void Down()
        {
            animator.SetBool(up, false);
            animator.SetBool(down, true);
        }

        public void PlayGame()
        {
            AsyncOperationBar.AllowBanish(false);
            AsyncOperationBar.Enqueue(SceneManager.LoadSceneAsync("Level1"));
        }

        public void Quit() => Process.GetCurrentProcess().Kill();
    }
}
