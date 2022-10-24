using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VRCSTT.Helper;
using VRCSTT.ViewModel;

namespace VRCSTT.UDT
{
    internal class HistoryPoint : INotifyPropertyChanged
    {
        internal VRCSTTViewModel parent;

        public HistoryPoint() { }
        internal HistoryPoint(string text, VRCSTTViewModel parent)
        {
            this.parent = parent;
            this.m_Text = text;
            this.m_ID = Guid.NewGuid();
        }


        private string m_Text;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }


        private Guid m_ID;

        public Guid ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        internal bool m_IsFavourited;
        public Visibility IsFavourited => m_IsFavourited ? Visibility.Visible : Visibility.Collapsed;


        private ICommand m_SendHistoryPoint;
        public ICommand SendHistoryPoint
        {
            get
            {
                return m_SendHistoryPoint ?? (m_SendHistoryPoint = new CommandHandler(o => DoSendHistoryPoint(), () => true));
            }
        }

        private ICommand m_ToggleFavourite;
        public ICommand ToggleFavourite
        {
            get
            {
                return m_ToggleFavourite ?? (m_ToggleFavourite = new CommandHandler(o => DoToggleFavourite(), () => true));
            }
        }

        private void DoSendHistoryPoint()
        {
            OSCHandler.SendOverOSC(Text, 10);
            Console.WriteLine(Text);
        }

        private void DoToggleFavourite()
        {
            this.m_IsFavourited = !this.m_IsFavourited;
            NotifyPropertyChanged(nameof(IsFavourited));

            if (this.m_IsFavourited)
            {
                this.parent.Favourites.Add(this);
                this.parent.VoiceHistory.Remove(this);
            }
            else
            {
                this.parent.Favourites.Remove(this);
                this.parent.VoiceHistory.Add(this);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}