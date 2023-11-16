using PugSharp.Shared;

namespace PugSharp.Api.G5Api;

public sealed class HeDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public HeDetonatedEvent() : base(new Weapon("hegrenade", CSWeaponId.HEGrenade), "hegrenade_detonated")
    {
    }
}
