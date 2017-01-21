using Discord;
using Discord.Audio;
using NAudio.Wave;
using System.Threading.Tasks;

namespace ChitoseV2
{
    internal class MusicModule
    {
        private readonly string configPath;
        private readonly AudioService service;
        private readonly string tempPath;
        private IAudioClient client;
        private bool paused;
        private object pausedLock = new object();
        private float volume;
        private object volumeLock = new object();

        public bool Paused
        {
            get
            {
                lock (pausedLock)
                {
                    return paused;
                }
            }
            set
            {
                lock (pausedLock)
                {
                    paused = value;
                }
            }
        }

        public float Volume
        {
            get
            {
                lock (volumeLock)
                {
                    return volume;
                }
            }
            set
            {
                lock (volumeLock)
                {
                    volume = value;
                }
            }
        }

        public MusicModule(string pathToConfig, string pathToTemp, AudioService audioService)
        {
            configPath = pathToConfig;
            tempPath = pathToTemp;
            service = audioService;
            volume = 0.5f;
            paused = false;
        }

        public async Task<bool> ConnectTo(Channel voiceChannel)
        {
            if (client == null)
            {
                client = await service.Join(voiceChannel);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Leave()
        {
            if (client != null)
            {
                service.Leave(client.Channel);
                client = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PlayFile(string filePath)
        {
            var channelCount = service.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    for (int i = 0; i < buffer.Length; i += 2)
                    {
                        short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                        short result = (short)(sample * Volume);
                        buffer[i] = (byte)(result & 0xFF);
                        buffer[i + 1] = (byte)(result >> 8);
                    }
                    client.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }
        }
    }
}