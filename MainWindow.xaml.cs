using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace ConwaysLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += new EventHandler(_timer_Tick);
        }

        
        private void OnNext(object sender, RoutedEventArgs e)
        {
            universeView.Next();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Universe"; 
            dlg.DefaultExt = ".xml"; 
            dlg.Filter = "XML documents|*.xml"; 

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                using (var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                {
                    var serializer = new XmlSerializer(typeof (Universe));
                    var universe = universeView.GetUniverseModel();
                    serializer.Serialize(fs, universe);
                    
                }
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML documents|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    XmlReader reader = XmlReader.Create(fs);
                    XmlSerializer serializer = new XmlSerializer(typeof(Universe));
                    var universe = (Universe)serializer.Deserialize(reader);
                    universeView.SetModel(universe);
                }
            }
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {
            universeView.Next();
            if (_timer.IsEnabled)
            {
                _timer.IsEnabled = false;
                btnRun.Content = "Run";
            }
            else
            {
                _timer.IsEnabled = true;
                btnRun.Content = "Stop";
            }
        }

        private void OnClear(object sender, RoutedEventArgs e)
        {
            universeView.Clear();
        }


        void _timer_Tick(object sender, EventArgs e)
        {
            if (!universeView.Next())
            {
                _timer.IsEnabled = false;
                btnRun.Content = "Run";
            }
        }
    }
}
