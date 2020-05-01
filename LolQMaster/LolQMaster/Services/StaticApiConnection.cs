using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiotSharp;
using RiotSharp.Caching;
using RiotSharp.Endpoints.StaticDataEndpoint;

namespace LolQMaster.Services
{
    public static class StaticApiConnection
    {
        private static ICache _cache = new Cache();

        public static StaticDataEndpoints StaticDataEndpoints
        {
            get
            {
                if (_staticDataEndpoints == null)
                {
                    var req = new RiotSharp.Http.Requester();
                    _staticDataEndpoints = new StaticDataEndpoints(req, _cache);
                }
                return _staticDataEndpoints;
            }
        }


        private static StaticDataEndpoints _staticDataEndpoints;

        public static string GetSummonerIconImageUrl(int iconId)
        {

            var curversion = StaticDataEndpoints.Versions.GetAllAsync().Result.First();

            if(iconId == -1)
            {
                return String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/passive/ZacPassive.png", curversion);
            }

            // Schema: http://ddragon.leagueoflegends.com/cdn/10.9.1/img/profileicon/685.png

            return String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/profileicon/{1}.png", curversion, iconId.ToString());
        }

        public static string GetQueueName(int queueId)
        {
            var curversion = StaticDataEndpoints.Versions.GetAllAsync().Result.First();

            string queueName;
            var success = _queueDict.TryGetValue(queueId, out queueName);
            if (!success)
            {
                return "Unknown Queue";
            }
            return queueName;
        }

        public static IEnumerable<int> AvailableQueues => _queueDict.Keys;

        private static Dictionary<int, string> _queueDict = new Dictionary<int, string>() {
            {-1,"Default" },
            {0, "Custom games" },
            {325, "All random on Summoners Rift" },
            {400, "5v5 Draft Pick" },
            {420, "5v5 Solo/Duo" },
            {430, "5v5 Blind Normal" },
            { 440, "5v5 Ranked Flex" },
            { 450, "ARAM" },
            {700,"Clash" },
            {900,"URF" },
            {920,"Legend of the Poro King" },
            { 1010, "Snow ARURF" },
            {1020,"One for All" }
        };
    }
}
