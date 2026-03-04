namespace StoryGame.Core
{
    public interface IAudioService
    {
        void PlayMusic(string musicId, bool loop = true);
        void StopMusic(float fadeDuration = 1f);
        void PlaySFX(string sfxId);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
    }
}