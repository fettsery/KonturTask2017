using System;
using Kontur.GameStats.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UpdateStatsTests
    {
        [TestMethod]
        public void TestServerStatsUpdate()
        {
            ServerStats stats = new ServerStats();
            //Create first match
            MatchInfo info = new MatchInfo();
            info.timeStamp = new DateTime(2017, 1, 22, 15, 17, 0);
            info.map = "DM-Kitchen";
            info.gameMode = "DM";
            info.scoreboard = new PlayerScore[50];
            //Update with first match
            stats.Update(info);
            //Create second match
            info.timeStamp = new DateTime(2017, 1, 23, 15, 17, 0);
            info.map = "DM-Italy";
            info.gameMode = "DM";
            info.scoreboard = new PlayerScore[100];
            //Update with second match twice
            stats.Update(info);
            stats.Update(info);
            stats.UpdateTop();
            Assert.AreEqual(stats.totalMatchesPlayed, 3);
            Assert.AreEqual(stats.maximumMatchesPerDay, 2);
            Assert.AreEqual(stats.averageMatchesPerDay, 1.5);
            Assert.AreEqual(stats.maximumPopulation, 100);
            Assert.AreEqual(stats.averagePopulation, 250 / 3.0);
            Assert.AreEqual(stats.top5Maps.Count, 2);
            Assert.AreEqual(stats.top5Maps[0], "DM-Italy");
            Assert.AreEqual(stats.top5Maps[1], "DM-Kitchen");
            Assert.AreEqual(stats.top5GameModes.Count, 1);
            Assert.AreEqual(stats.top5GameModes[0], "DM");
        }

        [TestMethod]
        public void TestPlayerStatsUpdate()
        {
            Player stats = new Player();
            //Create first match
            MatchInfo info = new MatchInfo();
            info.timeStamp = new DateTime(2017, 1, 22, 15, 17, 0);
            info.endPoint = "10.1.1.1:80";
            info.map = "DM-Kitchen";
            info.gameMode = "DM";
            info.scoreboard = new PlayerScore[2];
            info.scoreboard[0] = new PlayerScore
            {
                kills = 15,
                deaths = 5
            };
            //Update with first match
            stats.Update(info, 0);
            //Create second match
            info.timeStamp = new DateTime(2017, 1, 22, 16, 17, 0);
            info.endPoint = "10.1.1.2:80";
            info.map = "TDM-Kitchen";
            info.gameMode = "TDM";
            info.scoreboard = new PlayerScore[2];
            info.scoreboard[1] = new PlayerScore
            {
                kills = 10,
                deaths = 12
            };
            //Update with second match twice
            stats.Update(info, 1);
            stats.Update(info, 1);
            Assert.AreEqual(stats.totalMatchesPlayed, 3);
            Assert.AreEqual(stats.totalMatchesWon, 1);
            Assert.AreEqual(stats.favoriteServer, "10.1.1.2:80");
            Assert.AreEqual(stats.uniqueServers, 2);
            Assert.AreEqual(stats.favoriteGameMode, "TDM");
            Assert.AreEqual(stats.averageScoreboardPercent, 100 / 3.0);
            Assert.AreEqual(stats.maximumMatchesPerDay, 3);
            Assert.AreEqual(stats.averageMatchesPerDay, 3);
            Assert.AreEqual(stats.lastMatchPlayed, "2017-01-22T16:17:00Z");
            Assert.AreEqual(stats.killToDeathRatio, 35 / 29.0 );
        }
    }
}
