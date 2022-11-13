using System.Windows.Controls;
using VRCSTT.ViewModel;

namespace VRCSTT.Views
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            this.DataContext = VRCSTTViewModelFactory.GetInstance().HomeViewModel;
            InitializeComponent();
        }
    }
}
