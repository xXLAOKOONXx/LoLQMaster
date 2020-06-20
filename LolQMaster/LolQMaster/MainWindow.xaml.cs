using LolQMaster.Services;
using LolQMaster.Windows;
using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LolQMaster
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void OnSummonerNameChange(object o, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentSummonerName")
            {
                _iconManager = new IconManager(_lCUConnection.CurrentSummonerName);

                DrawList();
            }
        }

        private JToken _uISettings;
        private IconManager _iconManager;
        private LCUConnection _lCUConnection;
        private IEnumerable<Setting> MagicList
        {
            get
            {
                return _iconManager.QueueSummonerIcons.Select(keyValuePair => new Setting()
                {
                    Id = keyValuePair.Key,
                    ProfileImg = keyValuePair.Value
                });
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            DrawUISettings();

            _iconManager = new IconManager();
            _lCUConnection = new LCUConnection(_iconManager);

            InitBindings();

            DrawList();

            SummonerIconChanged();


        }

        private void DrawUISettings()
        {
            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Settings", "UISettings.json");

            var jsontext = System.IO.File.ReadAllText(filePath);

            var uISettings = JObject.Parse(jsontext);

            _uISettings = uISettings["MainWindow"];

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BackgroundColor"].ToString()));

            this.BTNAddQueue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNAddQueueBackgroundColor"].ToString()));
        }

        private void InitBindings()
        {
            _lCUConnection.PropertyChanged += this.OnSummonerNameChange;

            Binding myBinding = new Binding("CurrentSummonerName");
            myBinding.Source = _lCUConnection;
            // Bind the new data source to the Lebel control's Content dependency property.

            this.LBLSummonerName.SetBinding(Label.ContentProperty, myBinding);

            Binding ClubTagBinding = new Binding("CurrentSummonerClubTag");
            ClubTagBinding.Source = _lCUConnection;
            // Bind the new data source to the Lebel control's Content dependency property.

            this.LBLSummonerClubTag.SetBinding(Label.ContentProperty, ClubTagBinding);

            _lCUConnection.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "CurrentSummonerIconId")
                {
                    SummonerIconChanged();
                }
            };

            Binding ConnectionMessageBinding = new Binding("ConnectionMessage");
            ConnectionMessageBinding.Source = _lCUConnection;
            this.ClientStatus.SetBinding(Label.ContentProperty, ConnectionMessageBinding);
        }

        public void SummonerIconChanged()
        {
            DrawSummonerIcon(_lCUConnection.CurrentSummonerIconId);
        }

        private void DrawSummonerIcon(int id)
        {
            GetImageFromUrl(id, this.IMGCurrentSummonerIcon);
        }
        private void DrawSummonerName(string name)
        {
            this.LBLSummonerName.Content = name;
        }
        private void DrawList()
        {
            SettingsPanel.Dispatcher.Invoke(() =>
            {
                SettingsPanel.Children.Clear();

                foreach (var item in MagicList)
                {
                    SettingsPanel.Children.Add(GetUIElement(item));
                }
            });

        }

        private UIElement GetUIElement(Setting setting)
        {
            var magicWrap = new StackPanel();
            var head = new Button();
            head.Content = setting.Name;
            magicWrap.Children.Add(head);

            var body = new Grid();
            body.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["QueueBodyBackgroundColor"].ToString()));
            body.ColumnDefinitions.Add(new ColumnDefinition());
            body.ColumnDefinitions.Add(new ColumnDefinition());
            body.ColumnDefinitions.Add(new ColumnDefinition());
            var img = GetImageFromUrl(setting.ProfileImg);
            body.Children.Add(img);
            Grid.SetColumn(img, 0);

            magicWrap.Children.Add(body);

            body.Visibility = Visibility.Collapsed;

            head.Click += (sender, e) =>
            {
                if (body.Visibility == Visibility.Collapsed)
                {
                    body.Visibility = Visibility.Visible;
                }
                else if (body.Visibility == Visibility.Visible)
                {
                    body.Visibility = Visibility.Collapsed;
                }
            };
            head.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNheadBackgroundColor"].ToString()));
            head.FontSize = 20;

            // body.Orientation = Orientation.Horizontal;

            var BTNChangeImg = new Button();
            BTNChangeImg.Content = "Change Icon";
            BTNChangeImg.Click += (sender, e) =>
            {
                ChangeSummonerIcon(setting);
            };

            BTNChangeImg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNChangeImgBackgroundColor"].ToString()));
            Grid.SetColumn(BTNChangeImg, 1);

            body.Children.Add(BTNChangeImg);
            if (setting.Id != -1)
            {
                var BTNDelete = new Button();
                BTNDelete.Content = "Delete";
                body.Children.Add(BTNDelete);
                BTNDelete.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNDeleteBackgroundColor"].ToString()));
                BTNDelete.Click += (sender, e) =>
                {
                    this.RemoveMagicListItem(setting.Id);
                };

                Grid.SetColumn(BTNDelete, 2);
            }

            return magicWrap;
        }

        private void ChangeSetting(Setting setting, int IconId)
        {
            _iconManager.AddPair(setting.Id, IconId);

            DrawList();
        }

        private void SummonerIconSelected(int iconId)
        {
            ChangeSetting(_curChangingSetting, iconId);

            this.IsEnabled = true;
        }
        private Setting _curChangingSetting;

        private void ChangeSummonerIcon(Setting setting)
        {
            _curChangingSetting = setting;
            IconPicker iconPicker;
            try
            {
                iconPicker = new IconPicker(_lCUConnection, SummonerIconSelected);

            }
            catch (LCUConnection.NoConnectionException ncex)
            {
                MessageBox.Show(ncex.Message);
                return;
            }

            iconPicker.Show();

            this.IsEnabled = false;
        }

        private void RemoveMagicListItem(Setting user) => RemoveMagicListItem(user.Id);

        private void RemoveMagicListItem(int queue)
        {
            _iconManager.RemoveQueue(queue);

            this.DrawList();
        }

        private void AddMagicListItem(Setting user) => AddMagicListItem(user.Id, user.ProfileImg);
        private void AddMagicListItem(int queue, int iconId)
        {
            _iconManager.AddPair(queue, iconId);

            this.DrawList();
        }

        private Image GetImageFromUrl(int id, Image image = null)
        {
            var flagImageSet = true;
            if (image == null)
            {
                flagImageSet = false;
                image = new Image();
            }
            image.Dispatcher.Invoke(() =>
            {
                string fullFilePath = Services.StaticApiConnection.GetSummonerIconImageUrl(id);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();

                image.Source = bitmap;
                if (!flagImageSet)
                {
                    image.Height = 50;
                    image.Width = 50;
                }
            });

            return image;
        }

        public class Setting
        {
            public int Id { get; set; }
            public string Name { get => Services.StaticApiConnection.GetQueueName(Id); }

            public int ProfileImg { get; set; }
        }

        public void AddIconWindowDone(int queue, int iconId)
        {
            this.AddMagicListItem(queue, iconId);
            this.IsEnabled = true;
        }

        private void BTNAddQueue_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddQueue(_iconManager, _lCUConnection, AddIconWindowDone);
            this.IsEnabled = false;
            addWindow.Show();
        }
    }
}
