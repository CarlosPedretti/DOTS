using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using System.IO;
using UnityEngine.Networking;
using System.Linq;

namespace Utilities
{
    public class AudioManager : Singleton<AudioManager>
    {
        public Sound[] sounds;
        private Dictionary<string, Sound> soundsDictionary = new Dictionary<string, Sound>();

        private AudioClip randomClipSelected;
        private bool hasBeenRandomized = false;

        [SerializeField] private bool useRadio = false;
        [SerializeField] private Radio radio;

        public Radio Radio { get { return radio; } private set { } }


        protected override void Awake()
        {
            base.Awake();

            DictionaryInitialization();

            CreateMusicFolderPath();
        }

        #region Public Methods

        public void Play(string name, GameObject sourceObject)
        {
            AudioSource source = GetOrAddAudioSource(sourceObject);

            Sound selectedSound = GetSoundByName(name);

            ApplySoundSettings(source, selectedSound);

            if (source.clip == null)
            {
                Debug.LogWarning($"There is not any clip in the Sound: {selectedSound.name}");
                return;
            }

            source.Play();
        }

        public void PlaySoundIfNotPlaying(string name, GameObject sourceObject)
        {
            AudioSource source = GetOrAddAudioSource(sourceObject);

            Sound selectedSound = GetSoundByName(name);

            if (!source.isPlaying)
            {
                ApplySoundSettings(source, selectedSound);

                if (source.clip == null)
                {
                    Debug.LogWarning($"There is not any clip in the Sound: {selectedSound.name}");
                    return;
                }

                source.Play();
            }
            else
            {
                Debug.Log($"Sound '{selectedSound.name}' is already playing on {sourceObject.name}");
            }
        }

        public void Stop(GameObject sourceObject)
        {
            AudioSource source = sourceObject.GetComponent<AudioSource>();

            if (source != null)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
                else
                {
                    Debug.Log($"There is no sound being played on {sourceObject.name}");
                }
            }
        }

        public void PlayWithTimer(string name, float duration, GameObject sourceObject)
        {
            AudioSource source = GetOrAddAudioSource(sourceObject);

            Sound selectedSound = GetSoundByName(name);

            ApplySoundSettings(source, selectedSound);

            if (source.clip == null)
            {
                Debug.LogWarning($"There is not any clip in the Sound: {selectedSound.name}");
                return;
            }

            source.loop = true;

            source.Play();

            StartCoroutine(DisableLoopAfterTime(source, duration));
        }

        public void PlayWithForcedTimer(string name, float duration, GameObject sourceObject)
        {
            AudioSource source = GetOrAddAudioSource(sourceObject);

            Sound selectedSound = GetSoundByName(name);

            ApplySoundSettings(source, selectedSound);

            if (source.clip == null)
            {
                Debug.LogWarning($"There is not any clip in the Sound: {selectedSound.name}");
                return;
            }

            source.loop = true;

            source.Play();

            StartCoroutine(ForceStopAfterTime(source, duration));
        }


        //UI
        public void PlayUI(string name)
        {
            Play(name, this.gameObject);
        }

        public void PalySoundIfNotPlayingUI(string name)
        {
            PlaySoundIfNotPlaying(name, this.gameObject);
        }

        public void StopUI()
        {
            Stop(gameObject);
        }

        public void PlayWithTimerUI(string name, float duration)
        {
            PlayWithTimer(name, duration, this.gameObject);
        }

        public void PlayWithForcedTimerUI(string name, float duration)
        {
            PlayWithForcedTimer(name, duration, this.gameObject);
        }

        #endregion




        #region Private Methods

        private void DictionaryInitialization()
        {
            if (sounds.Length == 0 || sounds == null) return;

            foreach (Sound sound in sounds)
            {
                if (!soundsDictionary.ContainsKey(sound.name))
                {
                    soundsDictionary.Add(sound.name, sound);
                }
                else
                {
                    Debug.LogWarning($"The sound '{sound.name}' has been previously added. Please, check if the sound is duplicated. ");
                }
            }
        }

        private AudioSource GetOrAddAudioSource(GameObject sourceObject)
        {
            AudioSource source = sourceObject.GetComponent<AudioSource>();

            if (source == null)
            {
                source = sourceObject.AddComponent<AudioSource>();
            }

            return source;
        }

        private Sound GetSoundByName(string name)
        {
            soundsDictionary.TryGetValue(name, out var selectedSound);

            if (selectedSound == null)
            {
                Debug.LogWarning($"Sound '{name}' not found!");
            }

            return selectedSound;
        }

