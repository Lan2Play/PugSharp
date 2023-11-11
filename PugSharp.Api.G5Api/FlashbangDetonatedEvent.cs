namespace PugSharp.Api.G5Api;

public sealed class FlashbangDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public FlashbangDetonatedEvent() : base( new Weapon("flashbang", CsWeaponId.FLASHBANG), "flashbang_detonated")
    {
    }
}
