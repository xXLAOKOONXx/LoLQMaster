using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LolQMaster.Services
{
    public class IconManager
    {
        private static string _fileName
        {
            get
            {
                return "iconsettings.lqm";
            }
        }
        private static string _settingsPath
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, _fileName);
            }
        }

        public IEnumerable<KeyValuePair<int, int>> QueueSummonerIcons
        {
            get
            {
                return _queueSummonericonPairs.AsEnumerable();
            }
        }

        private Dictionary<int, int> _queueSummonericonPairs;

        public IconManager()
        {
            try
            {
                ReadSettingsFromFile();
            }
            catch (Exception ex)
            {
                _queueSummonericonPairs = new Dictionary<int, int>();
                _queueSummonericonPairs.Add(-1, -1);
            }

        }

        private void ReadSettingsFromFile()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _queueSummonericonPairs = (Dictionary<int, int>)formatter.Deserialize(stream);
            stream.Close();
        }

        private void WriteSettingsToFile()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_settingsPath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, _queueSummonericonPairs);
            stream.Close();
        }

        public void AddPair(int queue, int summonerIcon)
        {
            if (_queueSummonericonPairs.ContainsKey(queue))
            {
                _queueSummonericonPairs.Remove(queue);
            }
            _queueSummonericonPairs.Add(queue, summonerIcon);

            WriteSettingsToFile();
        }

        public int GetQueueValue(int queue)
        {
            int summonerIcon;
            var success = _queueSummonericonPairs.TryGetValue(queue, out summonerIcon);
            if (!success)
            {
                return -1;
            }
            return summonerIcon;
        }

        public bool RemoveQueue(int queue)
        {
            return _queueSummonericonPairs.Remove(queue);

            WriteSettingsToFile();
        }
    }
}
