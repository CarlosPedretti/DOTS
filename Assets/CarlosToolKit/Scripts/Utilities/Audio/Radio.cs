using QFSW.QC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(AudioSource))]
    public class Radio : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<PlayList> playLists = new List<PlayList>();
        [SerializeField] private List<AudioClip> songs = new List<AudioClip>();
        [SerializeField] private bool random = false;
        [SerializeField] private bool useDefaultAudioSourceConfig = true;


        private int currentSongIndex = 0;
        private int currentPlayListIndex = 0;
        private List<int> previousSongIndexes = new List<int>();

        public event Action OnPlaying;
        public event Action OnSongChanged;

        public event Action OnPause;
        public event Action OnStop;

        public event Action OnPlayListChanged;

        private void Start()
        {
            Initialize();
        }

        public void Play() => PlaySong(currentSongIndex);
        public void Play(int songIndex) => PlaySong(songIndex);
        public void Pause() => PauseSong();
        public void Stop() => StopSong();


        public void Next()
        {
            if (songs.Count == 0) return;
            PlaySong(GetNextSongIndex());
        }

        public void Previous()
        {
            if (previousSongIndexes.Count > 0)
            {
                int previousIndex = previousSongIndexes[^1];
                previousSongIndexes.RemoveAt(previousSongIndexes.Count - 1);
                PlaySong(previousIndex, false);
            }
        }

        public void Skip(int time)
        {
            if (ValidateAudioSource() && audioSource.clip != null)
            {
                audioSource.time = Mathf.Clamp(time, 0, (int)audioSource.clip.length);
            }
        }

        public void SelectPlayList(string playListName)
        {
            foreach (var playList in playLists)
            {
                if (string.Equals(playListName, playList.PlaylistName, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"'{playListName}' selected succesfully!");
                    songs = playList.Songs;
                    return;
                }
            }

            Debug.LogWarning($"The provided PlayList name does not exist.");
        }

        public void SetRandom(bool value)
        {
            random = value;
        }

        public void PlayListNext() => SelectPlayList(GetNextPlayListIndex());

        public void PlayListPrevious() => SelectPlayList(GetPreviousPlayListIndex());


        public PlayList GetCurrentPlayList()
        {
            return playLists[currentPlayListIndex];
        }

        public AudioClip GetCurrentSong()
        {
            return songs[currentSongIndex];
        }

        public AudioSource GetAudioSource()
        {
            return audioSource;
        }

        #region Auxiliar Methods
        private void PlaySong(int index, bool addToHistory = true)
        {
            if (!ValidateAudioSource() || songs.Count == 0) return;

            int previousSongIndex = currentSongIndex;
            currentSongIndex = CheckIndexOutOfRange(index);

            if (previousSongIndex != currentSongIndex)
            {
                OnSongChanged?.Invoke();
            }

            audioSource.clip = songs[currentSongIndex];
            audioSource.Play();

            if (addToHistory)
            {
                previousSongIndexes.Add(currentSongIndex);
            }

            OnPlaying?.Invoke();
        }

        private int GetNextSongIndex()
        {
            int nextIndex = currentSongIndex + 1;
            nextIndex = CheckIndexOutOfRange(nextIndex);
            return random ? GetRandomSongIndex() : nextIndex;
        }

        private void SelectPlayList(int index)
        {
            if (playLists == null || playLists.Count == 0) return;

            if (index < 0 || index >= playLists.Count)
            {
                Debug.LogWarning("Invalid playlist index.");
                return;
            }

            if (currentPlayListIndex != index)
            {
                currentPlayListIndex = index;
                songs = playLists[currentPlayListIndex].Songs;

                OnPlayListChanged?.Invoke();
            }

            Debug.Log($"Playlist changed to: {playLists[currentPlayListIndex].PlaylistName}");
        }

        private void PauseSong()
        {
            audioSource?.Pause();

            OnPause?.Invoke();
        }

        private void StopSong()
        {
            audioSource?.Stop();

            OnStop?.Invoke();

        }

        private int GetNextPlayListIndex()
        {
            if (playLists.Count == 0) return currentPlayListIndex;
            return (currentPlayListIndex + 1) % playLists.Count;
        }

        private int GetPreviousPlayListIndex()
        {
            if (playLists.Count == 0) return currentPlayListIndex;
            return (currentPlayListIndex - 1 + playLists.Count) % playLists.Count;
        }

        private int GetRandomSongIndex()
        {
            if (songs.Count <= 1) return currentSongIndex;

            int newIndex;
            do
            {
                newIndex = UnityEngine.Random.Range(0, songs.Count);
            } while (newIndex == currentSongIndex);

            return newIndex;
        }

        private int CheckIndexOutOfRange(int index)
        {
            if (index >= songs.Count) return 0;
            if (index < 0) return songs.Count - 1;
            return index;
        }

        private bool ValidateAudioSource()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("There is no AudioSource, you must have an audio source.");
                return false;
            }
            return true;
        }
        #endregion

        private void Initialize()
        {
            if (AudioManager.Instance != null) playLists = AudioManager.Instance.GetPlayLists();

            if (playLists.Count > 0)
            {
                songs = playLists[currentPlayListIndex].Songs;
            }


            ApplyAudioSourceConfig();
        }

        private void ApplyAudioSourceConfig()
        {
            if (audioSource == null) return;
            if (!useDefaultAudioSourceConfig) return;


            audioSource.clip = null;
            audioSource.mute = false;
            audioSource.bypassEffects = false;
            audioSource.bypassListenerEffects = false;
            audioSource.bypassReverbZones = false;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.priority = 128;
            audioSource.volume = 1f;
            audioSource.pitch = 1;
            audioSource.panStereo = 0;
            audioSource.spatialBlend = 0;
            audioSource.reverbZoneMix = 1;
            audioSource.dopplerLevel = 1;
            audioSource.spread = 0;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1;
            audioSource.maxDistance = 500;
        }
    }

    [System.Serializable]
    public class PlayList
    {
        public string PlaylistName { get; set; }
        public List<AudioClip> Songs { get; private set; }


        public PlayList(string playlistName, List<AudioClip> songs)
        {
            PlaylistName = playlistName;
            Songs = songs;
        }
    }
}
