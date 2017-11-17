using System.Windows;

namespace Kinect_ing_Pepper.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        unsafe public MainWindow()
        {            
            InitializeComponent();
            
            navigationFrame.Navigate(new RecordPage(navigationFrame));
        }
    }
}
