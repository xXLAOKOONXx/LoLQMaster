using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolQMaster.Models
{
    public class IconFilters
    {
        public IconFilters()
        {

        }

        public IEnumerable<IconFilter> Filters { get => filters; }
        private List<IconFilter> filters = new List<IconFilter>();

        public void AddFilter(IconFilter iconFilter)
        {
            filters.Add(iconFilter);
        }

        public class IconFilter
        {
            public IconFilter(string name, IEnumerable<int> icons)
            {
                FilterName = name;
                Icons = icons;
            }

            public string FilterName;
            public IEnumerable<int> Icons;
        }
    }
}
