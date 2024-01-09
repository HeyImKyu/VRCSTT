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
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public VRCSTTViewModel Parent => VRCSTTViewModelFactory.GetInstance();

        #region Constructor

        internal SettingsViewModel()
        {
            this.SelectedLanguage = Languages.FirstOrDefault();
            this.SelectedMicrophone = Microphones.FirstOrDefault();
            this.UseStandardMic = true;
            this.SecondsTimer = STTConfig.DelayTime;

            this.UseTranslateHack = false;
        }

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
            set 
            { 
                m_CurrentSong = value; 
                NotifyPropertyChanged(); 

                VRCSTTViewModelFactory.GetInstance().HomeViewModel.TextboxText = ""; 
                OSCHandler.SendOverOSC("", 0); // Trigger MusicLoop
            }
        }


        private bool m_UseTranslateHack;
        public bool UseTranslateHack
        {
            get { return m_UseTranslateHack; }
            set { m_UseTranslateHack = value; }
        }


        #endregion

        #region Methods

        private void SetMicrophones()
        {
            m_Microphones.Clear();
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                m_Microphones.Add(new Microphone() { FriendlyName = endpoint.FriendlyName, ID = endpoint.ID });
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
