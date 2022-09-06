﻿using NAudio.CoreAudioApi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

        private string m_TextboxText;

        public string TextboxText
        {
            get { return m_TextboxText; }
            set { m_TextboxText = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Commands

        private ICommand m_StartRecording;


        public ICommand StartRecording
        {
            get
            {
                return m_StartRecording ?? (m_StartRecording = new CommandHandler(() => DoStartRecording(), () => true));
            }
        }

        #endregion

        #region Methods
        private void DoStartRecording()
        {
            OSCHandler.IsTyping(true);
            this.TextboxText = STTHandler.StartSpeaking(SelectedLanguage, SelectedMicrophone, UseStandardMic);
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

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}