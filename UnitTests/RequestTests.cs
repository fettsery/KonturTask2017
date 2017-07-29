using System;
using System.Net;
using Kontur.GameStats.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class RequestTests
    {
        private static RequestHandler handler;
        public TestContext TestContext { get; set; }
        const string addServerData = @"{
                ""name"": ""] My P3rfect Server ["",
	            ""gameModes"": [ ""DM"", ""TDM"" ]
                }";
        const string addMatchData = @"{
	            ""map"": ""DM-HelloWorld"",
	            ""gameMode"": ""DM"",
	            ""fragLimit"": 20,
	            ""timeLimit"": 20,
	            ""timeElapsed"": 12.345678,
	            ""scoreboard"": [
		            {
			            ""name"": ""Player1"",
			            ""frags"": 20,
			            ""kills"": 21,
			            ""deaths"": 3
		            },
		            {
			            ""name"": ""Player2"",
			            ""frags"": 2,
			            ""kills"": 2,
			            ""deaths"": 21
		            }
            ]
            }";
        const string endPoint = "10.1.1.1-8080";

        [TestInitialize]
        public void Initialize()
        {
            handler = new RequestHandler($"{TestContext.TestName}.db");
        }

        [TestCleanup]
        public void CleanUp()
        {
            handler.Dispose();
            System.IO.File.Delete($"{TestContext.TestName}.db");
        }

        [TestMethod]
        public void TestAddServer()
        {
            string response;
            var res = handler.AddServer(endPoint, addServerData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            res = handler.GetServer(endPoint, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(addServerData), JObject.Parse(response)));
        }

        [TestMethod]
        public void TestAddMatch()
        {
            string response;
            var res = handler.AddServer(endPoint, addServerData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            DateTime date = new DateTime(2017, 1, 22, 15, 17, 0);
            res = handler.AddMatch(endPoint, date, addMatchData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            res = handler.GetMatch(endPoint, date, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(addMatchData), JObject.Parse(response)));
        }

        [TestMethod]
        public void TestServerStats()
        {
            string response;
            var res = handler.AddServer(endPoint, addServerData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            DateTime date = new DateTime(2017, 1, 22, 15, 17, 0);
            res = handler.AddMatch(endPoint, date, addMatchData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            string jsonOutput = @"{
	            ""totalMatchesPlayed"": 1,
	            ""maximumMatchesPerDay"": 1,
	            ""averageMatchesPerDay"": 1.0,
	            ""maximumPopulation"": 2,
	            ""averagePopulation"": 2.0,
	            ""top5GameModes"": [ ""DM"" ],
	            ""top5Maps"": [
		            ""DM-HelloWorld""
            ]
            }";

            res = handler.GetServerStats(endPoint, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(jsonOutput), JObject.Parse(response)));
        }

        [TestMethod]
        public void TestPlayerStats()
        {
            string response;
            var res = handler.AddServer(endPoint, addServerData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            DateTime date = new DateTime(2017, 1, 22, 15, 17, 0);
            res = handler.AddMatch(endPoint, date, addMatchData, out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);

            string jsonOutput = @"{
	            ""totalMatchesPlayed"": 1,
	            ""totalMatchesWon"": 1,
	            ""favoriteServer"": ""10.1.1.1-8080"",
	            ""uniqueServers"": 1,
	            ""favoriteGameMode"": ""DM"",
	            ""averageScoreboardPercent"": 100.0,
	            ""maximumMatchesPerDay"": 1,
	            ""averageMatchesPerDay"": 1.0,
	            ""lastMatchPlayed"": ""2017-01-22T15:17:00Z"",
	            ""killToDeathRatio"": 7.0
            }";

            res = handler.GetPlayerStats("Player1", out response);
            Assert.AreEqual(res, (int) HttpStatusCode.OK);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(jsonOutput), JObject.Parse(response)));
        }
    }
}
