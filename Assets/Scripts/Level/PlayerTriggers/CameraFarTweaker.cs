using Game.Level.Events;

using UnityEngine;

namespace Game.Level.Triggers
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFarTweaker : MonoBehaviour
    {
        private new Camera camera;
        private float target;
        private float speed;

        private void Awake()
        {
            camera = GetComponent<Camera>();
            EventManager.Subscribe<CameraFarChanged>(OnCameraFarChanged);
        }

        private void Update()
        {
            camera.farClipPlane = Mathf.MoveTowards(camera.farClipPlane, target, speed * Time.deltaTime);
            if (camera.farClipPlane == target)
                enabled = false;
        }

        private void OnCameraFarChanged(CameraFarChanged @event)
        {
            if (@event.Speed == 0)
                camera.farClipPlane = @event.Target;
            else
            {
                target = @event.Target;
                speed = @event.Speed;
                enabled = true;
            }
        }
    }
}