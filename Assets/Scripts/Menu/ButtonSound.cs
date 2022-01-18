using Enderlook.Unity.AudioManager;

using Game.Utility;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Menu
{
    public sealed class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        [SerializeField, Tooltip("Sound played on mouse down.")]
        private AudioFile onMouseDown;

        [SerializeField, Tooltip("Sound played on mouse in.")]
        private AudioFile onMouseIn;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
            => Try.PlayOneShoot(Camera.main.transform.position, onMouseDown, nameof(onMouseDown));

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
            => Try.PlayOneShoot(Camera.main.transform.position, onMouseIn, nameof(onMouseIn));
    }
}