using NAudio.CoreAudioApi;
using OscCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VRCSTT.Config;
using VRCSTT.Helper;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal class VRCSTTViewModel : INotifyPropertyChanged
    {
        #region Constructor

        internal VRCSTTViewModel()
        {
            this.SelectedLanguage = this.Languages.FirstOrDefault();

            this.SelectedMicrophone = this.Microphones.FirstOrDefault();
            this.UseStandardMic = true;

            this.incomingOscClient = new IncomingOscClient(STTConfig.IncomingPort, ReceiveIncomingCallback);
        }

        #endregion

        #region Attributes

        private readonly IncomingOscClient incomingOscClient;

        #endregion

        #region Properties
        public string[] Languages
        {
            get
            {
                return new string[] { "en-US", "de-DE", "fi-FI" };
            }
        }
        private string m_SelectedLanguage;

        public string SelectedLanguage
        {
            get { return m_SelectedLanguage; }
            set { m_SelectedLanguage = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Microphone> m_Microphones = new ObservableCollection<Microphone>();
        public ObservableCollection<Microphone> Microphones
        {
            get
            {
                if (m_Microphones.Count == 0)
                    SetMicrophones();

                return m_Microphones;
            }
        }

        private Microphone m_SelectedMicrophone;
        public Microphone SelectedMicrophone
        {
            get { return m_SelectedMicrophone; }
            set { m_SelectedMicrophone = value; NotifyPropertyChanged(); }
        }

        private bool m_UseStandardMic;
        public bool UseStandardMic
        {
            get { return m_UseStandardMic; }
            set { m_UseStandardMic = value; NotifyPropertyChanged(); }
        }

        private string m_TextboxText;

        public string TextboxText
        {
            get { return m_TextboxText; }
            set { m_TextboxText = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<HistoryPoint> m_VoiceHistory = new ObservableCollection<HistoryPoint>();
        public ObservableCollection<HistoryPoint> VoiceHistory
        {
            get { return m_VoiceHistory; }
            set { m_VoiceHistory = value; NotifyPropertyChanged(); }
        }

        private Visibility m_OSCIncoming = Visibility.Collapsed;

        public Visibility MicActivationVisible
        {
            get { return m_OSCIncoming; }
            set { m_OSCIncoming = value; NotifyPropertyChanged(); }
        }


        #endregion

        #region Commands

        private ICommand m_StartRecording;
        public ICommand StartRecording
        {
            get
            {
                return m_StartRecording ?? (m_StartRecording = new CommandHandler(o => DoStartRecording(), () => true));
            }
        }

        #endregion

        #region Methods

        private async void DoStartRecording()
        {
            OSCHandler.IsTyping(true);

            var speakTask = Task.Run(() => STTHandler.StartSpeaking(SelectedLanguage, SelectedMicrophone, UseStandardMic));
            MicActivationVisible = Visibility.Visible;

            var result = await speakTask;

            MicActivationVisible = Visibility.Collapsed;

            this.TextboxText = result; 
            OSCHandler.SendOverOSC(result);
            this.AddHistoryPoint(result);
        }

        private void SetMicrophones()
        {
            m_Microphones.Clear();
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                m_Microphones.Add(new Microphone() { FriendlyName = endpoint.FriendlyName, ID = endpoint.ID });
            }
        }

        private void AddHistoryPoint(string voiceString)
        {
            if (m_VoiceHistory.Count >= 5)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    m_VoiceHistory.RemoveAt(0);
                });
            }

            var point = new HistoryPoint() { text = voiceString, ID = Guid.NewGuid() };

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                m_VoiceHistory.Add(point);
            });
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReceiveIncomingCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).client;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).endpoint;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);
            OscMessage pack = (OscMessage)OscPacket.Read(receiveBytes, 0, receiveBytes.Length);

            if (pack.Address == "/avatar/parameters/StartVoiceRecognition" && ((bool)pack.FirstOrDefault()))
                DoStartRecording();

            if (this.incomingOscClient != null)
                this.incomingOscClient.BeginReceiving();

        }
    }
}
