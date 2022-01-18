using Game.Player.Weapons;

namespace Game.Level.Events
{
    public readonly struct CurrentWeaponChanged
    {
        public readonly Weapon Weapon;

        public CurrentWeaponChanged(Weapon weapon) => Weapon = weapon;
    }
}