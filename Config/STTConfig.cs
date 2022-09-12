using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCSTT.Config
{
    internal static class STTConfig
    {
        private static ConfigProvider instance;

        internal static string SubscriptionKey => GetSubscriptionKey();
        internal static string Region => GetRegionKey(); 

        internal static string Address = "127.0.0.1";
        internal static int OutgoingPort = 9000;
        internal static int IncomingPort = 9001;

        private static string GetSubscriptionKey()
        {
            CreateInstance();
            return instance.GetSubscriptionKey();
        }

        private static string GetRegionKey()
        {
            CreateInstance();
            return instance.GetRegion();
        }

        private static void CreateInstance()
        {
            if (instance == null)
                instance = new ConfigProvider();
        }
    }
}
