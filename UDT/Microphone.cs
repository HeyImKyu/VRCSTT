using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Linq;

namespace VRCSTT.UDT
{
    internal class Microphone
    {
        internal Microphone (MMDevice endpoint, int id)
        {
            var audioWave = new WaveInEvent
            {
                DeviceNumber = id,
                WaveFormat = new WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = 20
            };
            this.FriendlyName = endpoint.FriendlyName;
            this.ID = endpoint.ID;
            this.AudioWave = audioWave;
        }

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

        private WaveInEvent m_AudioWave;
        public WaveInEvent AudioWave
        {
            get { return m_AudioWave; }
            set { m_AudioWave = value; }
        }

    }
}
