using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class Player
    {
        public void Update(MatchInfo match, int index)
        {
            totalMatchesPlayed++;

            if (index == 0)
                totalMatchesWon++;

            if (serverMatches.ContainsKey(match.endPoint))
                serverMatches[match.endPoint]++;
            else
                serverMatches[match.endPoint] = 1;
            favoriteServer = serverMatches.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            uniqueServers = serverMatches.Count;

            if (gameModeMatches.ContainsKey(match.gameMode))
                gameModeMatches[match.gameMode]++;
            else
                gameModeMatches[match.gameMode] = 1;
            favoriteGameMode = gameModeMatches.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

            sumScoreboardPercent += (match.scoreboard.Length - index - 1) * 1.0 / (match.scoreboard.Length - 1) * 100;
            averageScoreboardPercent = sumScoreboardPercent / totalMatchesPlayed;

            DateTime day = match.timeStamp.Date;
            if (dayMatches.ContainsKey(day))
                dayMatches[day]++;
            else
                dayMatches[day] = 1;
            maximumMatchesPerDay = dayMatches.Values.Max();
            averageMatchesPerDay = dayMatches.Values.Average();

            if (lastMatchPlayed == null || lastMatchPlayedDate < match.timeStamp)
            {
                lastMatchPlayedDate = match.timeStamp;
                lastMatchPlayed = match.timeStamp.ToString("yyyy-MM-dd") + "T" + match.timeStamp.ToString("HH:mm:ss") + "Z";
            }

            totalKills += match.scoreboard[index].kills;
            totalDeaths += match.scoreboard[index].deaths;
            if (totalDeaths != 0)
            {
                killToDeathRatio = totalKills * 1.0 / totalDeaths;
                if (totalMatchesPlayed >= 10)
                    ignore = false;
            }

        }

        [JsonIgnore]
        public int id { get; set; }
        [JsonIgnore]
        public string nameLowerCase { get; set; }
        [JsonIgnore]
        public string name { get; set; }
        public long totalMatchesPlayed { get; set; }
        public long totalMatchesWon { get; set; }
        public string favoriteServer { get; set; }
        public int uniqueServers { get; set; }
        public string favoriteGameMode { get; set; }
        public double averageScoreboardPercent { get; set; }
        public int maximumMatchesPerDay { get; set; }
        public double averageMatchesPerDay { get; set; }
        public string lastMatchPlayed { get; set; }
        public double killToDeathRatio { get; set; }

        [JsonIgnore]
        public int totalKills { get; set; }
        [JsonIgnore]
        public int totalDeaths { get; set; }
        [JsonIgnore]
        public bool ignore { get; set; } = true;
        [JsonIgnore]
        public DateTime lastMatchPlayedDate { get; set; }
        [JsonIgnore]
        public double sumScoreboardPercent { get; set; }
        [JsonIgnore]
        public Dictionary<string, int> serverMatches { get; set; } = new Dictionary<string, int>();
        [JsonIgnore]
        public Dictionary<string, int> gameModeMatches { get; set; } = new Dictionary<string, int>();
        [JsonIgnore]
        public Dictionary<DateTime, int> dayMatches { get; set; } = new Dictionary<DateTime, int>();
    }

    public class PlayerShortInfo
    {
        public PlayerShortInfo(Player player)
        {
            name = player.name;
            killToDeathRatio = player.killToDeathRatio;
        }
        public string name { get; set; }
        public double killToDeathRatio { get; set; }
    }
}
