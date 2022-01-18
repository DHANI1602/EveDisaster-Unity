using Enderlook.Unity.AudioManager;
using Enderlook.Unity.Toolset.Attributes;

using Game.Level.Events;
using Game.Utility;

using UnityEngine;

namespace Game.Player.Weapons
{
    public sealed class Lantern : MonoBehaviour
    {
        public static Light ActiveLight { get; private set; }

        [Header("Configuration")]
        [SerializeField, Min(0), Tooltip("Duration of light in seconds.")]
        private float duration;

        [SerializeField, Tooltip("Amount of seconds after lantern is disabled to start recharging battery. (Use negative number to never recharge.)")]
        private float startRechargeDelay = -1;

        [SerializeField, Min(0), Tooltip("Seconds of battery worth duration recharged per second.")]
        private float rechargeRate;

        [Header("Animation Triggers")]
        [SerializeField, Tooltip("Animation trigger when run out of battery.")]
        private string outOfBatteryAnimationTrigger;

        [SerializeField, Tooltip("Animation trigger when turn on lantern.")]
        private string turnOnAnimationTrigger;

        [SerializeField, Tooltip("Animation trigger when turn off lantern.")]
        private string turnOffAnimationTrigger;

        [SerializeField, Tooltip("Animation name of idle active lantern.")]
        private string idleAnimationName;

        [Header("Sound")]
        [SerializeField, Tooltip("Sound played when running out of battery.")]
        private AudioFile outOfBatterySound;

        [SerializeField, Tooltip("Sound played when turn on lantern.")]
        private AudioFile turnOnSound;

        [SerializeField, Tooltip("Sound played when turn off lantern.")]
        private AudioFile turnOffSound;

        [Header("Shader")]
        [SerializeField, Tooltip("Object with the material that presents lantern feedback.")]
        private Renderer objectWithLanternRenderer;

        [SerializeField, ShowIf(nameof(objectWithLanternRenderer), null), Tooltip("Field of the shader used to set battery percent.")]
        private string batteryPercentFieldName;

        [SerializeField, ShowIf(nameof(batteryPercentFieldName), null), Min(0.001f), Tooltip("Factor at which battery percent is reduced.")]
        private float batteryPercentFactor = 1;

        [SerializeField, Tooltip("The object with the material that has the halo light effect.")]
        private Renderer haloLightRenderer;

        [SerializeField, ShowIf(nameof(haloLightRenderer), null), Tooltip("Field of the shader used to set opacity of halo.")]
        private string haloLightOpacityFieldName;

        private new Light light;
        private Animator animator;

        private Material batteryShader;
        private Material haloLightShader;

        private float originalOpacity;
        private float originalRange;

        private float currentDuration;
        private float startRechargeAt;

        private bool isInAnimation;

        private float timeWhenLastDisabled = 0;

        private void Awake()
        {
            if (animator != null) // Check if already executed during Initialize method.
                return;

            light = GetComponentInChildren<Light>();
            if (light == null)
                Debug.LogError("Missing Light component in object or children.");
            else
                originalRange = light.range;
            animator = GetComponent<Animator>();

            currentDuration = duration;

            if (objectWithLanternRenderer != null)
            {
                batteryShader = objectWithLanternRenderer.material;
                if (batteryShader == null)
                    Debug.LogWarning("Object with lantern doesn't have material.");
                else if (string.IsNullOrEmpty(batteryPercentFieldName))
                    Debug.LogWarning("Missing batery percent opacity field name.");
            }

            if (haloLightRenderer != null)
            {
                haloLightRenderer.enabled = false;
                haloLightShader = haloLightRenderer.material;
                if (haloLightShader == null)
                    Debug.LogWarning("Halo light doesn't have material.");
                else if (string.IsNullOrEmpty(haloLightOpacityFieldName))
                    Debug.LogWarning("Missing halo light opacity field name.");
                else
                    originalOpacity = haloLightShader.GetFloat(haloLightOpacityFieldName);
            }
        }

        public void Initialize(WeaponManager manager)
        {
            if (animator == null) // Check if this is being called before Awake method.
                Awake();
            SetOffImmediately();
        }

