using System.Windows;
using System.Windows.Input;
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
            this.vm = VRCSTTViewModelFactory.GetInstance();
            this.DataContext = this.vm;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.vm.WindowClosing();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Closed_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
