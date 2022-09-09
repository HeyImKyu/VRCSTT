using System;
using System.Windows.Input;
using VRCSTT.Helper;
using VRCSTT.ViewModel;

namespace VRCSTT.UDT
{
    internal class HistoryPoint
    {
        private string m_text;

        public string text
        {
            get { return m_text; }
            set { m_text = value; }
        }


        private Guid m_ID;

        public Guid ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }


        private ICommand m_SendHistoryPoint;
        public ICommand SendHistoryPoint
        {
            get
            {
                return m_SendHistoryPoint ?? (m_SendHistoryPoint = new CommandHandler(o => DoSendHistoryPoint(), () => true));
            }
        }

        private void DoSendHistoryPoint()
        {
            OSCHandler.SendOverOSC(text);
            Console.WriteLine(text);
        }
    }
}