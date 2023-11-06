namespace PugSharp.Match.Contract;

public interface IPlayerMatchStats
{
    int Kills { get; set; }
    int Assists { get; set; }
    int Deaths { get; set; }
    int Suicides { get; set; }
    int FirstDeathCt { get; set; }
    int FirstDeathT { get; set; }
    int TeamKills { get; set; }
    int FirstKillT { get; set; }
    int FirstKillCt { get; set; }
    int HeadshotKills { get; set; }
    int KnifeKills { get; set; }
    int FlashbangAssists { get; set; }

    void ResetStats();
}
