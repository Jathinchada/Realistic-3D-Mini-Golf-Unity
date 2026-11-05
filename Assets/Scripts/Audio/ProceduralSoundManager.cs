using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Procedurally synthesizes rich natural sound effects, realistic golf audio,
    /// and relaxing, cheerful instrumental background music:
    /// - Multi-instrument acoustic guitar, soft piano, light marimba, and nature ambiance BGM
    /// - Seamless looping with dedicated volume slider and mute/unmute control
    /// - Continuous playback that persists smoothly across hole transitions without restarting
    /// - Putter compression strikes, ball roll friction, turf/curb bounces, water splash, and cup drop fanfare
    /// </summary>
    public class ProceduralSoundManager : MonoBehaviour
    {
        public static ProceduralSoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource rollSource;

        private AudioClip puttClip;
        private AudioClip bounceClip;
        private AudioClip waterSplashClip;
        private AudioClip gravityFlipClip;
        private AudioClip zeroGEnterClip;
        private AudioClip boostClip;
        private AudioClip checkpointClip;
        private AudioClip holeCompleteClip;
        private AudioClip uiClickClip;
        private AudioClip bgmClip;
        private AudioClip rollClip;

        private const int SampleRate = 44100;
        private float musicVolume = 0.65f;
        public bool IsMusicMuted { get; private set; } = false;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            InitAudioSources();
            GenerateAudioClips();
        }

        private void InitAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.spatialBlend = 0f;
            }

            if (rollSource == null)
            {
                rollSource = gameObject.AddComponent<AudioSource>();
                rollSource.playOnAwake = false;
                rollSource.loop = true;
                rollSource.spatialBlend = 0f;
                rollSource.volume = 0f;
            }

            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.85f);
            sfxSource.volume = sfxVol;

            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.65f);
            IsMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            UpdateMusicVolume();
        }

        private void GenerateAudioClips()
        {
            puttClip = CreatePuttSound();
            bounceClip = CreateBounceSound();
            waterSplashClip = CreateWaterSplashSound();
            gravityFlipClip = CreateGravityFlipSound();
            zeroGEnterClip = CreateZeroGEnterSound();
            boostClip = CreateBoostSound();
            checkpointClip = CreateCheckpointSound();
            holeCompleteClip = CreateHoleCompleteSound();
            uiClickClip = CreateUiClickSound();

            rollClip = CreateTurfRollSound();
            if (rollSource != null)
            {
                rollSource.clip = rollClip;
            }

            // Relaxing Cheerful Instrumental Background Music (Acoustic Guitar, Soft Piano, Marimba, Nature)
            bgmClip = CreateRelaxingGolfBGM();
            if (musicSource != null)
            {
                musicSource.clip = bgmClip;
                musicSource.Play();
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            UpdateMusicVolume();
        }

        public bool ToggleMusicMute()
        {
            IsMusicMuted = !IsMusicMuted;
            PlayerPrefs.SetInt("MusicMuted", IsMusicMuted ? 1 : 0);
            UpdateMusicVolume();
            return IsMusicMuted;
        }

        private void UpdateMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = IsMusicMuted ? 0f : (musicVolume * 0.48f);
                if (!musicSource.isPlaying && !IsMusicMuted && musicSource.clip != null)
                {
                    musicSource.Play();
                }
            }
        }

        public void UpdateBallRoll(float speed)
        {
            if (rollSource == null) return;
            if (speed > 0.35f)
            {
                if (!rollSource.isPlaying) rollSource.Play();
                float sfxVol = sfxSource != null ? sfxSource.volume : 0.85f;
                rollSource.volume = Mathf.Clamp01((speed - 0.35f) / 6f) * sfxVol * 0.35f;
                rollSource.pitch = Mathf.Lerp(0.85f, 1.25f, speed / 8f);
            }
            else
            {
                if (rollSource.isPlaying) rollSource.Stop();
            }
        }

        public void PlayPutt(float powerRatio = 1f)
        {
            if (sfxSource == null || puttClip == null) return;
            sfxSource.pitch = Mathf.Lerp(0.95f, 1.15f, powerRatio);
            sfxSource.PlayOneShot(puttClip, Mathf.Clamp01(0.45f + powerRatio * 0.55f));
        }

        public void PlayBounce(float impactRatio = 1f)
        {
            if (sfxSource == null || bounceClip == null) return;
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.PlayOneShot(bounceClip, Mathf.Clamp(impactRatio * 0.75f, 0.25f, 1f));
        }

        public void PlayWaterSplash()
        {
            if (sfxSource == null || waterSplashClip == null) return;
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(waterSplashClip, 0.85f);
        }

        public void PlayGravityFlip()
        {
            if (sfxSource == null || gravityFlipClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(gravityFlipClip, 0.85f);
        }

        public void PlayZeroGEnter()
        {
            if (sfxSource == null || zeroGEnterClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(zeroGEnterClip, 0.75f);
        }

        public void PlayBoost()
        {
            if (sfxSource == null || boostClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(boostClip, 0.85f);
        }

        public void PlayCheckpoint()
        {
            if (sfxSource == null || checkpointClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(checkpointClip, 0.8f);
        }

        public void PlayHoleComplete()
        {
            if (sfxSource == null || holeCompleteClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(holeCompleteClip, 0.95f);
        }

        public void PlayUIClick()
        {
            if (sfxSource == null || uiClickClip == null) return;
            sfxSource.pitch = Random.Range(0.98f, 1.02f);
            sfxSource.PlayOneShot(uiClickClip, 0.5f);
        }

        #region Procedural Waveform & Music Synthesis

        /// <summary>
        /// Synthesizes 16 seconds of peaceful, cheerful, relaxing instrumental mini-golf music:
        /// - Acoustic guitar fingerpicked arpeggios
        /// - Soft warm grand piano chords and gentle melody
        /// - Light sunny marimba chimes
        /// - Soothing nature wind and woodland bird warbles
        /// - Seamless equal-power crossfade loop
        /// </summary>
        private AudioClip CreateRelaxingGolfBGM()
        {
            float duration = 16.0f;
            int totalSamples = (int)(SampleRate * duration);
            float[] data = new float[totalSamples];

            // 4 Measures at 60 BPM (each measure is 4.0 seconds, each beat is 1.0 second):
            // Bar 0: Cmaj7/9 (C2, G2, C3, E3, G3, B3, D4)
            // Bar 1: Am7/9   (A1, E2, A2, C3, E3, G3, B3)
            // Bar 2: Fmaj7   (F1, C2, F2, A2, C3, E3, A3)
            // Bar 3: Gsus/G7 (G1, D2, G2, B2, D3, F3, A3)

            float[][] chordPitches = new float[][]
            {
                new float[] { 65.41f, 98.00f, 130.81f, 164.81f, 196.00f, 246.94f, 293.66f, 329.63f },
                new float[] { 55.00f, 82.41f, 110.00f, 130.81f, 164.81f, 196.00f, 246.94f, 261.63f },
                new float[] { 43.65f, 65.41f, 87.31f, 110.00f, 130.81f, 164.81f, 220.00f, 261.63f },
                new float[] { 49.00f, 73.42f, 98.00f, 123.47f, 146.83f, 174.61f, 220.00f, 246.94f }
            };

            // Piano Melody Notes (TimeInBar, Frequency, Duration)
            float[][] melodyNotes = new float[][]
            {
                // Bar 0 (Cmaj7):
                new float[] { 0.5f, 329.63f, 0.8f }, // E4
                new float[] { 1.5f, 392.00f, 0.8f }, // G4
                new float[] { 2.5f, 493.88f, 0.7f }, // B4
                new float[] { 3.25f, 440.00f, 0.6f },// A4
                // Bar 1 (Am7):
                new float[] { 4.5f, 523.25f, 0.8f }, // C5
                new float[] { 5.5f, 493.88f, 0.8f }, // B4
                new float[] { 6.5f, 392.00f, 0.7f }, // G4
                new float[] { 7.25f, 329.63f, 0.6f },// E4
                // Bar 2 (Fmaj7):
                new float[] { 8.5f, 349.23f, 0.8f }, // F4
                new float[] { 9.5f, 440.00f, 0.8f }, // A4
                new float[] { 10.5f, 523.25f, 0.7f },// C5
                new float[] { 11.25f, 587.33f, 0.6f },// D5
                // Bar 3 (G7):
                new float[] { 12.5f, 493.88f, 0.8f },// B4
                new float[] { 13.5f, 392.00f, 0.8f },// G4
                new float[] { 14.5f, 293.66f, 0.7f },// D4
                new float[] { 15.25f, 329.63f, 0.6f } // E4
            };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                int bar = Mathf.Clamp((int)(t / 4.0f), 0, 3);
                float tBar = t - (bar * 4.0f);
                float sample = 0f;

                // --- 1. Acoustic Guitar Fingerpicked Arpeggio (Eighth notes every 0.5s) ---
                int arpStep = (int)(tBar / 0.5f);
                float tArp = tBar - (arpStep * 0.5f);
                int pitchIdx = arpStep % chordPitches[bar].Length;
                float guitarFreq = chordPitches[bar][pitchIdx];
                float guitarEnv = Mathf.Exp(-tArp * 4.5f);
                // Decayed harmonics of plucked nylon/steel string
                float guitarTone = (Mathf.Sin(2f * Mathf.PI * guitarFreq * t) * 0.55f) +
                                   (Mathf.Sin(2f * Mathf.PI * guitarFreq * 2f * t) * 0.28f) +
                                   (Mathf.Sin(2f * Mathf.PI * guitarFreq * 3f * t) * 0.12f);
                sample += guitarTone * guitarEnv * 0.38f;

                // Warm Bass Root (Beats 0 and 2)
                float bassFreq = chordPitches[bar][0];
                float tBass = tBar % 2.0f;
                float bassEnv = Mathf.Exp(-tBass * 2.2f);
                float bassTone = (Mathf.Sin(2f * Mathf.PI * bassFreq * t) * 0.60f) +
                                 (Mathf.Sin(2f * Mathf.PI * bassFreq * 2f * t) * 0.25f);
                sample += bassTone * bassEnv * 0.32f;

                // --- 2. Soft Piano Melody ---
                foreach (var note in melodyNotes)
                {
                    float nStart = note[0];
                    float nFreq = note[1];
                    float nDur = note[2];
                    if (t >= nStart && t < nStart + nDur)
                    {
                        float nt = t - nStart;
                        float pEnv = Mathf.Exp(-nt * 3.2f) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(nt / 0.04f));
                        float pTone = (Mathf.Sin(2f * Mathf.PI * nFreq * t) * 0.55f) +
                                      (Mathf.Sin(2f * Mathf.PI * nFreq * 2f * t) * 0.25f) +
                                      (Mathf.Sin(2f * Mathf.PI * nFreq * 3f * t) * 0.08f);
                        sample += pTone * pEnv * 0.30f;
                    }
                }

                // --- 3. Light Sunny Marimba Chimes (Syncopated upbeats at 1.75s, 3.75s) ---
                float tMarimba1 = tBar - 1.75f;
                if (tMarimba1 >= 0f && tMarimba1 < 0.45f)
                {
                    float mFreq = chordPitches[bar][5] * 2.0f; // High octave chime
                    float mEnv = Mathf.Exp(-tMarimba1 * 9.0f);
                    float mTone = (Mathf.Sin(2f * Mathf.PI * mFreq * t) * 0.65f) +
                                  (Mathf.Sin(2f * Mathf.PI * mFreq * 4f * t) * 0.20f);
                    sample += mTone * mEnv * 0.25f;
                }

                float tMarimba2 = tBar - 3.75f;
                if (tMarimba2 >= 0f && tMarimba2 < 0.45f)
                {
                    float mFreq = chordPitches[bar][6] * 2.0f;
                    float mEnv = Mathf.Exp(-tMarimba2 * 9.0f);
                    float mTone = (Mathf.Sin(2f * Mathf.PI * mFreq * t) * 0.65f) +
                                  (Mathf.Sin(2f * Mathf.PI * mFreq * 4f * t) * 0.20f);
                    sample += mTone * mEnv * 0.25f;
                }

                // --- 4. Gentle Nature Ambiance (Soft Breeze & Woodland Birdsong) ---
                float slowWindLfo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.12f * t);
                float breeze = (Mathf.Sin(2f * Mathf.PI * 92f * t) * 0.2f +
                                Mathf.Sin(2f * Mathf.PI * 135f * t) * 0.15f) * slowWindLfo;

                float birdChirp = 0f;
                // Periodic cheerful bird warbles at 3.3s and 11.4s
                if (t >= 3.2f && t <= 3.8f)
                {
                    float tb = t - 3.2f;
                    float bEnv = Mathf.Sin(Mathf.PI * (tb / 0.6f));
                    float bF = 2600f + Mathf.Sin(2f * Mathf.PI * 16f * tb) * 450f;
                    birdChirp = Mathf.Sin(2f * Mathf.PI * bF * tb) * bEnv * 0.14f;
                }
                else if (t >= 11.3f && t <= 11.9f)
                {
                    float tb = t - 11.3f;
                    float bEnv = Mathf.Sin(Mathf.PI * (tb / 0.6f));
                    float bF = 3100f + Mathf.Sin(2f * Mathf.PI * 18f * tb) * 550f;
                    birdChirp = Mathf.Sin(2f * Mathf.PI * bF * tb) * bEnv * 0.12f;
                }

                sample += (breeze * 0.22f) + birdChirp;

                data[i] = Mathf.Clamp(sample * 0.72f, -0.95f, 0.95f);
            }

            // Equal-Power Seamless Loop Crossfade (0.35 seconds at boundary)
            int fadeSamples = (int)(SampleRate * 0.35f);
            for (int j = 0; j < fadeSamples; j++)
            {
                float tFade = (float)j / fadeSamples;
                float gainStart = Mathf.Sin(tFade * Mathf.PI * 0.5f);
                float gainEnd = Mathf.Cos(tFade * Mathf.PI * 0.5f);

                int endIdx = totalSamples - fadeSamples + j;
                float blended = (data[j] * gainStart) + (data[endIdx] * gainEnd);
                data[j] = blended;
                data[endIdx] = blended;
            }

            AudioClip clip = AudioClip.Create("RelaxingMiniGolfBGM", totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateTurfRollSound()
        {
            int length = SampleRate * 2;
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float r1 = Mathf.Sin(2f * Mathf.PI * 85f * t) * 0.4f;
                float r2 = Mathf.Sin(2f * Mathf.PI * 175f * t) * 0.3f;
                float noise = (Random.value * 2f - 1f) * 0.2f;
                data[i] = (r1 + r2 + noise) * 0.4f;
            }
            AudioClip clip = AudioClip.Create("TurfRollFriction", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreatePuttSound()
        {
            int length = (int)(SampleRate * 0.12f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 85f);
                float click = Mathf.Sin(2f * Mathf.PI * 2400f * t) * 0.35f;
                float body = Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.65f;
                data[i] = (click + body) * env;
            }
            AudioClip clip = AudioClip.Create("PuttSound", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBounceSound()
        {
            int length = (int)(SampleRate * 0.08f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 95f);
                float f = Mathf.Lerp(480f, 160f, t / 0.08f);
                float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                data[i] = wave * env;
            }
            AudioClip clip = AudioClip.Create("BounceSound", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateWaterSplashSound()
        {
            int length = (int)(SampleRate * 0.65f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 7.5f);
                float bubble1 = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(320f, 180f, t) * t) * 0.4f;
                float bubble2 = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(640f, 290f, t) * t) * 0.3f;
                float noise = (Random.value * 2f - 1f) * 0.35f;
                data[i] = (bubble1 + bubble2 + noise) * env * 0.85f;
            }
            AudioClip clip = AudioClip.Create("WaterSplashSound", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateGravityFlipSound()
        {
            int length = (int)(SampleRate * 0.35f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / 0.35f));
                float f = Mathf.Lerp(220f, 880f, t / 0.35f);
                float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                data[i] = wave * env * 0.65f;
            }
            AudioClip clip = AudioClip.Create("GravityFlipChime", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateZeroGEnterSound()
        {
            int length = (int)(SampleRate * 0.25f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / 0.25f));
                float wave = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.5f +
                             Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.3f;
                data[i] = wave * env * 0.7f;
            }
            AudioClip clip = AudioClip.Create("ZeroGEnterTone", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBoostSound()
        {
            int length = (int)(SampleRate * 0.25f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 8f);
                float f = Mathf.Lerp(300f, 950f, t / 0.25f);
                float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                data[i] = wave * env * 0.8f;
            }
            AudioClip clip = AudioClip.Create("BoostSwoosh", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCheckpointSound()
        {
            int length = (int)(SampleRate * 0.3f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 10f);
                float f = t < 0.15f ? 523.25f : 783.99f;
                float wave = Mathf.Sin(2f * Mathf.PI * f * t) * 0.7f;
                data[i] = wave * env * 0.75f;
            }
            AudioClip clip = AudioClip.Create("CheckpointChime", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateHoleCompleteSound()
        {
            int length = (int)(SampleRate * 1.6f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float sample = 0f;

                if (t < 0.28f)
                {
                    float cupEnv = Mathf.Exp(-t * 18f);
                    float rattle = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f +
                                   Mathf.Sin(2f * Mathf.PI * 1400f * t) * 0.4f +
                                   (Random.value * 2f - 1f) * 0.2f;
                    sample += rattle * cupEnv * 0.8f;
                }

                if (t >= 0.25f)
                {
                    float tMusic = t - 0.25f;
                    float[] notes = { 587.33f, 739.99f, 880.00f, 1174.66f };
                    int noteIdx = Mathf.Clamp((int)(tMusic / 0.32f), 0, notes.Length - 1);
                    float noteT = tMusic - (noteIdx * 0.32f);
                    float noteEnv = Mathf.Exp(-noteT * 3.5f);
                    float wave = Mathf.Sin(2f * Mathf.PI * notes[noteIdx] * tMusic) * 0.6f +
                                 Mathf.Sin(2f * Mathf.PI * notes[noteIdx] * 2f * tMusic) * 0.2f;
                    sample += wave * noteEnv * 0.75f;
                }

                data[i] = sample;
            }
            AudioClip clip = AudioClip.Create("GolfCupSinkFanfare", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateUiClickSound()
        {
            int length = (int)(SampleRate * 0.08f);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 65f);
                float wave = Mathf.Sin(2f * Mathf.PI * 980f * t);
                data[i] = wave * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("WoodUiClick", length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion
    }
}
