namespace PugSharp.Api.G5Api;

public sealed class DemoFinishedEvent : DemoFileEvent
{
    public DemoFinishedEvent(string matchId, int mapNumber, string fileName) : base(matchId, mapNumber, fileName, "demo_finished")
    {
    }
}
