using System.Windows;
using VRCSTT.ViewModel;

namespace VRCSTT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VRCSTTViewModel vm;

        public MainWindow()
        {
            InitializeComponent();
            this.vm = new VRCSTTViewModel();
            this.DataContext = this.vm;
        }
    }
}
