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
        #region private properties
        private string _fileSummonerName = "";
        private string _fileName
        {
            get
            {
                if(_fileSummonerName != "")
                {
                    return _fileSummonerName + "_iconsettings.lqm";
                }
                return "iconsettings.lqm";
            }
        }
        private string _settingsPath
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, _fileName);
            }
        }

        private Dictionary<int, int> _queueSummonericonPairs;

        #endregion

        #region public properties
        /// <summary>
        /// Provides a <see cref="IEnumerable{KeyValuePair}"/> to visualize the current settings.
        /// No changes can be made towards this property directly.
        /// To change the registered settings use the methods provided for <see cref="IconManager"/>.
        /// Key represents the queue id;
        /// Value represents the summoner icon id.
        /// </summary>
        public IEnumerable<KeyValuePair<int, int>> QueueSummonerIcons
        {
            get
            {
                return _queueSummonericonPairs.AsEnumerable();
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Constructor for <see cref="IconManager"/>.
        /// Reads and writes settings in a the file '/iconsettings.lqm'.
        /// </summary>
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

        public IconManager(string summonerName)
        {
            this._fileSummonerName = TinySummonerName(summonerName);

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

        /// <summary>
        /// Add a pair of <paramref name="queue"/> and <paramref name="summonerIcon"/> to the settings
        /// </summary>
        /// <param name="queue">Id of the queue</param>
        /// <param name="summonerIcon">Id of the summoner icon. 
        /// Use -1 to not change the summoner icon.
        /// Use -2 to not add the pair to settings.
        /// </param>
        public void AddPair(int queue, int summonerIcon)
        {
            if (summonerIcon == -2)
            {
                // -2 is default error answer
                // -1 stands for do not change icon
                return;
            }
            if (_queueSummonericonPairs.ContainsKey(queue))
            {
                _queueSummonericonPairs.Remove(queue);
            }
            _queueSummonericonPairs.Add(queue, summonerIcon);

            WriteSettingsToFile();
        }
        /// <summary>
        /// Get the summoner icon id for <paramref name="queue"/>.
        /// If <paramref name="queue"/> is not in current settings return the default icon id.
        /// </summary>
        /// <param name="queue">Id of the queue</param>
        /// <returns>summoner icon id or -1 to signal no change in summoner icon</returns>
        public int GetQueueValue(int queue)
        {
            int summonerIcon = -1;
            var success = _queueSummonericonPairs.TryGetValue(queue, out summonerIcon);
            if (!success)
            {
                _queueSummonericonPairs.TryGetValue(-1, out summonerIcon);
            }
            return summonerIcon;
        }
        /// <summary>
        /// Removes the specified <paramref name="queue"/> from the current settings.
        /// -1 as default queue can not be removed.
        /// </summary>
        /// <param name="queue">Id of the queue</param>
        /// <returns>true when removed successfully, false otherwise</returns>
        public bool RemoveQueue(int queue)
        {
            if(queue == -1)
            {
                return false;
            }

            var ret = _queueSummonericonPairs.Remove(queue);

            WriteSettingsToFile();

            return ret;
        }
        #endregion

        #region private methods
        #region file interactions
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
        #endregion

        private string TinySummonerName(string summonerName)
        {
            string[] replaceChars = new string[] { " ", "\\" };

            var ret = summonerName;

            foreach(var c in replaceChars)
            {
                ret = ret.Replace(c, "");
            }

            return ret;
        }
        #endregion


    }
}
