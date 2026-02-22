using UnityEngine;
using System.Collections;

namespace IndAssets.Scripts.Audio
{
    public class PvMnAudioManager : MonoBehaviour
    {
        public static PvMnAudioManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource; // BGM only
        [SerializeField] private AudioSource sfxSource;   // Sounds for single effect

        [Header("Settings")]
        [SerializeField] private float defaultFadeDuration = 1.0f;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- BGM Logic ---

        /// <summary>
        /// Play BGM (fade in fade out)
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip)
            {
                // If same clip, then no change
                return;
            }

            StartCoroutine(FadeTrack(clip, loop));
        }

        private IEnumerator FadeTrack(AudioClip newClip, bool loop)
        {
            float time = 0;
            float startVolume = musicSource.volume;

            // Fade out music
            while (time < defaultFadeDuration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, time / defaultFadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in music
            time = 0;
            while (time < defaultFadeDuration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, startVolume, time / defaultFadeDuration);
                yield return null;
            }
        }

        // --- SFX Logic ---

        /// <summary>
        /// Single Sound like "Explode", "Attack", etc.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Play sound at specific position
        /// </summary>
        public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}