using System.Collections.Generic;
using UnityEngine;

namespace NutriPick.Utilities
{
    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }

        private AudioSource source;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            source = GetComponent<AudioSource>();
            if (source == null)
                source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        public void PlayHover() => Play("hover", 880f, 0.05f, 0.12f);
        public void PlayPickup() => Play("pickup", 220f, 0.12f, 0.25f);
        public void PlayPlace() => Play("place", 440f, 0.10f, 0.25f);
        public void PlayDrop() => Play("drop", 180f, 0.10f, 0.18f);
        public void PlayServe() => Play("serve", 660f, 0.25f, 0.30f);
        public void PlayReport() => Play("report", 520f, 0.20f, 0.22f);

        private void Play(string key, float frequency, float duration, float volume)
        {
            if (!clips.TryGetValue(key, out var clip))
            {
                clip = CreateTone(frequency, duration);
                clips[key] = clip;
            }

            if (source != null && clip != null)
                source.PlayOneShot(clip, volume);
        }

        private static AudioClip CreateTone(float frequency, float duration)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.35f;
            }

            var clip = AudioClip.Create("NutriPickTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
