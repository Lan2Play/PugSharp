namespace PugSharp.Api.G5Api;

public sealed class GrenadeThrownEvent : PlayerWeaponEvent
{
    public GrenadeThrownEvent(Weapon weapon) : base(weapon, "grenade_thrown")
    {
    }
}
