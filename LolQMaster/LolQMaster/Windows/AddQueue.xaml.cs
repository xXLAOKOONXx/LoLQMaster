using LolQMaster.Services;
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
using System.Windows.Shapes;

namespace LolQMaster.Windows
{
    /// <summary>
    /// Interaktionslogik für AddQueue.xaml
    /// </summary>
    public partial class AddQueue : Window
    {
        private bool _manualClose = true;
        private LCUConnection _lCUConnection;
        private Action<int, int> _action;
        public AddQueue(IconManager iconManager, LCUConnection lCUConnection, Action<int, int> action)
        {
            InitializeComponent();

            _lCUConnection = lCUConnection;
            _action = action;

            this.Closing += OnWindowClosing;

            SummonerIconSelected(-1);

            DrawUISettings();

            var availableQList = StaticApiConnection.AvailableQueues.ToList();

            foreach (var q in iconManager.QueueSummonerIcons)
            {
                if (availableQList.Contains(q.Key))
                {
                    availableQList.Remove(q.Key);
                }
            }

            if (availableQList.Count == 0)
            {
                var msgbx = MessageBox.Show("Check your settings list, you already have every available queue in your list.");
                _manualClose = false;
                this.Close();

                return;
            }


            foreach (var q in availableQList)
            {
                this.QueuePicker.Children.Add(PickerOption(q));
            }
            selectedQueue = availableQList.First();
            RadioButtonClicked.Invoke(null, availableQList.First());
        }

        private JToken _uISettings;
        private void DrawUISettings()
        {
            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Settings", "UISettings.json");

            var jsontext = System.IO.File.ReadAllText(filePath);

            var uISettings = JObject.Parse(jsontext);

            _uISettings = uISettings["AddQueue"];

            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BackgroundColor"].ToString()));

            this.BTNAddQueue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNAddQueueBackgroundColor"].ToString()));

            this.BTNPickIcon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["BTNPickIconBackgroundColor"].ToString()));
        }

        private int selectedQueue;

        private EventHandler<int> RadioButtonClicked;

        private UIElement PickerOption(int queueId)
        {
            var horistack = new StackPanel();
            horistack.Margin = new Thickness(5, 5, 5, 5);
            horistack.Orientation = Orientation.Horizontal;
            var radio = new RadioButton();
            radio.Padding = new Thickness(10);
            radio.VerticalAlignment = VerticalAlignment.Center;
            radio.Click += (sender, e) =>
            {
                selectedQueue = queueId;
                RadioButtonClicked.Invoke(sender, queueId);
            };
            RadioButtonClicked += (sender, selectedQueue) => { radio.IsChecked = selectedQueue == queueId; };

            var lbl = new Label();
            lbl.Content = StaticApiConnection.GetQueueName(queueId);
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.Width = 150;
            lbl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_uISettings["LBLQueueBackgroundColor"].ToString()));
            lbl.MouseDown += (sender, e) =>
            {
                selectedQueue = queueId;
                RadioButtonClicked.Invoke(sender, queueId);
            };

            horistack.Children.Add(radio);
            horistack.Children.Add(lbl);

            return horistack;
        }

        private int _iconId;

        private void SummonerIconSelected(int iconId)
        {
            _iconId = iconId;

            if (_iconId != -2)
                this.GetImageFromUrl(iconId, this.IMGSummonerIcon);

            this.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IconPicker iconPicker;

            try
            {
                iconPicker = new IconPicker(_lCUConnection, SummonerIconSelected);

                iconPicker.Show();
            }
            catch (LCUConnection.NoConnectionException ncex)
            {
                MessageBox.Show(ncex.Message);
                return;
            }

            this.IsEnabled = false;

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

        private void BTNAddQueue_Click(object sender, RoutedEventArgs e)
        {
            _action(this.selectedQueue, this._iconId);

            this.Close();
        }

        private void OnWindowClosing(object sender, EventArgs e)
        {
            if (_manualClose)
                _action(-1, -2);
        }
    }
}
