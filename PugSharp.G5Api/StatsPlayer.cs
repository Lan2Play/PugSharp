using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class StatsPlayer
{
    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int Assists { get; set; }

    [JsonPropertyName("flash_assists")]
    public int FlashAssists { get; set; }

    [JsonPropertyName("team_kills")]
    public int TeamKills { get; set; }

    [JsonPropertyName("suicides")]
    public int Suicides { get; set; }

    [JsonPropertyName("damage")]
    public int Damage { get; set; }

    [JsonPropertyName("utility_damage")]
    public int UtilityDamage { get; set; }

    [JsonPropertyName("enemies_flashed")]
    public int EnemiesFlashed { get; set; }

    [JsonPropertyName("friendlies_flashed")]
    public int FriendliesFlashed { get; set; }

    [JsonPropertyName("knife_kills")]
    public int KnifeKills { get; set; }

    [JsonPropertyName("headshot_kills")]
    public int HeadshotKills { get; set; }

    [JsonPropertyName("rounds_played")]
    public int RoundsPlayed { get; set; }

    [JsonPropertyName("bomb_defuses")]
    public int BombDefuses { get; set; }

    [JsonPropertyName("bomb_plants")]
    public int BombPlants { get; set; }

    [JsonPropertyName("1k")]
    public int Kills1 { get; set; }

    [JsonPropertyName("2k")]
    public int Kills2 { get; set; }

    [JsonPropertyName("3k")]
    public int Kills3 { get; set; }

    [JsonPropertyName("4k")]
    public int Kills4 { get; set; }

    [JsonPropertyName("5k")]
    public int Kills5 { get; set; }

    [JsonPropertyName("1v1")]
    public int OneV1s { get; set; }

    [JsonPropertyName("1v1")]
    public int OneV2s { get; set; }

    [JsonPropertyName("1v3")]
    public int OneV3s { get; set; }

    [JsonPropertyName("1v4")]
    public int OneV4s { get; set; }

    [JsonPropertyName("1v5")]
    public int OneV5s { get; set; }

    [JsonPropertyName("first_kills_t")]
    public int FirstKillsT { get; set; }

    [JsonPropertyName("first_kills_ct")]
    public int FirstKillsCT { get; set; }

    [JsonPropertyName("first_deaths_t")]
    public int FirstDeathsT { get; set; }

    [JsonPropertyName("first_deaths_ct")]
    public int FirstDeathsCT { get; set; }

    [JsonPropertyName("trade_kills")]
    public int TradeKills { get; set; }

    [JsonPropertyName("kast")]
    public int Kast { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("mvp")]
    public int Mvps { get; set; }
}