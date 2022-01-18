using UnityEngine;

namespace Game.Menu
{
    public sealed class AllowBanishOperationBar : MonoBehaviour
    {
        private void Update() // We use update to guarante it's after Start.
        {
            AsyncOperationBar.AllowBanish(true);
            Destroy(this);
        }
    }
}