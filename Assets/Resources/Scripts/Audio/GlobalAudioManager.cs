#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Resources.Scripts.Audio
{
    /// <summary>
    /// Глобальный аудио-менеджер.
    /// Управляет SFX, UI-звуками, фоновой музыкой (BGM) и ритмом тиков.
    /// </summary>
    public sealed class GlobalAudioManager : MonoBehaviour
    {
        #region Singleton

        public static GlobalAudioManager? Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            InitializeSfxSource();
            InitializeMusicSource();
            RegisterButtonSounds();
        }

        #endregion

        [Header("Audio Mixer (optional)")]
        [Tooltip("Главный AudioMixer проекта с группами SFX, Music и т.д.")]
        [SerializeField] private AudioMixer? audioMixer;

        [Header("Настройки SFX/UI")]
        [Tooltip("AudioSource для воспроизведения SFX и UI-звуков")]
        [SerializeField] private AudioSource? sfxSource;
        [Tooltip("Громкость для SFX/UI (0…1)")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultSfxVolume = 1f;
        [Tooltip("Mixer Group для SFX/UI")]
        [SerializeField] private AudioMixerGroup? sfxMixerGroup;

        [Header("Tick Sound (для рисования)")]
        [Tooltip("Короткий клип-тик")]
        [SerializeField] private AudioClip? drawingTickClip;
        [Tooltip("Громкость тиков (0…1)")]
        [Range(0f, 1f)]
        [SerializeField] private float drawingTickVolume = 1f;

        [Header("Настройки ритма тиков")]
        [Tooltip("Интервал между первыми тиками, сек.")]
        [SerializeField] private float startTickInterval = 1f;
        [Tooltip("Минимальный интервал под конец, сек.")]
        [SerializeField] private float endTickInterval = 0.2f;
        [Tooltip("Pitch первого тика")]
        [SerializeField] private float tickMinPitch = 1f;
        [Tooltip("Pitch последнего тика")]
        [SerializeField] private float tickMaxPitch = 1.5f;

        [Header("Настройки фоновой музыки (BGM)")]
        [Tooltip("AudioSource для фоновой музыки")]
        [SerializeField] private AudioSource? musicSource;
        [Tooltip("Плейлист фоновой музыки")]
        [SerializeField] private List<AudioClip> musicPlaylist = new();
        [Tooltip("Громкость музыки (0…1)")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;
        [Tooltip("Длительность кросс-фейда между треками")]
        [SerializeField] private float crossFadeDuration = 1f;

        private int currentTrackIndex;
        private Coroutine? musicFadeCoroutine;
        private Coroutine? tickRoutine;

        #region Initialization

        private void InitializeSfxSource()
        {
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();

            sfxSource.playOnAwake = false;
            sfxSource.volume = defaultSfxVolume;
            if (sfxMixerGroup != null)
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        private void InitializeMusicSource()
        {
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();

            musicSource.playOnAwake = false;
            musicSource.loop = false;
            musicSource.volume = 0f;

            if (audioMixer != null)
            {
                var group = audioMixer.FindMatchingGroups("Music");
                if (group.Length > 0)
                    musicSource.outputAudioMixerGroup = group[0];
            }
        }

        #endregion

        #region Button Registration

        [Header("Привязки кнопок")]
        [Tooltip("Список кнопок и звуков для клика и наведения")]
        [SerializeField] private List<ButtonAudioConfig> buttonConfigs = new();

        private void RegisterButtonSounds()
        {
            foreach (var cfg in buttonConfigs)
                cfg.Register(this);
        }

        [Serializable]
        private class ButtonAudioConfig
        {
            [Tooltip("Кнопка, на которую назначаются звуки")]
            [SerializeField] private Button? button;
            [Tooltip("Аудиоклип для клика")]
            [SerializeField] private AudioClip? clickClip;
            [Tooltip("Громкость клика")]
            [Range(0f, 1f)]
            [SerializeField] private float clickVolume;
            [Tooltip("Аудиоклип для наведения")]
            [SerializeField] private AudioClip? hoverClip;
            [Tooltip("Громкость наведения")]
            [Range(0f, 1f)]
            [SerializeField] private float hoverVolume;

            public void Register(GlobalAudioManager mgr)
            {
                if (button == null)
                    return;

                if (clickClip != null)
                    button.onClick.AddListener(() => mgr.PlaySfx(clickClip, clickVolume));

                if (hoverClip != null)
                {
                    var trig = button.GetComponent<EventTrigger>()
                               ?? button.gameObject.AddComponent<EventTrigger>();
                    var entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerEnter
                    };
                    entry.callback.AddListener(_ => mgr.PlaySfx(hoverClip, hoverVolume));
                    trig.triggers.Add(entry);
                }
            }
        }

        #endregion

        #region SFX & Tick Playback

        /// <summary>Обычный SFX/UI.</summary>
        public void PlaySfx(AudioClip clip, float? volume = null)
        {
            if (sfxSource == null)
                InitializeSfxSource();

            var src = sfxSource!;
            float vol = Mathf.Clamp01(volume ?? defaultSfxVolume);
            src.pitch = 1f;
            src.PlayOneShot(clip, vol);
        }

        /// <summary>Единичный тик (используется внутр. корутиной).</summary>
        private void PlayTickOnce(float pitch)
        {
            if (drawingTickClip == null)
                return;

            if (sfxSource == null)
                InitializeSfxSource();

            var src = sfxSource!;
            float prev = src.pitch;
            src.pitch = pitch;
            src.PlayOneShot(drawingTickClip, drawingTickVolume);
            src.pitch = prev;
        }

        /// <summary>
        /// Запускает корутину ритма тиков на длительность duration.
        /// </summary>
        public void StartTickRhythm(float duration)
        {
            if (tickRoutine != null)
                StopCoroutine(tickRoutine);

            tickRoutine = StartCoroutine(TickRhythmCoroutine(duration));
        }

        /// <summary>Останавливает ритм тиков.</summary>
        public void StopTickRhythm()
        {
            if (tickRoutine != null)
            {
                StopCoroutine(tickRoutine);
                tickRoutine = null;
            }
        }

        private IEnumerator TickRhythmCoroutine(float duration)
        {
            if (drawingTickClip == null)
                yield break;

            float start = Time.time;
            while (Time.time - start < duration)
            {
                float elapsed = Time.time - start;
                float t = Mathf.Clamp01(elapsed / duration);
                float interval = Mathf.Lerp(startTickInterval, endTickInterval, t);
                float pitch    = Mathf.Lerp(tickMinPitch,   tickMaxPitch,    t);

                PlayTickOnce(pitch);

                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>Меняет громкость SFX/UI.</summary>
        public void SetSfxVolume(float vol)
        {
            defaultSfxVolume = Mathf.Clamp01(vol);
            if (sfxSource != null)
                sfxSource.volume = defaultSfxVolume;
        }

        #endregion

        #region Music Playback

        public void PlayNextMusic() => PlayMusic((currentTrackIndex + 1) % musicPlaylist.Count);

        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
                musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
                musicSource.UnPause();
        }

        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        private void PlayMusic(int index = 0, bool loop = true)
        {
            if (musicPlaylist.Count == 0 || musicSource == null)
                return;

            index = Mathf.Clamp(index, 0, musicPlaylist.Count - 1);
            currentTrackIndex = index;

            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(FadeAndPlayMusic(musicPlaylist[index], loop));
        }

        private IEnumerator FadeAndPlayMusic(AudioClip clip, bool loop)
        {
            if (musicSource!.isPlaying)
                yield return FadeCoroutine(musicSource, musicSource.volume, 0f, crossFadeDuration / 2);

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();

            yield return FadeCoroutine(musicSource, 0f, musicVolume, crossFadeDuration / 2);
        }

        private IEnumerator FadeCoroutine(AudioSource src, float from, float to, float dur)
        {
            float e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(from, to, e / dur);
                yield return null;
            }
            src.volume = to;
        }

        #endregion
    }
}
