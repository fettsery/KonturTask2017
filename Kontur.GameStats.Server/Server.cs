using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class Server
    {
        [JsonIgnore]
        public int id { get; set; }
        [JsonIgnore]
        public string endpoint { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string[] gameModes { get; set; }
        [JsonIgnore]
        public ServerStats stats { get; set; }
    }

    public class ServerWithInfo
    {
        public class Info
        {
            public string name { get; set; }
            public string[] gameModes { get; set; }
        }
        public ServerWithInfo(Server server)
        {
            endpoint = server.endpoint;
            info = new Info
            {
                name = server.name,
                gameModes = server.gameModes
            };
        }
        public string endpoint { get; set; }
        public Info info { get; set; }
    }

    public class ServerReport
    {
        public ServerReport(Server server)
        {
            endpoint = server.endpoint;
            name = server.name;
            averageMatchesPerDay = server.stats.averageMatchesPerDay;
        }
        public string endpoint { get; set; }
        public string name { get; set; }
        public double averageMatchesPerDay { get; set; }
    }

    public class ServerStats
    {
        public void Update(MatchInfo match)
        {
            totalMatchesPlayed++;

            DateTime day = match.timeStamp.Date;
            if (dayMatches.ContainsKey(day))
                dayMatches[day]++;
            else
                dayMatches[day] = 1;

            maximumMatchesPerDay = dayMatches.Values.Max();
            averageMatchesPerDay = dayMatches.Values.Average();

            sumPopulation += match.scoreboard.Length;
            averagePopulation = (sumPopulation * 1.0) / totalMatchesPlayed;
            if (match.scoreboard.Length > maximumPopulation)
                maximumPopulation = match.scoreboard.Length;

            if (gameModeMatches.ContainsKey(match.gameMode))
                gameModeMatches[match.gameMode]++;
            else
                gameModeMatches[match.gameMode] = 1;

            if (mapMatches.ContainsKey(match.map))
                mapMatches[match.map]++;
            else
                mapMatches[match.map] = 1;
        }

        public void UpdateTop()
        {
            top5GameModes = gameModeMatches.OrderByDescending(item => item.Value).Select(item => item.Key).Take(5).ToList();
            top5Maps = mapMatches.OrderByDescending(item => item.Value).Select(item => item.Key).Take(5).ToList();
        }


        public long totalMatchesPlayed { get; set; }
        public int maximumMatchesPerDay { get; set; }
        public double averageMatchesPerDay { get; set; }
        public int maximumPopulation { get; set; }
        public double averagePopulation { get; set; }
        [BsonIgnore]
        public List<string> top5GameModes { get; set; }
        [BsonIgnore]
        public List<string> top5Maps { get; set; }

        [JsonIgnore]
        public long sumPopulation { get; set; }

        [JsonIgnore]
        public Dictionary<DateTime, int> dayMatches { get; set; } = new Dictionary<DateTime, int>();

        [JsonIgnore]
        public Dictionary<string, int> gameModeMatches { get; set; } = new Dictionary<string, int>();

        [JsonIgnore]
        public Dictionary<string, int> mapMatches { get; set; } = new Dictionary<string, int>();
    }

}
