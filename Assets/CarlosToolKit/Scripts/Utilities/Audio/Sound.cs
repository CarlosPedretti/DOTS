using UnityEngine;
using UnityEngine.Audio;

namespace Utilities
{
    [System.Serializable]
    public class Sound
    {
        [Header("General Settings")]
        [Tooltip("Unique name identifier for this sound.")]
        public string name;

        [Tooltip("Audio clip to be played.")]
        public AudioClip clip;

        [Tooltip("Audio mixer group this sound belongs to.")]
        public AudioMixerGroup group;


        [Header("Playback Settings")]
        [Tooltip("If true, the sound will start playing automatically when the object awakens.")]
        public bool playOnAwake = false;

        [Tooltip("If true, the sound will loop continuously until stopped.")]
        public bool loop = false;


        [Header("Bypass Options")]
        [Tooltip("Mutes the audio without disabling the AudioSource.")]
        public bool mute = false;

        [Tooltip("If true, bypasses all audio effects applied to this AudioSource.")]
        public bool bypassEffects = false;

        [Tooltip("If true, bypasses audio listener effects.")]
        public bool bypassListenerEffects = false;

        [Tooltip("If true, bypasses reverb zones.")]
        public bool bypassReverbZones = false;


        [Header("Volume & Pitch")]
        [Tooltip("Determines the priority of this sound when many AudioSources are playing. Lower value = higher priority.")]
        [Range(0, 256)]
        public int priority = 128;

        [Tooltip("Volume of the sound (0 = silent, 1 = full volume).")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Pitch multiplier of the sound. Negative values reverse playback.")]
        [Range(-3f, 3f)]
        public float pitch = 1f;


        [Header("Stereo & Spatial Settings")]
        [Tooltip("Pans the sound left (-1) to right (1). 0 plays centered.")]
        [Range(0f, 1f)]
        public float stereoPan = 0f;

        [Tooltip("Blends between 2D (0) and 3D (1) sound.")]
        [Range(0f, 1f)]
        public float spatialBlend = 0f;

        [Tooltip("Amount of the sound sent to reverb zones (0 = none, 1 = full, can go slightly above 1).")]
        [Range(0f, 1.1f)]
        public float reverbZoneMix = 1f;

        [Tooltip("Controls doppler effect strength. Higher values exaggerate pitch changes when moving.")]
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;

        [Tooltip("Spreads the sound in 3D space (0 = mono, 360 = omnidirectional).")]
        [Range(0, 360)]
        public int spread = 0;

        [Tooltip("Controls how volume decreases with distance.")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Tooltip("Minimum distance where the sound is at full volume.")]
        public float minDistance = 1f;

        [Tooltip("Maximum distance where the sound can be heard.")]
        public float maxDistance = 500f;


        [Header("Pitch Randomization")]
        [Tooltip("If enabled, pitch will be randomized using the settings below.")]
        public bool randomizePitch = false;

        [Tooltip("If true, applies a slight pitch variation instead of the full min/max range.")]
        public bool slightPitchVariation = false;

        [Tooltip("Minimum random pitch multiplier.")]
        [Range(-3f, 3f)]
        public float randomPitchMin = 1f;

        [Tooltip("Maximum random pitch multiplier.")]
        [Range(-3f, 3f)]
        public float randomPitchMax = 1f;


        [Header("Clip Randomization")]
        [Tooltip("If enabled, the sound will randomly pick from the provided AudioClips.")]
        public bool useClipRandomization;

        [Tooltip("If true, a single random clip will be chosen once and reused, instead of re-randomizing every time.")]
        public bool selectARandomClipOnce = false;

        [Tooltip("List of audio clips to use for randomization.")]
        public AudioClip[] clips;

        public Sound()
        {
            // defaults
            playOnAwake = false;
            loop = false;
            priority = 128;
            volume = 1f;
            pitch = 1f;
            stereoPan = 0f;
            spatialBlend = 0f;
            reverbZoneMix = 1f;
            dopplerLevel = 1f;
            spread = 0;
            rolloffMode = AudioRolloffMode.Logarithmic;
            minDistance = 1f;
            maxDistance = 500f;
            randomPitchMin = 1f;
            randomPitchMax = 1f;
        }
    }

}
