using Enderlook.Unity.AudioManager;

using Game.Level;
using Game.Utility;

using UnityEngine;

namespace Game.Player.Weapons
{
    public sealed class WeaponPickup : MonoBehaviour, IPickup, IInteractable
    {
        [SerializeField, Tooltip("Name of weapon unlocked.")]
        private string weaponName;

        [SerializeField, Tooltip("Audio played on pickup.")]
        private AudioUnit audioOnPickup;

        void IInteractable.Interact() => Pickup();

        public void Pickup()
        {
            WeaponManager weaponManager = WeaponManager.Instance;

            weaponManager.UnlockWeapon(weaponName);

            Try.PlayOneShoot(transform.position, audioOnPickup, "on pickup");

            Destroy(gameObject);
        }
    }
}