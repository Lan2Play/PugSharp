using System.Text.Json.Serialization;

using PugSharp.Shared;

namespace PugSharp.Api.G5Api;

public class Weapon
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("id")]
    public CSWeaponId CsWeaponID { get; }

    public Weapon(string name, CSWeaponId csWeaponID)
    {
        Name = name;
        CsWeaponID = csWeaponID;
    }
}
