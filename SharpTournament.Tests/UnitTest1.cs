namespace SharpTournament.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sharpTournament = new SharpTournament();
            var testUrlConfig = "https://dev.lan2play.de/api/matchmaking/40/configure/1";
            var authToken = "62|jrHJtxIWYE2r9Uh6ippvPv3wqlDhfDBvQyIUo624";
            Assert.True(sharpTournament.LoadConfig(testUrlConfig, authToken));
        }
    }
}