using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RiotSharp;
using RiotSharp.Caching;
using RiotSharp.Endpoints.StaticDataEndpoint;

namespace LolQMaster.Services
{
    public static class StaticApiConnection
    {
        #region RiotSharp

        private static ICache _cache = new Cache();
        private static StaticDataEndpoints _staticDataEndpoints;

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

        #endregion


        /// <summary>
        /// Provides an imgurl for <paramref name="iconId"/>
        /// </summary>
        /// <param name="iconId">Id of summoner icon</param>
        /// <returns>imgurl of summoner id. For iconid "-1" returns Zac passive</returns>
        public static string GetSummonerIconImageUrl(int iconId)
        {
            while (!File.Exists(LocalIconLocation(iconId)))
            {
                Task.Run(()=>CopyToLocal(iconId));
            }
            return LocalIconLocation(iconId);
        }

        /// <summary>
        /// Provides the name of a <paramref name="queueId"/> based on a static manually maintained list
        /// </summary>
        /// <param name="queueId">Id of the queue</param>
        /// <returns>Name of Queue</returns>
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

        /// <summary>
        /// Provides an <see cref="IEnumerable{int}"/> with all queue ids.
        /// This list is maintained manually in the code behind.
        /// </summary>
        public static IEnumerable<int> AvailableQueues => _queueDict.Keys;

        private static string IconFolder
        {
            get
            {
                var iconfolderpath = Path.Combine(AppContext.BaseDirectory, "icons");
                if (!Directory.Exists(iconfolderpath))
                {
                    Directory.CreateDirectory(iconfolderpath);
                }
                return iconfolderpath;
            }
        }
        private static void CopyToLocal(int icon)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(WebUrl(icon),LocalIconLocation(icon));
            }
        }

        private static string WebUrl(int iconId)
        {
            var curversion = StaticDataEndpoints.Versions.GetAllAsync().Result.First();

            if (iconId == -1)
            {
                return String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/passive/ZacPassive.png", curversion);
            }

            // Schema: http://ddragon.leagueoflegends.com/cdn/10.9.1/img/profileicon/685.png

            return String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/profileicon/{1}.png", curversion, iconId.ToString());
        }
        private static string LocalIconLocation(int icon)
        {
            return Path.Combine(IconFolder, icon + ".png");
        }

        private static Dictionary<int, string> _queueDict = new Dictionary<int, string>() {
            {-1,"Default" },
            {0, "Custom games" },
            {325, "All random on Summoners Rift" },
            {400, "5v5 Draft Pick" },
            {420, "5v5 Solo/Duo" },
            {430, "5v5 Blind Normal" },
            {440, "5v5 Ranked Flex" },
            {450, "ARAM" },
            {700,"Clash" },
            {900,"URF" },
            {920,"Legend of the Poro King" },
            {1010, "Snow ARURF" },
            {1020,"One for All" }
        };
    }
}
