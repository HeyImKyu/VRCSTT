using AutoUpdaterDotNET;
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VRCSTT.Config;
using VRCSTT.Helper;
using VRCSTT.UDT;

namespace VRCSTT.ViewModel
{
    internal static class VRCSTTViewModelFactory
    {
        private static VRCSTTViewModel m_instance { get; set; }

        public static VRCSTTViewModel GetInstance()
        {
            m_instance ??= new VRCSTTViewModel();

            return m_instance;
        }
    }

    internal class VRCSTTViewModel : INotifyPropertyChanged
    {
        #region Constructor

        internal VRCSTTViewModel()
        {
            // If theres a backup file, an update has just occured and we need to load from the backup file
            if (File.Exists(".\\VRCSTT.dll.config.bak"))
            {
                try
                {
                    File.Copy(".\\VRCSTT.dll.config.bak", ".\\VRCSTT.dll.config", true);
                    File.Delete(".\\VRCSTT.dll.config.bak");
                }
                catch 
                {
                    System.Windows.Forms.MessageBox.Show(
                        @"There was an error loading the file from before the update. You can just remove the bak from 'VRCSTT.dll.config.bak' to load the backup manually.",
                        @"Config loading failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            AutoUpdater.CheckForUpdateEvent += AutoUpdaterHandler.AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdaterHandler.Start();

            this.homeViewModel = new HomeViewModel();
            this.settingsViewModel = new SettingsViewModel();
            this.CurrentView = homeViewModel;

            this.incomingOscClient = new IncomingOscClient(STTConfig.IncomingPort, ReceiveIncomingCallback);

            LoadFavourites();
        }

        #endregion

        #region Attributes

        private readonly HomeViewModel homeViewModel;
        private readonly SettingsViewModel settingsViewModel;
        private readonly IncomingOscClient incomingOscClient;

        private const string FavouritesFilePath = ".\\Favourites.save";

        #endregion

        #region Properties

        private object m_currentView;
        public object CurrentView
        {
            get { return m_currentView; }
            set { m_currentView = value; NotifyPropertyChanged(); }
        }

        private Visibility m_OSCIncoming = Visibility.Collapsed;
        public Visibility MicActivationVisible
        {
            get { return m_OSCIncoming; }
            set { m_OSCIncoming = value; NotifyPropertyChanged(); }
        }

        public HomeViewModel HomeViewModel
        {
            get { return this.homeViewModel; }
        }

        public SettingsViewModel SettingsViewModel
        {
            get { return this.settingsViewModel; }
        }

        #endregion

        #region Commands

        private ICommand m_HomeCommand;
        public ICommand HomeCommand
        {
            get
            {
                return m_HomeCommand ?? (m_HomeCommand = new CommandHandler(o => this.CurrentView = homeViewModel, () => true));
            }
        }

        private ICommand m_SettingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                return m_SettingsCommand ?? (m_SettingsCommand = new CommandHandler(o => this.CurrentView = settingsViewModel, () => true));
            }
        }

        #endregion

        #region Methods

        private void ReceiveIncomingCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).client;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).endpoint;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);
            OscMessage pack = (OscMessage)OscPacket.Read(receiveBytes, 0, receiveBytes.Length);

            if (pack.Address == "/avatar/parameters/StartVoiceRecognition" && ((bool)pack.FirstOrDefault()))
                homeViewModel.DoStartRecording();

            if (this.incomingOscClient != null)
                this.incomingOscClient.BeginReceiving();

        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void WindowClosing()
        {
            STTHandler.AbortSpeaking();
            homeViewModel.cancellationTokenSource.Cancel();
            SaveFavouritesAndSeconds();
        }

        private void SaveFavouritesAndSeconds()
        {
            STTConfig.SetDelayTime(settingsViewModel.SecondsTimer);

            string serialized = JsonSerializer.Serialize(homeViewModel.Favourites);

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
            settingsViewModel.SecondsTimer = STTConfig.DelayTime;

            if (!File.Exists(FavouritesFilePath))
                return;

            using (StreamReader sr = new StreamReader(FavouritesFilePath))
            {
                string contents = sr.ReadToEnd();
                ObservableCollection<HistoryPoint> favourites = JsonSerializer.Deserialize<ObservableCollection<HistoryPoint>>(contents);

                foreach (HistoryPoint favourite in favourites)
                {
                    favourite.m_IsFavourited = true;
                }
                homeViewModel.Favourites = favourites;
            }
        }
    }
}
