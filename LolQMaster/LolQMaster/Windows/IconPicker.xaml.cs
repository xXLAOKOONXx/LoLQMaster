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
using LolQMaster.Services;

namespace LolQMaster.Windows
{
    /// <summary>
    /// Interaktionslogik für IconPicker.xaml
    /// </summary>
    public partial class IconPicker : Window
    {
        private LCUConnection _lCUConnection;
        private Action<int> _actionOnIconSelected;

        public IconPicker(LCUConnection lCUConnection, Action<int> actionOnIconSelected)
        {
            InitializeComponent();

            _lCUConnection = lCUConnection;
            _actionOnIconSelected = actionOnIconSelected;

            foreach (var item in _lCUConnection.OwnedIcons())
            {
                this.ContentPanel.Children.Add(ClickableImage(item));
            }
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
            this.Close();
        }
    }
}
