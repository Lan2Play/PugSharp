using PugSharp.Shared;

namespace PugSharp.Api.G5Api;

public sealed class DecoyStartedEvent : PlayerWeaponEvent
{
    public DecoyStartedEvent() : base(new Weapon("decoy", CSWeaponId.Decoy), "decoygrenade_started")
    {
    }
}
