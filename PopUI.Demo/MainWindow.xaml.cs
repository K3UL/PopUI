using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PopUI.Demo
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Color colorVS { get; set; }

        public MainWindow()
        {
            colorVS = (Color)ColorConverter.ConvertFromString("#662E93");

            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                for (int i = ((int[])args.Argument)[0]; i < ((int[])args.Argument)[1]; i++)
                {
                    int percent = i + 1;

                    Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        progressImage.Value = percent;
                        lblPercent.Content = percent + " %";
                    }));

                    Thread.Sleep(100);
                }
            };

            worker.RunWorkerAsync(new int[] { progressImage.Minimum, progressImage.Maximum });
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            progressImage.Value = 0;
        }

        private ImageSource GetImageSource(string imageName)
        {
            string packUri = "pack://application:,,,/PopUI.Demo;component/Images/" + imageName;
            return new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(colorVS);
            progressImage.Image = GetImageSource("vs256_white.png");
            lblPercent.Foreground = new SolidColorBrush(Colors.White);
        }

        private void button_Copy2_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Colors.White);
            progressImage.Image = GetImageSource("vs256.png");
            lblPercent.Foreground = new SolidColorBrush(colorVS);
        }
    }
}
