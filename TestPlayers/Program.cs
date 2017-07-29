using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontur.GameStats.Server;
using LiteDB;
using NetTools;

namespace TestPlayers
{
    class Program
    {
        static void FillServers()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var servers = db.GetCollection<Server>("servers");
                List<Server> servs = new List<Server>();
                IPAddressRange temp = IPAddressRange.Parse("192.168.0.1-192.168.39.255");
                int count = 1;
                Random random = new Random();
                foreach (var ip in temp)
                {
                    servs.Add(new Server { endpoint = ip.ToString(), name = "Server"+count++,
                        stats = new ServerStats {averageMatchesPerDay = random.NextDouble() * 30 + 2} });
                }
                servers.Insert(servs);
                servers.EnsureIndex(x => x.endpoint);
                servers.EnsureIndex(x => x.stats.averageMatchesPerDay);
            }
        }

        static void FillPlayers()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var players = db.GetCollection<Player>("players");
                List<Player> plays = new List<Player>();
                Random random = new Random();
                for (int i = 1; i < 1000000; i++)
                {
                    plays.Add(new Player { nameLowerCase = "Player" + i, ignore = false, killToDeathRatio = random.NextDouble() * 20 + 1 });
                }
                players.Insert(plays);
                players.EnsureIndex(x => x.nameLowerCase);
                players.EnsureIndex(x => x.killToDeathRatio);
                players.EnsureIndex(x => x.ignore);
            }
        }

        static void FillMatches()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var matches = db.GetCollection<MatchInfo>("matches");
                List<MatchInfo> mcs = new List<MatchInfo>();
                Random random = new Random();
                var temp = IPAddressRange.Parse("192.168.0.1-192.168.39.255").ToList();
                for (int i = 1; i < 1000000; i++)
                {
                    MatchInfo mi = new MatchInfo
                    {
                        scoreboard = new PlayerScore[100],
                        timeStamp = DateTime.Today.AddSeconds(i),
                        endPoint = temp[random.Next(0, temp.Count - 1)].ToString()
                    };
                    for (int j = 0; i < 100; i++)
                        mi.scoreboard[j] = new PlayerScore {name = "SimpleName"};
                    mcs.Add(mi);
                    if (i == 500000)
                    {
                        matches.Insert(mcs);
                        mcs.Clear();
                    }
                }
                matches.Insert(mcs);
                matches.EnsureIndex(x => x.endPoint);
                matches.EnsureIndex(x => x.timeStamp);
            }
        }

        static void Get()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var players = db.GetCollection<Player>("players");
                Player player = players.FindOne(Query.EQ("nameLowerCase", "Player500000"));
                Console.WriteLine(player.nameLowerCase);
            }
        }

        static void InsertOne()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var players = db.GetCollection<Player>("players");
                var plays = new List<Player>();
                for (int i = 0; i < 100; i++)
                {
                    plays.Add(new Player { nameLowerCase = "Testo" + i });
                    //players.Insert(new Player { name = "Test"+i });
                }
                players.Insert(plays);
                players.EnsureIndex(x => x.nameLowerCase);
            }
        }
        static void UpdateOne()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                int count =0;
                var players = db.GetCollection<Player>("players");
                for (int i = 50000; i < 50100; i++)
                {
                    Player player = players.FindOne(Query.EQ("nameLowerCase", "Player"+i));
                    player.totalMatchesPlayed++;
                    player.ignore = false;
                    Console.WriteLine(player.totalMatchesPlayed);
                    players.Update(player);
                    count++;
                }
                players.EnsureIndex(x => x.nameLowerCase);
                players.EnsureIndex(x => x.killToDeathRatio);
                players.EnsureIndex(x => x.ignore);
                Console.WriteLine(count);
            }
        }

        static void getBest()
        {
            using (var db = new LiteDatabase("DataTest.db"))
            {
                var players = db.GetCollection<Player>("players");
                players.Find(Query.And(Query.All("killToDeathRatio", Query.Descending), Query.EQ("ignore", false)), 0, 50);
            }
        }
        static void Main(string[] args)
        {
            FillServers();
            Console.WriteLine("Servers done");
            FillPlayers();
            Console.WriteLine("Players done");
            //FillMatches();
            Console.WriteLine("Ready");
            Console.ReadKey();
        }
    }
}
