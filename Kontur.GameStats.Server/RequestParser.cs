using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kontur.GameStats.Server
{
    public class RequestParser
    {
        public static bool CheckServerCommand(string rawurl, out string endPoint)
        {
            var result = ServersRegex.Match(rawurl);
            endPoint = result.Value;
            return result.Success;
        }

        public static bool CheckAllServersCommand(string rawurl)
        {
            return rawurl == GetAllServers;
        }

        public static bool CheckGetRecentMatchesCommand(string rawurl, out int count)
        {
            count = -1;
            if (!rawurl.StartsWith(GetRecentMatches)) return false;
            if (rawurl == GetRecentMatches)
                count = defaultCount;
            else if (rawurl.StartsWith(GetRecentMatches + @"/"))
            {
                if (!int.TryParse(rawurl.Replace(GetRecentMatches + @"/", ""), out count))
                {
                    return false;
                }
                if (count > defaultMaxCount) count = defaultMaxCount;
                if (count < defaultMinCount) count = defaultMinCount;
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool CheckMatchCommand(string rawurl, out string endPoint, out DateTime dateTime)
        {
            var result = MatchEndpointRegex.Match(rawurl);
            if (result.Success)
            {
                string timeStamp = MatchTimestampRegex.Match(rawurl).Value;
                timeStamp = timeStamp.Replace("T", "").Replace("Z", "");
                if (DateTime.TryParseExact(timeStamp, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                {
                    endPoint = result.Value;
                    return true;
                }
            }
            endPoint = "";
            dateTime = DateTime.MinValue;
            return false;
        }

        public static bool CheckGetServerStatsCommand(string rawurl, out string endPoint)
        {
            var result = GetStatsRegex.Match(rawurl);
            endPoint = result.Value;
            return result.Success;
        }

        public static bool CheckGetPlayerStatsCommand(string rawurl, out string name)
        {
            var result = GetPlayerRegex.Match(rawurl);
            name = result.Value;
            return result.Success;
        }

        public static bool CheckGetBestPlayersCommand(string rawurl, out int count)
        {
            count = -1;
            if (!rawurl.StartsWith(GetBestPlayers)) return false;
            if (rawurl == GetBestPlayers)
                count = defaultCount;
            else if (rawurl.StartsWith(GetBestPlayers + @"/"))
            {
                if (!int.TryParse(rawurl.Replace(GetBestPlayers + @"/", ""), out count))
                {
                    return false;
                }
                if (count > defaultMaxCount) count = defaultMaxCount;
                if (count < defaultMinCount) count = defaultMinCount;
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool CheckGetPopularServersCommand(string rawurl, out int count)
        {
            count = -1;
            if (!rawurl.StartsWith(GetPopularServers)) return false;
            if (rawurl == GetPopularServers)
                count = defaultCount;
            else if (rawurl.StartsWith(GetPopularServers + @"/"))
            {
                if (!int.TryParse(rawurl.Replace(GetPopularServers + @"/", ""), out count))
                {
                    return false;
                }
                if (count > defaultMaxCount) count = defaultMaxCount;
                if (count < defaultMinCount) count = defaultMinCount;
            }
            else
            {
                return false;
            }
            return true;
        }

        private const int defaultCount = 5;
        private const int defaultMaxCount = 50;
        private const int defaultMinCount = 0;

        private const string ValidIpAddressRegex = @"((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]))";

        private const string ValidHostnameRegex = @"((([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9]))";

        private const string ValidPortRegex = @"(?:6553[0-5]|655[0-2][0-9]|65[0-4][0-9]{2}|6[0-4][0-9]{3}|[1-5][0-9]{4}|[1-9][0-9]{1,3}|[0-9])";

        private static readonly Regex ServersRegex = new Regex(@"(?<=^/servers/)(" + ValidIpAddressRegex + "|" + ValidHostnameRegex + ")-" + ValidPortRegex + "(?=/info$)");

        private const string GetAllServers = @"/servers/info";

        private const string GetRecentMatches = @"/reports/recent-matches";

        private static readonly Regex MatchEndpointRegex = new Regex(@"(?<=^/servers/)(" + ValidIpAddressRegex + "|" + ValidHostnameRegex + ")-" + ValidPortRegex + 
            "(?=/matches/.+$)");

        private static readonly Regex MatchTimestampRegex = new Regex(@"(?<=/matches/)\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z");

        private static readonly Regex GetStatsRegex = new Regex(@"(?<=^/servers/)(" + ValidIpAddressRegex + "|" + ValidHostnameRegex + ")-" + ValidPortRegex + "(?=/stats$)");

        private static readonly Regex GetPlayerRegex = new Regex(@"(?<=^/players/).+(?=/stats$)");

        private const string GetBestPlayers = @"/reports/best-players";

        private const string GetPopularServers = @"/reports/popular-servers";
    }
}
