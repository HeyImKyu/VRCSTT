using NAudio.CoreAudioApi;
using OscCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
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

            this.cancellationTokenSource = new CancellationTokenSource();

            LoadFavourites();
        }

        #endregion

        #region Attributes

        private readonly IncomingOscClient incomingOscClient;
        public CancellationTokenSource cancellationTokenSource { get; private set; }
        public Task RunningTask { get; private set; }
        private const string FavouritesFilePath = ".\\Favourites.save";
        private STTHandler sttHandler = new STTHandler();

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

        private ObservableCollection<HistoryPoint> m_Favourites = new ObservableCollection<HistoryPoint>();
        public ObservableCollection<HistoryPoint> Favourites
        {
            get { return m_Favourites; }
            set { m_Favourites = value; NotifyPropertyChanged(); }
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

        private ICommand m_TextboxFocusCommand;
        public ICommand TextboxFocusCommand
        {
            get
            {
                return m_TextboxFocusCommand ?? (m_TextboxFocusCommand = new CommandHandler(o => DoStartFocus(), () => true));
            }
        }


        private ICommand m_TextboxEnterCommand;
        public ICommand TextboxEnterCommand
        {
            get
            {
                return m_TextboxEnterCommand ?? (m_TextboxEnterCommand = new CommandHandler(o => DoSendTextbox(), () => true));
            }
        }


        #endregion

        #region Methods

        private async void DoStartRecording()
        {
            // Cancel recording if currently running
            if (RunningTask != null && !cancellationTokenSource.Token.IsCancellationRequested && !RunningTask.IsCompleted)
            {
                STTHandler.AbortSpeaking();
                cancellationTokenSource.Cancel();
                MicActivationVisible = Visibility.Collapsed;
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            OSCHandler.IsTyping(true);

            var speakTask = Task.Run(() => sttHandler.StartSpeaking(SelectedLanguage, SelectedMicrophone, UseStandardMic, cancellationTokenSource.Token));
            this.RunningTask = speakTask.Result;
            MicActivationVisible = Visibility.Visible;

            var result = await speakTask;

            MicActivationVisible = Visibility.Collapsed;

            //if (result == "")
            //    return;

            //this.TextboxText = result;
            //OSCHandler.SendOverOSC(result);
            //this.AddHistoryPoint(result);
        }

        private void DoStartFocus()
        {
            TextboxText = "";
        }

        private void DoSendTextbox()
        {
            OSCHandler.SendOverOSC(TextboxText);
            this.AddHistoryPoint(TextboxText);
            this.TextboxText = "";
        }

        private void SetMicrophones()
        {
            m_Microphones.Clear();
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
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

            var point = new HistoryPoint(voiceString, this);

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

        internal void WindowClosing()
        {
            STTHandler.AbortSpeaking();
            cancellationTokenSource.Cancel();
            SaveFavourites();
        }

        private void SaveFavourites()
        {
            string serialized = JsonSerializer.Serialize(this.Favourites);

            if (!File.Exists(FavouritesFilePath))
            {
                var fs = File.Create(FavouritesFilePath);
                fs.Close();
            }

            using (StreamWriter sw = new StreamWriter(FavouritesFilePath))
            {
                sw.Write(serialized);
            }
        }


        private void LoadFavourites()
        {
            if (!File.Exists(FavouritesFilePath))
                return;

            using (StreamReader sr = new StreamReader(FavouritesFilePath))
            {
                string contents = sr.ReadToEnd();
                ObservableCollection<HistoryPoint> favourites = JsonSerializer.Deserialize<ObservableCollection<HistoryPoint>>(contents);

                foreach (HistoryPoint favourite in favourites)
                {
                    favourite.parent = this;
                    favourite.m_IsFavourited = true;
                }
                this.Favourites = favourites;
            }

        }
    }
}