        private void ApplySoundSettings(AudioSource source, Sound sound)
        {
            source.clip = sound.clip;
            source.outputAudioMixerGroup = sound.group;
            source.mute = sound.mute;
            source.bypassEffects = sound.bypassEffects;
            source.bypassListenerEffects = sound.bypassListenerEffects;
            source.bypassReverbZones = sound.bypassReverbZones;
            source.playOnAwake = sound.playOnAwake;
            source.loop = sound.loop;
            source.priority = sound.priority;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.panStereo = sound.stereoPan;
            source.spatialBlend = sound.spatialBlend;
            source.reverbZoneMix = sound.reverbZoneMix;
            source.dopplerLevel = sound.dopplerLevel;
            source.spread = sound.spread;
            source.rolloffMode = sound.rolloffMode;
            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;

            SlightPitchVariation(source, sound);

            PitchRandomization(source, sound);

            ClipRandomization(source, sound);

        }

        private void SlightPitchVariation(AudioSource source, Sound sound)
        {
            if (sound.slightPitchVariation)
            {
                source.pitch = Random.Range(sound.pitch * 0.95f, sound.pitch * 1.05f);
            }
        }

        private void PitchRandomization(AudioSource source, Sound sound)
        {
            if (sound.randomizePitch)
            {
                source.pitch = Random.Range(sound.randomPitchMin, sound.randomPitchMax);
            }
        }

        private void ClipRandomization(AudioSource source, Sound sound)
        {
            if (sound.useClipRandomization && sound.clips != null && sound.clips.Length > 0)
            {
                if (sound.selectARandomClipOnce)
                {
                    if (!hasBeenRandomized)
                    {
                        int randomIndex = Random.Range(0, sound.clips.Length);
                        randomClipSelected = sound.clips[randomIndex];

                        hasBeenRandomized = true;
                    }
                }
                else
                {
                    int randomIndex = Random.Range(0, sound.clips.Length);
                    randomClipSelected = sound.clips[randomIndex];
                }

                source.clip = randomClipSelected;
            }
        }

        private IEnumerator DisableLoopAfterTime(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.loop = false;
        }

        private IEnumerator ForceStopAfterTime(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.Stop();
        }

        #endregion

        #region Radio
        private string musicFolderPath;
        private List<PlayList> playListsList = new List<PlayList>();

        void CreateMusicFolderPath()
        {
            if (!useRadio) return;

            musicFolderPath = Path.Combine(Application.persistentDataPath, "Music");

            if (!Directory.Exists(musicFolderPath))
            {
                Directory.CreateDirectory(musicFolderPath);
                Debug.Log($"Music folder created at: {musicFolderPath}");
            }

            LoadPlaylists();
        }

        void LoadPlaylists()
        {
            playListsList.Clear();

            LoadOfficialSongs();

            LoadUserPlaylists();
        }

        void LoadOfficialSongs()
        {
            AudioClip[] officialSongs = Resources.LoadAll<AudioClip>("OfficialSongs");
            if (officialSongs.Length > 0)
            {
                playListsList.Add(new PlayList("Official Songs", officialSongs.ToList()));
                Debug.Log($"Loaded {officialSongs.Length} official songs.");
            }
            else
            {
                Debug.LogWarning("No official songs found in Resources/OfficialSongs.");
            }
        }

        void LoadUserPlaylists()
        {
            if (!Directory.Exists(musicFolderPath))
            {
                Debug.LogError("Music folder not found: " + musicFolderPath);
                return;
            }

            string[] subfolders = Directory.GetDirectories(musicFolderPath);

            foreach (string folder in subfolders)
            {
                string playlistName = Path.GetFileName(folder);
                string[] audioFiles = Directory.GetFiles(folder, "*.*")
                    .Where(file => file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".ogg"))
                    .ToArray();

                StartCoroutine(LoadAudioFiles(playlistName, audioFiles));
            }
        }

        IEnumerator LoadAudioFiles(string playlistName, string[] audioFiles)
        {
            List<AudioClip> songs = new List<AudioClip>();

            foreach (string file in audioFiles)
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + file, AudioType.MPEG))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to load {file}: {www.error}");
                        continue;
                    }

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.name = Path.GetFileNameWithoutExtension(file);
                    songs.Add(clip);
                }
            }

            if (songs.Count > 0)
            {
                playListsList.Add(new PlayList(playlistName, songs));
                Debug.Log($"Playlist '{playlistName}' loaded with {songs.Count} songs.");
            }
        }

        public void OpenMusicFolder()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Music");

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Music folder does not exist at: " + folderPath);
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(folderPath);
#else
        Application.OpenURL("file://" + folderPath);
#endif
        }

        public List<PlayList> GetPlayLists()
        {
            return playListsList ?? new List<PlayList>();
        }

        #endregion



        private void TestSound(string soundName)
        {
            Play(soundName, gameObject);
        }


    }

    public static class VolumeKeys
    {
        public const string MASTER_GROUP_KEY = "MasterVolume";
        public const string MENU_GROUP_KEY = "MenuVolume";
        public const string SFX_GROUP_KEY = "SFXVolume";
        public const string MUSIC_GROUP_KEY = "MusicVolume";
    }
}

