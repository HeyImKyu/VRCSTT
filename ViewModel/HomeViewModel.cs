using NAudio.CoreAudioApi;
using OscCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
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
    internal class HomeViewModel : INotifyPropertyChanged
    {
        public VRCSTTViewModel Parent => VRCSTTViewModelFactory.GetInstance();

        #region Constructor

        internal HomeViewModel()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Attributes

        public CancellationTokenSource cancellationTokenSource { get; private set; }
        public Task<string> RunningTask { get; private set; }

        #endregion

        #region Properties

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

        private Visibility m_MicActiovationVisible = Visibility.Collapsed;
        public Visibility MicActivationVisible
        {
            get { return m_MicActiovationVisible; }
            set { m_MicActiovationVisible = value; NotifyPropertyChanged(); }
        }

        private int m_SecondsTimer;
        public int SecondsTimer
        {
            get { return m_SecondsTimer;}
            set { m_SecondsTimer = value; NotifyPropertyChanged(); }
        }

        private bool m_KeepActive;
        public bool KeepActive
        {
            get { return m_KeepActive; }
            set { m_KeepActive = value; NotifyPropertyChanged(); }
        }

        private bool m_CurrentSong;
        public bool CurrentSong
        {
            get { return m_CurrentSong; }
            set { m_CurrentSong = value; NotifyPropertyChanged(); m_TextboxText = ""; OSCHandler.SendOverOSC("", 0); /* Trigger MusicLoop*/ }
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

        internal async void DoStartRecording()
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

            var settings = VRCSTTViewModelFactory.GetInstance().SettingsViewModel;
            var speakTask = Task.Run(() => 
            STTHandler.StartSpeaking(
                settings.SelectedLanguage,
                settings.SelectedMicrophone,
                settings.UseStandardMic, 
                cancellationTokenSource.Token));
            this.RunningTask = speakTask;
            MicActivationVisible = Visibility.Visible;


            var result = await speakTask;

            MicActivationVisible = Visibility.Collapsed;

            if (result == "")
                return;

            this.TextboxText = result;
            OSCHandler.SendOverOSC(result, SecondsTimer);
            this.AddHistoryPoint(result);
        }

        private void DoStartFocus()
        {
            TextboxText = "";
        }

        private void DoSendTextbox()
        {
            var latinized = TextboxText.Latinize();
            OSCHandler.SendOverOSC(latinized, SecondsTimer);
            this.AddHistoryPoint(latinized);
            this.TextboxText = "";
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

            var point = new HistoryPoint(voiceString);

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
    }
}
