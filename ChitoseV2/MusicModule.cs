using Discord.Audio;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace ChitoseV2
{
    internal class MusicModule
    {
        public event OnSongChangedHandler OnSongChanged;

        private static readonly YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = new StreamReader(File.OpenRead(Chitose.ConfigDirectory + "youtube.txt")).ReadToEnd(),
            ApplicationName = "Chitose V2"
        });

        private readonly AudioService service;

        private IAudioClient client;
        private object clientLock = new object();
        private Song currentSong;
        private AudioState currentState;
        private object currentStateLock = new object();
        private bool paused;
        private object pausedLock = new object();
        private List<Song> queue;
        private bool requestStop;
        private object requestStopLock = new object();
        private float volume;
        private object volumeLock = new object();

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

        private IAudioClient Client
        {
            get
            {
                lock (clientLock)
                {
                    return client;
                }
            }
            set
            {
                lock (clientLock)
                {
                    client = value;
                }
            }
        }

        private AudioState CurrentState
        {
            get
            {
                lock (currentStateLock)
                {
                    return currentState;
                }
            }
            set
            {
                lock (currentStateLock)
                {
                    currentState = value;
                }
            }
        }

        private bool Paused
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

        private bool RequestStop
        {
            get
            {
                lock (requestStopLock)
                {
                    return requestStop;
                }
            }
            set
            {
                lock (requestStopLock)
                {
                    requestStop = value;
                }
            }
        }

        public MusicModule(AudioService audioService)
        {
            service = audioService;
            volume = 0.5f;
            paused = false;
            currentState = AudioState.Stopped;
            queue = new List<Song>();
            OnSongChanged += (_) => { };
        }

        public async Task<string> AddToQueue(IEnumerable<string> searchTerms)
        {
            SearchResult bestResult = await GetBestResult(searchTerms);
            if (bestResult != null)
            {
                queue.Add(new Song() { Title = bestResult.Snippet.Title, Url = $"https://www.youtube.com/watch?v={bestResult.Id.VideoId}" });
                if (queue.Count == 1 && CurrentState == AudioState.Playing && currentSong == null)
                {
                    PlayNext();
                }
                return bestResult.Snippet.Title;
            }
            else
            {
                return null;
            }
        }

        public void ClearQueue()
        {
            queue.Clear();
        }

        public async Task<bool> ConnectTo(Discord.Channel voiceChannel)
        {
            if (Client == null || Client.State == Discord.ConnectionState.Disconnected || Client.Channel.Name != voiceChannel.Name)
            {
                Client = await service.Join(voiceChannel);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string[] GetQueue()
        {
            return queue.Select(song => song.Title).ToArray();
        }

        public bool Leave()
        {
            if (Client != null)
            {
                StopPlaying();
                service.Leave(Client.Channel);
                Client = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public string MoveToTopOfQueue(int index)
        {
            if (index >= 1 && index <= queue.Count)
            {
                Song temp = queue[index - 1];
                queue[index - 1] = queue[0];
                queue[0] = temp;
                return queue[0].Title;
            }
            else
            {
                return null;
            }
        }

        public bool PlayNext()
        {
            Paused = false;
            if (queue.Count != 0)
            {
                AcquireAndPlay(queue[0]);
                OnSongChanged(queue[0].Title);
                queue.RemoveAt(0);
                return true;
            }
            else
            {
                OnSongChanged(null);
                return false;
            }
        }

        public string RemoveFromQueue(int index)
        {
            if (index >= 1 && index <= queue.Count)
            {
                string title = queue[index - 1].Title;
                queue.RemoveAt(index - 1);
                return title;
            }
            else
            {
                return null;
            }
        }

        public bool SetPause(bool pause)
        {
            bool changed = Paused != pause;
            Paused = pause;
            return changed;
        }

        public bool Skip()
        {
            if (currentSong != null)
            {
                RequestStop = true;
                Paused = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StartPlaying()
        {
            if (Client == null)
            {
                return false;
            }
            if (CurrentState != AudioState.Playing)
            {
                CurrentState = AudioState.Playing;
                PlayNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StopPlaying()
        {
            if (CurrentState != AudioState.Stopped)
            {
                RequestStop = true;
                CurrentState = AudioState.Stopped;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AcquireAndPlay(Song song)
        {
            currentSong = song;
            IEnumerable<VideoInfo> infos = DownloadUrlResolver.GetDownloadUrls(song.Url);
            VideoInfo video = infos.OrderByDescending(info => info.AudioBitrate).FirstOrDefault();
            if (video != null)
            {
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }
                string videoFile = Chitose.TempDirectory + Chitose.CleanFileName(song.Title + video.VideoExtension);
                string audioFile = Chitose.TempDirectory + Chitose.CleanFileName(song.Title + ".mp3");
                var videoDownloader = new VideoDownloader(video, videoFile);
                videoDownloader.Execute();
                File.Delete(audioFile);
                Process process = new Process();
                process.StartInfo.FileName = Chitose.FfmpegPath + "ffmpeg.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = $"-i \"{videoFile}\" \"{audioFile}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                process.Close();
                File.Delete(videoFile);
                new Task(() => PlayFile(audioFile)).Start();
            }
        }

        private void FinishedSong()
        {
            //File.Delete(Chitose.TempDirectory + Chitose.CleanFileName(currentSong.Title + ".mp3"));
            currentSong = null;
            if (CurrentState == AudioState.Playing)
            {
                PlayNext();
            }
        }

        private async Task<SearchResult> GetBestResult(IEnumerable<string> searchTerms)
        {
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.Q = string.Join("+", searchTerms);
            searchRequest.MaxResults = 25;
            var response = await searchRequest.ExecuteAsync();
            return response.Items.FirstOrDefault(x => x.Id.Kind == "youtube#video");
        }

        private void PlayFile(string filePath)
        {
            if (Client != null)
            {
                var channelCount = service.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our Client supports.
                using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    byte[] silence = new byte[blockSize];
                    int byteCount = resampler.Read(buffer, 0, blockSize);

                    while (byteCount > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (RequestStop)
                        {
                            RequestStop = false;
                            FinishedSong();
                            return;
                        }
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
                        try
                        {
                            Client.Send(Paused ? silence : buffer, 0, blockSize); // Send the buffer to Discord
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Client = null;
                            CurrentState = AudioState.Stopped;
                            break;
                        }
                        if (!Paused)
                        {
                            byteCount = resampler.Read(buffer, 0, blockSize);
                        }
                    }
                }
                FinishedSong();
            }
        }

        private enum AudioState { Playing, Stopped }

        public delegate void OnSongChangedHandler(string title);

        private class Song
        {
            public string Title;
            public string Url;
        }
    }
}