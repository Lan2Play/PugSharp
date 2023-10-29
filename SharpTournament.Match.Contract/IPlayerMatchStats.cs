namespace SharpTournament.Match.Contract;

public interface IPlayerMatchStats
{
    int Kills { get; set; }
    int Assists { get; set; }
    int Deaths { get; set; }

    void ResetStats();
}
