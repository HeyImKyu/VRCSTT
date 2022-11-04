using System.Configuration;
using System.Collections.Specialized;

namespace VRCSTT.Config
{
    internal class ConfigProvider
    {
        private string m_SubKey;
        private string m_Region;
        private int m_DelayTime;


        internal string GetSubscriptionKey()
        {
            if (m_SubKey == null)
                m_SubKey = ConfigurationManager.AppSettings.Get("SubscriptionKey");

            return m_SubKey;
        }

        internal string GetRegion()
        {
            if (m_Region == null)
                m_Region = ConfigurationManager.AppSettings.Get("Region");

            return m_Region;
        }

        internal int GetDelayTime()
        {
            if (m_DelayTime == null)
                m_DelayTime = int.Parse(ConfigurationManager.AppSettings.Get("DelaySeconds"));

            return m_DelayTime;
        }
    }
}