        private void Update()
        {
            if (light == null)
                return;

            if (light.range > 0 && light.intensity > 0 && light.spotAngle > 0 && light.enabled)
            {
                ActiveLight = light;
                light.enabled = true;
                if (haloLightRenderer != null)
                {
                    haloLightRenderer.enabled = true;
                    if (haloLightShader != null)
                        haloLightShader.SetFloat(haloLightOpacityFieldName, originalOpacity * (light.range / originalRange));
                }
            }
            else
            {
                ActiveLight = null;
                light.enabled = false;
                if (haloLightRenderer != null)
                {
                    haloLightRenderer.enabled = false;
                    if (haloLightShader != null)
                        haloLightShader.SetFloat(haloLightOpacityFieldName, 0);
                }
            }

            float oldDuration = currentDuration;
            if (light.enabled)
            {
                if (currentDuration > 0)
                {
                    currentDuration -= Time.deltaTime;
                    SetBatteryUI();
                }
                else
                {
                    isInAnimation = true;
                    if (Try.SetAnimationTrigger(animator, outOfBatteryAnimationTrigger, "out of battery"))
                        EventManager.Raise(new LanternBatteryChanged(oldDuration, duration));
                    else
                        SetOffImmediately();
                    Try.PlayOneShoot(transform, outOfBatterySound, "out of battery");
                }
            }
            else if (!isInAnimation && Time.time >= startRechargeAt)
            {
                currentDuration = Mathf.Min(duration, currentDuration + (Time.deltaTime * rechargeRate));
                SetBatteryUI();
            }

            void SetBatteryUI()
            {
                if (batteryShader != null && !string.IsNullOrEmpty(batteryPercentFieldName))
                    batteryShader.SetFloat(batteryPercentFieldName, Mathf.Pow(Mathf.Max(currentDuration, 0) / duration, batteryPercentFactor));

                EventManager.Raise(new LanternBatteryChanged(oldDuration, currentDuration, duration));
            }
        }

        private void OnEnable()
        {
            float time = Time.time - timeWhenLastDisabled - startRechargeDelay;
            if (time > 0)
                currentDuration = Mathf.Min(currentDuration + (time * rechargeRate), duration);
        }

        private void OnDisable() => timeWhenLastDisabled = Time.time;

        public void SetOnImmediately()
        {
            if (light == null)
                return;

            if (currentDuration <= 0)
                return;

            light.enabled = true;
            Try.SetAnimationName(animator, idleAnimationName, "idle");
            haloLightShader?.SetFloat(haloLightOpacityFieldName, 1);
            if (haloLightRenderer != null)
                haloLightRenderer.enabled = true;
            startRechargeAt = float.PositiveInfinity;
        }

        public void SetOn()
        {
            if (isInAnimation)
                return;

            if (currentDuration <= 0)
                return;

            light.enabled = true;
            if (!Try.SetAnimationTrigger(animator, turnOnAnimationTrigger, "turn on"))
                SetOnImmediately();
            Try.PlayOneShoot(transform, turnOnSound, "turn on");
        }

        public void SetOffImmediately()
        {
            if (light != null)
                light.enabled = false;

            isInAnimation = false;

            haloLightShader?.SetFloat(haloLightOpacityFieldName, 0);
            if (haloLightRenderer != null)
                haloLightRenderer.enabled = false;

            SetRechargeAt();
        }

        private void SetRechargeAt()
        {
            if (startRechargeDelay < 0)
                startRechargeAt = float.PositiveInfinity;
            else
                startRechargeAt = Time.time + startRechargeDelay;
        }

        public void SetOff()
        {
            if (!Try.SetAnimationTrigger(animator, turnOffAnimationTrigger, "turn off"))
                SetOffImmediately();
            Try.PlayOneShoot(transform, turnOffSound, "turn off");
        }

        private void FromOutOfBattery()
        {
            SetOffImmediately();
            EventManager.Raise(new LanternBatteryChanged(duration));
        }

        private void FromTurnOff() => SetRechargeAt();
    }
}
