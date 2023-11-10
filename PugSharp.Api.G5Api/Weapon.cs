using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class Weapon
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("id")]
    public CsWeaponId CsWeaponID { get; }

    public Weapon(string name, CsWeaponId csWeaponID)
    {
        Name = name;
        CsWeaponID = csWeaponID;
    }
}
