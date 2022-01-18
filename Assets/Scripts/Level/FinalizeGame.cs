using UnityEngine;

namespace Game.Level
{
    public sealed class FinalizeGame : MonoBehaviour
    {
        public void Win() => GameManager.Instance.MainMenu();
    }
}