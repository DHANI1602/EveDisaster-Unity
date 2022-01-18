using Game.Player;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    public sealed class InteractableIcon : MonoBehaviour, IInteractableFeedback
    {
        [SerializeField, Tooltip("Color of canvas when is about to interact.")]
        private Color interactableColor;

        private Color originalColor;
        private Image image;

        private bool isInSight;
        private bool isHightlight;

        private void Awake()
        {
            image = GetComponentInChildren<Image>();
            if (image == null)
            {
                image = null;
                Debug.LogWarning($"Component of type {nameof(Image)} not found in children.");
            }
            else
            {
                originalColor = image.color;
                image.enabled = false;
            }
        }

        private void Update()
        {
            if (!(image is null) && (isInSight || isHightlight))
                image.transform.LookAt(PlayerBody.Instance.transform);
        }

        private void OnDisable()
        {
            if (!(image is null))
                image.enabled = false;
        }

        private void OnEnable()
        {
            if (!(image is null))
            {
                image.enabled = true;
                SetCanvas();
            }
        }

        private void SetCanvas()
        {
            if (image is null)
                return;

            if (isHightlight)
            {
                image.color = interactableColor;
                image.enabled = true;
            }
            else if (isInSight)
            {
                image.color = originalColor;
                image.enabled = true;
            }
            else
                image.enabled = false;
        }

        void IInteractableFeedback.Highlight()
        {
            isHightlight = true;
            SetCanvas();
        }

        void IInteractableFeedback.Unhighlight()
        {
            isHightlight = false;
            SetCanvas();
        }

        void IInteractableFeedback.InSight()
        {
            isInSight = true;
            SetCanvas();
        }

        void IInteractableFeedback.OutOfSight()
        {
            isInSight = false;
            SetCanvas();
        }
    }
}