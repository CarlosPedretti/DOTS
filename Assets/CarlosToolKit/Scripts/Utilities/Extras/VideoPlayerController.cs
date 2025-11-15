using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Utilities
{
    [RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
    public class VideoPlayerController : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        public VideoPlayer VideoPlayer
        {
            get
            {
                if (videoPlayer == null)
                {
                    videoPlayer = GetComponent<VideoPlayer>();
                    if (videoPlayer == null)
                    {
                        Debug.LogError("No VideoPlayer component found on this GameObject.");
                    }
                }
                return videoPlayer;
            }
        }
        private AudioSource audioSource;

        [Header("UI Controls (Optional)")]
        public Slider volumeSlider;
        public Toggle muteToggle;

        [Header("Video Events")]
        public UnityEvent OnPlayVideo;
        public UnityEvent OnPauseVideo;
        public UnityEvent OnVideoEnd;

        void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = GetComponent<AudioSource>();

            VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            VideoPlayer.SetTargetAudioSource(0, audioSource);

            VideoPlayer.loopPointReached += HandleVideoEnd;
        }

        void Start()
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = audioSource.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            if (muteToggle != null)
            {
                muteToggle.isOn = audioSource.mute;
                muteToggle.onValueChanged.AddListener(SetMute);
            }
        }

        /// <summary>
        /// Plays the video from the current position.
        /// </summary>
        public void PlayVideo()
        {
            VideoPlayer.Play();
            OnPlayVideo?.Invoke();
        }

        /// <summary>
        /// Pauses the video playback.
        /// </summary>
        public void PauseVideo()
        {
            VideoPlayer.Pause();
            OnPauseVideo?.Invoke();
        }

        /// <summary>
        /// Stops and restarts the video from the beginning.
        /// </summary>
        public void RestartVideo()
        {
            if (VideoPlayer.isPlaying)
            {
                VideoPlayer.Stop();
            }

            VideoPlayer.Play();
            OnPlayVideo?.Invoke();
        }

        /// <summary>
        /// Mutes or unmutes the video's audio.
        /// </summary>
        /// <param name="isMuted">Determines whether the video should be muted.</param>
        public void SetMute(bool isMuted)
        {
            audioSource.mute = isMuted;
        }

        /// <summary>
        /// Adjusts the video volume.
        /// </summary>
        /// <param name="volume">Volume level between 0 (mute) and 1 (max).</param>
        public void SetVolume(float volume)
        {
            audioSource.volume = volume;
        }

        /// <summary>
        /// Toggles between playing and pausing the video.
        /// </summary>
        public void TogglePlayPause()
        {
            if (VideoPlayer.isPlaying)
            {
                PauseVideo();
            }
            else
            {
                PlayVideo();
            }
        }

        private void HandleVideoEnd(VideoPlayer vp)
        {
            OnVideoEnd?.Invoke();
        }
    }
}
