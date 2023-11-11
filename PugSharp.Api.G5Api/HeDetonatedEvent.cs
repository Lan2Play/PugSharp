namespace PugSharp.Api.G5Api;

public sealed class HeDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public HeDetonatedEvent() : base(new Weapon("hegrenade", CsWeaponId.HEGRENADE), "hegrenade_detonated")
    {
    }
}
