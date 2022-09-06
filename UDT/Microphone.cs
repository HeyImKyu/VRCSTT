namespace VRCSTT.UDT
{
    internal class Microphone
    {
        private string m_FriendlyName;

        public string FriendlyName
        {
            get { return m_FriendlyName; }
            set { m_FriendlyName = value; }
        }

        private string m_ID;

        public string ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

    }
}
