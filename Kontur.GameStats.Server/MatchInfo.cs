using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class MatchInfo
    {
        [JsonIgnore]
        public int id { get; set; }
        [JsonIgnore]
        public string endPoint { get; set; }
        [JsonIgnore]
        public DateTime timeStamp { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string map { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string gameMode { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int fragLimit { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int timeLimit { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double timeElapsed { get; set; }
        [JsonProperty(Required = Required.Always)]
        public PlayerScore[] scoreboard { get; set; }
    }

    public class MatchReport
    {
        public MatchReport(MatchInfo info)
        {
            server = info.endPoint;
            timestamp = info.timeStamp.ToString("yyyy-MM-dd") + "T" + info.timeStamp.ToString("HH:mm:ss") + "Z";
            results = info;
        }
        public string server { get; set; }
        public string timestamp { get; set; }
        public MatchInfo results { get; set; }
    }
    public class PlayerScore
    {
        [JsonProperty(Required = Required.Always)]
        public string name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int frags { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int kills { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int deaths { get; set; }
    }
}
