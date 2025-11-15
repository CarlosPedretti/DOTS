using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Utilities
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Sound mainSound = new Sound();
        [SerializeField] private Sound[] sounds;

        private Dictionary<string, Sound> soundsDictionary = new Dictionary<string, Sound>();

        private bool hasBeenRandomized = false;
        private AudioClip randomClipSelected;

        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            InitMainSound();

            DictionaryInitialization();

            void InitMainSound()
            {
                if (mainSound != null)
                {
                    ApplySoundSettings(audioSource, mainSound);

                    if (mainSound.playOnAwake)
                    {
                        Play();
                    }
                }
            }
        }

        #region Public Methods


        /// <summary>
        /// Plays the main sound configured in the AudioPlayer.
        /// </summary>
        public void Play()
        {
            if (mainSound == null)
            {
                Debug.LogWarning("[AudioPlayer] There is no Main Sound to play");
                return;
            }

            ApplySoundSettings(audioSource, mainSound);

            if (mainSound.clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] There is not any clip in the main Sound: {mainSound.name}");
                return;
            }

            audioSource.Play();
        }


        /// <summary>
        /// Plays the sound with the specified name from the sound list.
        /// </summary>
        public void Play(string name)
        {
            Sound selectedSound = GetSoundByName(name);

            ApplySoundSettings(audioSource, selectedSound);

            if (selectedSound.clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] There is not any clip in the Sound: {selectedSound.name}");
                return;
            }

            audioSource.Play();
        }


        /// <summary>
        /// Plays the main sound after a specified delay (in seconds).
        /// </summary>
        /// <param name="delay">The delay time in seconds before playing the main sound.</param>
        public void PlayDelayed(float delay)
        {
            if (mainSound == null)
            {
                Debug.LogWarning("[AudioPlayer] There is no Main Sound to play");
                return;
            }

            ApplySoundSettings(audioSource, mainSound);

            if (mainSound.clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] There is not any clip in the main Sound: {mainSound.name}");
                return;
            }

            audioSource.PlayDelayed(delay);
        }

        /// <summary>
        /// Plays a specific sound by name after a specified delay (in seconds).
        /// </summary>
        /// <param name="name">The name of the sound to play.</param>
        /// <param name="delay">The delay time in seconds before playing the sound.</param>
        public void PlayDelayed(string name, float delay)
        {
            Sound selectedSound = GetSoundByName(name);

            ApplySoundSettings(audioSource, selectedSound);

            if (selectedSound.clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] There is not any clip in the Sound: {selectedSound.name}");
                return;
            }

            audioSource.PlayDelayed(delay);
        }

        /// <summary>
        /// Plays a one-shot sound by name without interrupting the current clip.
        /// Useful for short sound effects like UI clicks or impacts.
        /// </summary>
        /// <param name="name">The name of the sound to play.</param>
        /// <param name="volume">
        /// Optional custom volume multiplier (0 = use the sound's default volume).
        /// </param>
        public void PlayOneShot(string name, float volume = 0)
        {
            Sound selectedSound = GetSoundByName(name);

            if (volume == 0)
            {
                volume = selectedSound.volume;
            }

            if (selectedSound.clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] There is no clip to play for OneShot: {selectedSound.name}");
                return;
            }

            audioSource.PlayOneShot(selectedSound.clip, volume);
        }

        /// <summary>
        /// Plays the specified sound only if the AudioSource is not already playing something.
        /// </summary>
        public void PlaySoundIfNotPlaying(string name)
        {
            Sound selectedSound = GetSoundByName(name);

            if (!audioSource.isPlaying)
            {
                ApplySoundSettings(audioSource, selectedSound);

                if (selectedSound.clip == null)
                {
                    Debug.LogWarning($"[AudioPlayer] There is not any clip in the Sound: {selectedSound.name}");
                    return;
                }

                audioSource.Play();
            }
            else
            {
                Debug.Log($"[AudioPlayer] Sound '{selectedSound.name}' is already playing on {audioSource.name}");
            }
        }


        /// <summary>
        /// Stops the AudioSource if it is currently playing.
        /// </summary>
        public void Stop()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else
            {
                Debug.Log($"[AudioPlayer] There is no sound being played on {audioSource.name}");
            }
        }

        /// <summary>
        /// Pauses the currently playing sound on the AudioSource.
        /// </summary>
        public void Pause()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log($"[AudioPlayer] Paused sound on {audioSource.name}");
            }
            else
            {
                Debug.Log($"[AudioPlayer] No sound is currently playing to pause.");
            }
        }

        /// <summary>
        /// Resumes the sound on the AudioSource if it was previously paused.
        /// </summary>
        public void Unpause()
        {
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.UnPause();
                Debug.Log($"[AudioPlayer] Resumed sound on {audioSource.name}");
            }
            else
            {
                Debug.Log($"[AudioPlayer] No paused sound to resume on {audioSource.name}");
            }
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
                    Debug.LogWarning($"[AudioPlayer] The sound '{sound.name}' has been previously added. Please, check if the sound is duplicated.");
                }
            }
        }

        private Sound GetSoundByName(string name = default)
        {
            soundsDictionary.TryGetValue(name, out var selectedSound);

            if (selectedSound == null)
            {
                Debug.LogWarning($"[AudioPlayer] Sound '{name}' not found!");
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

        #endregion
    }
}
