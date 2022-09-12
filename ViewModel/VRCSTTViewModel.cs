using NAudio.CoreAudioApi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VRCSTT.Helper;
using VRCSTT.UDT;
using NAudio.Wave;
using System.Drawing;

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
            set 
            {
                this.AutoMic = false;
                m_SelectedMicrophone = value; NotifyPropertyChanged();
            }
        }

        private bool m_UseStandardMic;
        public bool UseStandardMic
        {
            get { return m_UseStandardMic; }
            set { m_UseStandardMic = value; NotifyPropertyChanged(); }
        }

        private bool m_AutoMic;
        public bool AutoMic
        {
            get { return m_AutoMic; }
            set 
            { 
                if (SelectedMicrophone != null)
                {
                    if (value)
                    {                    
                        SelectedMicrophone.AudioWave.StartRecording();
                    }
                    else
                        SelectedMicrophone.AudioWave.StopRecording();
                }

                m_AutoMic = value; NotifyPropertyChanged();
            }
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

        private double m_MicrophoneLevel;
        public double MicrophoneLevel
        {
            get { return m_MicrophoneLevel; }
            set { m_MicrophoneLevel = value; NotifyPropertyChanged(); }
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

        private void AudioWaveHandler(object? sender, WaveInEventArgs e)
        {
            // copy buffer into an array of integers
            Int16[] values = new Int16[e.Buffer.Length / 2];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

            // determine the highest value as a fraction of the maximum possible value
            float fraction = (float)values.Max() / 32768;

            if (fraction > .2)
            {
                DoStartRecording();
            }

            this.MicrophoneLevel = fraction;
        }

        #endregion

        #region Methods
        private void DoStartRecording()
        {
            OSCHandler.IsTyping(true);

            var result = STTHandler.StartSpeaking(SelectedLanguage, SelectedMicrophone, UseStandardMic);

            this.TextboxText = result; 
            OSCHandler.SendOverOSC(result);
            this.AddHistoryPoint(result);
        }

        private void SetMicrophones()
        {
            m_Microphones.Clear();
            var enumerator = new MMDeviceEnumerator();
            var endpointList = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            for (int i = 0; i < endpointList.Count(); i++)
            {
                var endpoint = endpointList[i];
                var mic = new Microphone(endpoint, i);
                mic.AudioWave.DataAvailable += AudioWaveHandler;

                m_Microphones.Add(mic);
            }
        }

        private void AddHistoryPoint(string voiceString)
        {
            if (m_VoiceHistory.Count >= 5)
                m_VoiceHistory.RemoveAt(0);

            var point = new HistoryPoint() { text = voiceString, ID = Guid.NewGuid() };

            this.m_VoiceHistory.Add(point);
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
