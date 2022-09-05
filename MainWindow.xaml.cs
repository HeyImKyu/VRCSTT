using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OscCore;
using OscCore.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VRCSTT.Config;

namespace VRCSTT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string[] Languages
        {
            get
            {
                return new string[] { "en-US", "de-DE", "fi-FI" };
            }
        }

        private void SetTextboxText(string text)
        {
            OutputTextbox.Text = text;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LanguageBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OSCHandler.IsTyping(true);
            SetTextboxText(STTHandler.StartSpeaking(LanguageBox.SelectedItem.ToString()));
        }
    }
}
