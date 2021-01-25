using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LolQMaster.Models;
using LolQMaster.Services;
using Newtonsoft.Json.Linq;

namespace LolQMaster.Windows
{
    /// <summary>
    /// Interaktionslogik für IconPicker.xaml
    /// </summary>
    public partial class IconPicker : Window
    {
        private const string allIconsFilter = "all";
        private bool _manualClose = true;
        private LCUConnection _lCUConnection;
        private Action<int> _actionOnIconSelected;
        private IconFilters iconFilters;

        public IconPicker(LCUConnection lCUConnection, Action<int> actionOnIconSelected)
        {
            InitializeComponent();

            _lCUConnection = lCUConnection;
            _actionOnIconSelected = actionOnIconSelected;

            this.Closing += OnWindowClosing;

            GetIconFilters();

            DrawIcons(allIconsFilter);

            InitFilterButtons();

        }
        private void InitFilterButtons()
        {
            AddFilterButton(allIconsFilter);

            foreach (var filter in iconFilters.Filters)
            {
                AddFilterButton(filter.FilterName);
            }
        }

        private void AddFilterButton(string filtername)
        {
            var button = new Button();

            button.Content = filtername;

            button.Click += (o, s) => { DrawIcons(filtername); };

            this.FilterRow.Children.Add(button);

            button.Width = 100;
        }

        private void DrawIcons(string filterName)
        {
            this.ContentPanel.Children.Clear();

            if (filterName == allIconsFilter)
            {
                foreach (var item in _lCUConnection.OwnedIcons())
                {
                    this.ContentPanel.Children.Add(ClickableImage(item));
                }
                return;
            }
            try
            {
                var filter = this.iconFilters.Filters.Where(x => x.FilterName == filterName).First();

                foreach (var item in _lCUConnection.OwnedIcons())
                {
                    if (filter.Icons.Contains(item))
                    {
                        this.ContentPanel.Children.Add(ClickableImage(item));
                    }
                }
                return;

            }
            catch (Exception ex)
            {
                DrawIcons(allIconsFilter);
            }
        }

        private JToken _uISettings;
        private void DrawUISettings()
        {
            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Settings", "UISettings.json");

            var jsontext = System.IO.File.ReadAllText(filePath);

            var uISettings = JObject.Parse(jsontext);

            _uISettings = uISettings["IconPicker"];

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BackgroundColor"].ToString()));
        }

        private void GetIconFilters()
        {

            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Settings", "IconFilters.json");

            var jsontext = System.IO.File.ReadAllText(filePath);

            var filters = JObject.Parse(jsontext);

            this.iconFilters = new IconFilters();

            foreach (var v in filters)
            {
                var filtername = v.Key;
                var filtervalues = v.Value as JArray;
                var iconlist = new List<int>();
                foreach (var item in filtervalues)
                {
                    try
                    {
                        var id = (int)item;
                        iconlist.Add(id);
                    }
                    catch (Exception ex) { }
                }
                iconFilters.AddFilter(new IconFilters.IconFilter(filtername, iconlist));
            }
        }

        private void OnWindowClosing(object sender, EventArgs e)
        {
            if (_manualClose)
                _actionOnIconSelected(-2);
        }

        private Image DrawImage(int iconId)
        {
            var image = new Image();

            string fullFilePath = Services.StaticApiConnection.GetSummonerIconImageUrl(iconId);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            image.Source = bitmap;
            {
                image.Height = 50;
                image.Width = 50;
            }

            return image;
        }

        private UIElement ClickableImage(int iconId)
        {
            var image = DrawImage(iconId);

            var btn = new Button();

            btn.Content = image;

            btn.Click += (sender, e) =>
            {
                _actionOnIconSelected(iconId);
                CloseThis();
            };

            return btn;
        }

        private void CloseThis()
        {
            _manualClose = false;

            this.Close();
        }

        private void TXTSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = ((TextBox)e.Source).Text;
            DrawIconsBySearch(search);
        }

        private void DrawIconsBySearch(string searchText)
        {
            this.ContentPanel.Children.Clear();

            try
            {
                foreach (var item in _lCUConnection.OwnedIcons())
                {
                    if (item.ToString().Contains(searchText))
                    {
                        this.ContentPanel.Children.Add(ClickableImage(item));
                    }
                }
                return;

            }
            catch (Exception ex)
            {
                DrawIcons(allIconsFilter);
            }
        }
    }
}
