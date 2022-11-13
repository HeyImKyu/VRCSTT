using System.Windows.Controls;
using VRCSTT.ViewModel;

namespace VRCSTT.Views
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            this.DataContext = VRCSTTViewModelFactory.GetInstance().SettingsViewModel;
            InitializeComponent();
        }
    }
}
