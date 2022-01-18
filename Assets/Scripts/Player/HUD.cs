using Game.Level.Events;
using Game.Player.Weapons;
using Game.Utility;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    [DefaultExecutionOrder(1)]
    public sealed class HUD : MonoBehaviourSinglenton<HUD>
    {
        [SerializeField, Tooltip("Health bar image.")]
        private Pack healthBar;

        [SerializeField, Tooltip("Energy bar of lantern.")]
        private Pack energyBar;

        [SerializeField, Tooltip("Energy bar of lantern when running out.")]
        private Image energyBarRunningOutImage;

        [SerializeField, Tooltip("Slot used to show weapon image.")]
        private Image currentWeaponImage;

        private bool isEnergyBarRunningOut;
        private bool ascend;

        private const float ENERGY_BAR_RUNNING_OUT_SPEED = 3;

        private void Awake()
        {
            if (healthBar.Initialize(nameof(healthBar)))
                EventManager.Subscribe<PlayerHealthChanged>(OnPlayerHealthChanged);

            if (energyBar.Initialize(nameof(energyBar)))
                EventManager.Subscribe<LanternBatteryChanged>(OnLanternBatteryChanged);

            if (currentWeaponImage == null)
                Debug.LogWarning($"{nameof(currentWeaponImage)} is null.");
            else
            {
                EventManager.Subscribe<CurrentWeaponChanged>(OnCurrentWeaponChanged);
                currentWeaponImage.sprite = FindObjectOfType<WeaponManager>().CurrentWeapon.WeaponSprite;
            }
        }

        private void Update()
        {
            if (energyBarRunningOutImage != null)
            {
                Color color = energyBarRunningOutImage.color;
                if (isEnergyBarRunningOut)
                {
                    if (ascend)
                    {
                        color.a += Time.deltaTime * ENERGY_BAR_RUNNING_OUT_SPEED;
                        if (color.a > 1)
                        {
                            color.a = 1;
                            ascend = false;
                        }
                    }
                    else
                    {
                        color.a -= Time.deltaTime * ENERGY_BAR_RUNNING_OUT_SPEED;
                        if (color.a < 0)
                        {
                            color.a = 0;
                            ascend = true;
                        }
                    }
                }
                else
                    color.a = Mathf.Max(color.a - (Time.deltaTime * ENERGY_BAR_RUNNING_OUT_SPEED), 0);
                energyBarRunningOutImage.color = color;
            }
        }

        public static void FullHealthEffect()
        {
            HUD instance = Instance;
            instance.healthBar.SetFullAnimation(instance);
        }

        private void OnPlayerHealthChanged(PlayerHealthChanged @event) => healthBar.SetValue(@event.NewHealthPercentage);

        private void OnLanternBatteryChanged(LanternBatteryChanged @event)
        {
            energyBar.SetValue(@event.NewValuePercentage);
            isEnergyBarRunningOut = @event.IsRunningOutOfBattery;
        }

        private void OnCurrentWeaponChanged(CurrentWeaponChanged @event)
        {
            if (currentWeaponImage != null)
                currentWeaponImage.sprite = @event.Weapon.WeaponSprite;
        }

        [Serializable]
        private struct Pack
        {
            [SerializeField, Tooltip("Main image used by the bar.")]
            private Image mainBar;

            [SerializeField, Tooltip("Secondary image used by the bar.")]
            private Image secondaryBar;

            [SerializeField, Tooltip("Colors of the bars given the percentage of them.")]
            private Gradient gradient;

            private Coroutine coroutine;

            public bool Initialize(string name)
            {
                if (mainBar == null)
                {
                    mainBar = null;
                    Debug.LogWarning($"{name} main bar is null.");
                    return false;
                }

                Color color;
                if (gradient != null)
                    color = gradient.Evaluate(1);
                else
                {
                    gradient = new Gradient();
                    color = mainBar != null ? mainBar.color : (secondaryBar != null ? secondaryBar.color : Color.black);
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(color, 0) },
                        new GradientAlphaKey[] { new GradientAlphaKey(color.a, 1) }
                    );
                }

                if (secondaryBar != null)
                    secondaryBar.color = color;

                if (mainBar != null)
                {
                    mainBar.color = color;
                    return true;
                }

                return false;
            }

            public void SetValue(float value)
            {
                Color color = gradient.Evaluate(value);

                if (mainBar != null)
                {
                    mainBar.color = color;
                    if (mainBar.type == Image.Type.Filled)
                        mainBar.fillAmount = value;
                }

                if (secondaryBar != null)
                {
                    secondaryBar.color = color;
                    if (secondaryBar.type == Image.Type.Filled)
                        secondaryBar.fillAmount = value;
                }
            }

            public void SetFullAnimation(HUD hud)
            {
                Image mainBar = this.mainBar;
                Image secondaryBar = this.secondaryBar;

                Debug.Assert(mainBar != null && mainBar.type == Image.Type.Filled ? mainBar.fillAmount == 1 : true);
                Debug.Assert(secondaryBar != null && secondaryBar.type == Image.Type.Filled ? secondaryBar.fillAmount == 1 : true);

                if (coroutine != null)
                    return;

                Color originalColor = gradient.Evaluate(1);

                hud.StartCoroutine(Work());

                IEnumerator Work()
                {
                    float v = 0;
                    while (v < 1)
                    {
                        Color color = Color.Lerp(originalColor, Color.green, Mathf.Pow(v, 3));

                        if (mainBar != null)
                            mainBar.color = color;

                        if (secondaryBar != null)
                            secondaryBar.color = color;

                        v += Time.deltaTime;

                        yield return null;
                    }

                    if (mainBar != null)
                        mainBar.color = Color.green;

                    if (secondaryBar != null)
                        secondaryBar.color = Color.green;

                    yield return null;

                    v = 0;
                    while (v < 1)
                    {
                        Color color = Color.Lerp(Color.green, originalColor, Mathf.Pow(v, 3));

                        if (mainBar != null)
                            mainBar.color = color;

                        if (secondaryBar != null)
                            secondaryBar.color = color;

                        v += Time.deltaTime;

                        yield return null;
                    }

                    if (mainBar != null)
                        mainBar.color = originalColor;

                    if (secondaryBar != null)
                        secondaryBar.color = originalColor;
                }
            }
        }
    }
}