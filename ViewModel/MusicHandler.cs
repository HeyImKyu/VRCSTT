using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCSTT.Helper;
using Windows.Media.Control;

namespace VRCSTT.ViewModel
{
    internal static class MusicHandler
    {
        private static string lastString;
        internal static string FormatInformation(MediaInformation musicInfo)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(musicInfo.Media.Artist);
            builder.Append(" - ");
            builder.Append(musicInfo.Media.Title);

            builder.Append(" ");

            builder.Append("<");
            builder.Append(musicInfo.Time.Position.ToString(@"mm\:ss"));
            builder.Append("/");
            builder.Append(musicInfo.Time.EndTime.ToString(@"mm\:ss"));
            builder.Append(">");



            return builder.ToString();
        }

        internal static async Task<MediaInformation> GetMusicInformation()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            var mediaProp = await sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
            var timeProp = sessionManager.GetCurrentSession().GetTimelineProperties();

            return new MediaInformation(mediaProp, timeProp);
        }
    }

    internal class MediaInformation
    {
        internal MediaInformation(GlobalSystemMediaTransportControlsSessionMediaProperties media, GlobalSystemMediaTransportControlsSessionTimelineProperties time)
        {
            Media = media;
            Time = time;
        }

        public GlobalSystemMediaTransportControlsSessionMediaProperties Media { get; }
        public GlobalSystemMediaTransportControlsSessionTimelineProperties Time { get; }
    }
}
