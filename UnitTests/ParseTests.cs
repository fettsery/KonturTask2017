using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kontur.GameStats.Server;

namespace Tests
{
    [TestClass]
    public class ParserTests
    {
        private string _endPoint;
        private string _rawUrl;
        private string _name;
        private int _count;
        private DateTime _date;

        [TestMethod]
        public void TestGetServerCorrectIp()
        {
            _rawUrl = "/servers/10.0.0.0-8080/info";
            var res = RequestParser.CheckServerCommand(_rawUrl, out _endPoint);
            Assert.IsTrue(res);
            Assert.AreEqual(_endPoint, "10.0.0.0-8080");
        }
        [TestMethod]
        public void TestGetServerCorrectDomain()
        {
            _rawUrl = "/servers/starcraftserver.com-444/info";
            var res = RequestParser.CheckServerCommand(_rawUrl, out _endPoint);
            Assert.IsTrue(res);
            Assert.AreEqual(_endPoint, "starcraftserver.com-444");
        }
        [TestMethod]
        public void TestGetServerIncorrect()
        {
            _rawUrl = "/servers/trash/info";
            var res = RequestParser.CheckServerCommand(_rawUrl, out _endPoint);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetAllServersCorrect()
        {
            _rawUrl = "/servers/info";
            var res = RequestParser.CheckAllServersCommand(_rawUrl);
            Assert.IsTrue(res);
        }
        [TestMethod]
        public void TestGetAllServersIncorrect()
        {
            _rawUrl = "/trash";
            var res = RequestParser.CheckAllServersCommand(_rawUrl);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetRecentMatchesCorrect()
        {
            _rawUrl = "/reports/recent-matches/15";
            var res = RequestParser.CheckGetRecentMatchesCommand(_rawUrl, out _count);
            Assert.IsTrue(res);
            Assert.AreEqual(_count, 15);
        }
        [TestMethod]
        public void TestGetRecentMatchesIncorrect()
        {
            _rawUrl = "/reports/recent-matches/trash";
            var res = RequestParser.CheckGetRecentMatchesCommand(_rawUrl, out _count);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestMatchCorrect()
        {
            _rawUrl = "/servers/121.12.242.13-80/matches/2017-01-22T15:17:00Z";
            var res = RequestParser.CheckMatchCommand(_rawUrl, out _endPoint, out _date);
            Assert.IsTrue(res);
            Assert.AreEqual(_endPoint, "121.12.242.13-80");
            Assert.AreEqual(_date, new DateTime(2017, 1, 22, 15, 17, 0));
        }
        [TestMethod]
        public void TestMatchIncorrect()
        {
            _rawUrl = "/servers/121.12.242.13-80/matches/2017-1-22T15:17:00";
            var res = RequestParser.CheckMatchCommand(_rawUrl, out _endPoint, out _date);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetServerStatsCorrect()
        {
            _rawUrl = "/servers/121.12.242.13-80/stats";
            var res = RequestParser.CheckGetServerStatsCommand(_rawUrl, out _endPoint);
            Assert.IsTrue(res);
            Assert.AreEqual(_endPoint, "121.12.242.13-80");
        }
        [TestMethod]
        public void TestGetServerStatsIncorrect()
        {
            _rawUrl = "/servers/121.12.242.13-80/stat";
            var res = RequestParser.CheckGetServerStatsCommand(_rawUrl, out _endPoint);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetPlayerStatsCorrect()
        {
            _rawUrl = "/players/James/stats";
            var res = RequestParser.CheckGetPlayerStatsCommand(_rawUrl, out _name);
            Assert.IsTrue(res);
            Assert.AreEqual(_name, "James");
        }
        [TestMethod]
        public void TestGetPlayerStatIncorrect()
        {
            _rawUrl = "/player/James/stats";
            var res = RequestParser.CheckGetPlayerStatsCommand(_rawUrl, out _name);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetBestPlayersCorrect()
        {
            _rawUrl = "/reports/best-players/1000";
            var res = RequestParser.CheckGetBestPlayersCommand(_rawUrl, out _count);
            Assert.IsTrue(res);
            Assert.AreEqual(_count, 50);
        }
        [TestMethod]
        public void TestGetBestPlayersInCorrect()
        {
            _rawUrl = "/reports/bestplayers/1000";
            var res = RequestParser.CheckGetBestPlayersCommand(_rawUrl, out _count);
            Assert.IsFalse(res);
        }
        [TestMethod]
        public void TestGetPopularServersCorrect()
        {
            _rawUrl = "/reports/popular-servers/-100";
            var res = RequestParser.CheckGetPopularServersCommand(_rawUrl, out _count);
            Assert.IsTrue(res);
            Assert.AreEqual(_count, 0);
        }
        [TestMethod]
        public void TestGetPopularServersIncorrect()
        {
            _rawUrl = "/reports/trash/1000";
            var res = RequestParser.CheckGetPopularServersCommand(_rawUrl, out _count);
            Assert.IsFalse(res);
        }
    }
}
