using PugSharp.Shared;

namespace PugSharp.Api.G5Api;

public sealed class FlashbangDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public FlashbangDetonatedEvent() : base(new Weapon("flashbang", CSWeaponId.Flashbang), "flashbang_detonated")
    {
    }
}
