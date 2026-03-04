using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Core
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioEntry[] musicTracks;
        [SerializeField] private AudioEntry[] sfxClips;

        [System.Serializable]
        public class AudioEntry
        {
            public string id;
            public AudioClip clip;
        }

        private Dictionary<string, AudioClip> _musicDict;
        private Dictionary<string, AudioClip> _sfxDict;

        private void Awake()
        {
            _musicDict = new Dictionary<string, AudioClip>();
            foreach (var entry in musicTracks)
                if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    _musicDict[entry.id] = entry.clip;

            _sfxDict = new Dictionary<string, AudioClip>();
            foreach (var entry in sfxClips)
                if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    _sfxDict[entry.id] = entry.clip;
        }

        public void PlayMusic(string musicId, bool loop = true)
        {
            if (!_musicDict.TryGetValue(musicId, out var clip))
            {
                Debug.LogWarning($"[AudioService] Müzik bulunamadý: {musicId}");
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            StartCoroutine(CrossFadeMusic(clip, loop));
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }

        public void PlaySFX(string sfxId)
        {
            if (!_sfxDict.TryGetValue(sfxId, out var clip))
            {
                Debug.LogWarning($"[AudioService] SFX bulunamadý: {sfxId}");
                return;
            }
            sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        private IEnumerator CrossFadeMusic(AudioClip newClip, bool loop)
        {
            float duration = 0.5f;
            float startVolume = musicSource.volume;

            // Fade out
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / duration);
                yield return null;
            }

            musicSource.volume = startVolume;
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }
    }
}