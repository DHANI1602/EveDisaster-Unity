using System.Collections;

using UnityEngine;

namespace Name.Menu
{
    public sealed class MouseLocker : MonoBehaviour
    {
        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;

            StartCoroutine(Work());

            IEnumerator Work()
            {
                yield return null;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
    }
}