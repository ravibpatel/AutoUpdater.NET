using System.Reflection;
using System.Windows;
using AutoUpdater.NET.Core.WPF;


namespace AutoUpdaterTest.NET.Core.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetEntryAssembly();
            //LabelVersion.Content = $"Current Version : {assembly.GetName().Version}";
        }

        
        private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            ApplicationUpdater applicationUpdater = new ApplicationUpdater();
            applicationUpdater.Start("http://localhost/test.xml");
        }
    }
}
