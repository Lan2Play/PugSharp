namespace PugSharp.Api.G5Api;

public sealed class DecoyStartedEvent : PlayerWeaponEvent
{
    public DecoyStartedEvent() : base( new Weapon("decoy", CsWeaponId.DECOY), "decoygrenade_started")
    {
    }
}
