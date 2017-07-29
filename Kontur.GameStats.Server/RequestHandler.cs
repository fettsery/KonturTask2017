using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Newtonsoft.Json;
using System.Net;

namespace Kontur.GameStats.Server
{
    public  class RequestHandler : IDisposable
    {
        public RequestHandler(string name)
        {
            db = new LiteDatabase(name);
        }
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            db.Dispose();
        }
        public int AddServer(string endPoint, string query, out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            Server server;
            if (servers.Exists(Query.EQ("endpoint", endPoint)))
            {
                server = servers.FindOne(Query.EQ("endpoint", endPoint));
                JsonConvert.PopulateObject(query, server, jsonSettings);
                servers.Update(server);
            }
            else
            {
                server = JsonConvert.DeserializeObject<Server>(query, jsonSettings);
                server.endpoint = endPoint;
                server.stats = new ServerStats();
                servers.Insert(server);
            }
            servers.EnsureIndex(x => x.endpoint);
            servers.EnsureIndex(x => x.stats.averageMatchesPerDay);

            response = "";
            return (int)HttpStatusCode.OK;
        }

        public int GetServer(string endPoint, out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            Server server = servers.FindOne(Query.EQ("endpoint", endPoint));
            if (server == null)
            {
                response = "";
                return (int) HttpStatusCode.NotFound;
            }
            response = JsonConvert.SerializeObject(server, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int GetAllServers(out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            var serversList = servers.FindAll();
            List<ServerWithInfo> serversInfos = serversList.Select(server => new ServerWithInfo(server)).ToList();
            response = JsonConvert.SerializeObject(serversInfos, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int AddMatch(string endPoint, DateTime timeStamp, string query, out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            if (servers.Exists(Query.EQ("endpoint", endPoint)))
            {
                var matches = db.GetCollection<MatchInfo>("matches");
                MatchInfo match = JsonConvert.DeserializeObject<MatchInfo>(query, jsonSettings);
                match.endPoint = endPoint;
                match.timeStamp = timeStamp;

                using (var trans = db.BeginTrans())
                {
                    Server server = servers.FindOne(Query.EQ("endpoint", endPoint));
                    server.stats.Update(match);
                    servers.Update(server);
                    matches.Insert(match);
                    AddPlayersStats(match);
                    trans.Commit();
                }
                matches.EnsureIndex(x => x.endPoint);
                matches.EnsureIndex(x => x.timeStamp);
                servers.EnsureIndex(x => x.stats.averageMatchesPerDay);
                response = "";
                return (int)HttpStatusCode.OK;
            }
            response = "";
            return (int) HttpStatusCode.BadRequest;
        }

        public int GetMatch(string endPoint, DateTime timeStamp, out string response)
        {
            var matches = db.GetCollection<MatchInfo>("matches");
            MatchInfo match =
                matches.FindOne(Query.And(Query.EQ("endPoint", endPoint), Query.EQ("timeStamp", timeStamp)));
            if (match == null)
            {
                response = "";
                return (int)HttpStatusCode.NotFound;
            }
            response = JsonConvert.SerializeObject(match, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int GetRecentMatches(int count, out string response)
        {
            var matches = db.GetCollection<MatchInfo>("matches");
            var matchesInfo = matches.Find(Query.All("timeStamp", Query.Descending), 0, count);
            List<MatchReport> reports = matchesInfo.Select(match => new MatchReport(match)).ToList();
            response = JsonConvert.SerializeObject(reports, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int GetServerStats(string endPoint, out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            Server server = servers.FindOne(Query.EQ("endpoint", endPoint));
            if (server == null)
            {
                response = "";
                return (int)HttpStatusCode.NotFound;
            }
            server.stats.UpdateTop();
            response = JsonConvert.SerializeObject(server.stats, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public void AddPlayersStats(MatchInfo match)
        {
            var players = db.GetCollection<Player>("players");
            int index = 0;
            foreach (var playerScore in match.scoreboard)
            {
                Player player = players.FindOne(Query.EQ("nameLowerCase", playerScore.name.ToLower()));
                if (player == null)
                {
                    player = new Player {nameLowerCase = playerScore.name.ToLower(), name = playerScore.name};
                    player.Update(match, index++);
                    players.Insert(player);
                }
                else
                {
                    player.name = playerScore.name;
                    player.Update(match, index++);
                    players.Update(player);
                }
            }
            players.EnsureIndex(x => x.nameLowerCase);
            players.EnsureIndex(x => x.killToDeathRatio);
            players.EnsureIndex(x => x.ignore);
        }

        public int GetPlayerStats(string name, out string response)
        {
            var players = db.GetCollection<Player>("players");
            Player player = players.FindOne(Query.EQ("nameLowerCase", name.ToLower()));
            if (player == null)
            {
                response = "";
                return (int)HttpStatusCode.NotFound;
            }
            response = JsonConvert.SerializeObject(player, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int GetBestPlayers(int count, out string response)
        {
            var players = db.GetCollection<Player>("players");
            var bestPlayers =
                players.Find(Query.All("killToDeathRatio", Query.Descending)).Where(x => x.ignore == false).Take(count);
            List<PlayerShortInfo> bestPlayersShortInfos = bestPlayers.Select(player => new PlayerShortInfo(player)).ToList();      
            response = JsonConvert.SerializeObject(bestPlayersShortInfos, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        public int GetPopularServers(int count, out string response)
        {
            var servers = db.GetCollection<Server>("servers");
            var popularServers = servers.Find(Query.All("stats.averageMatchesPerDay", Query.Descending), 0, count);
            List<ServerReport> popularServerShortInfos = popularServers.Select(server => new ServerReport(server)).ToList();
            response = JsonConvert.SerializeObject(popularServerShortInfos, Formatting.Indented);
            return (int)HttpStatusCode.OK;
        }

        private readonly LiteDatabase db;
        private bool disposed;
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error };
    }
}
