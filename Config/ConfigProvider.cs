using System.Configuration;
using System.Collections.Specialized;
using System;
using System.IO;

namespace VRCSTT.Config
{
    internal class ConfigProvider
    {
        private string m_SubKey;
        private string m_Region;
        private int m_DelayTime;

        private STTConfigSection sttConfigSection;
        private Configuration configFile;

        internal ConfigProvider()
        {
            string exeFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var configFile = ConfigurationManager.OpenExeConfiguration(exeFileName);
            var sttConfigSection = (STTConfigSection)configFile.GetSection("VRCSTTSettings");

            this.sttConfigSection = sttConfigSection;
            this.configFile = configFile;
        }

        internal string GetSubscriptionKey()
        {
            if (m_SubKey == null)
                m_SubKey = sttConfigSection.SubscriptionKey;

            return m_SubKey;
        }

        internal string GetRegion()
        {
            if (m_Region == null)
                m_Region = sttConfigSection.Region;

            return m_Region;
        }

        internal int GetDelayTime()
        {
            if (m_DelayTime == 0)
                m_DelayTime = sttConfigSection.DelaySeconds;

            return m_DelayTime;
        }

        internal void SetDelayTime(int seconds)
        {
            sttConfigSection.DelaySeconds = seconds;
            configFile.Save();
        }
    }

    internal class STTConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("SubscriptionKey")]
        public string SubscriptionKey
        {
            get { return (string)this["SubscriptionKey"]; }
            set { this["SubscriptionKey"] = value; }
        }

        [ConfigurationProperty("Region")]
        public string Region
        {
            get { return (string)this["Region"]; }
            set { this["Region"] = value; }
        }

        [ConfigurationProperty("DelaySeconds")]
        public int DelaySeconds
        {
            get { return (int)this["DelaySeconds"]; }
            set { this["DelaySeconds"] = value; }
        }
    }
}
