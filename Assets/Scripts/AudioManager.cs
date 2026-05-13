using UnityEngine;
using System.Collections;

/// <summary>
/// Centralized audio manager for Conquer-The-Plane.
/// All sounds are synthesized procedurally — no audio files required.
/// Call AudioManager.Instance.Play(...) from anywhere.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume    = 1.0f;
    [Range(0f, 1f)] public float musicVolume     = 0.35f;
    [Range(0f, 1f)] public float sfxVolume       = 0.75f;

    // Dedicated AudioSource for background music (looping)
    private AudioSource musicSource;

    // Pool of one-shot SFX sources
    private const int SFX_POOL_SIZE = 12;
    private AudioSource[] sfxPool;
    private int sfxPoolIndex = 0;

    // Rolling sound (persistent, looping)
    private AudioSource rollSource;
    private bool isRolling = false;

    private AudioClip playerHurtClip;

    // ────────────────────────────────────────────────
    // Sound names (used as keys)
    // ────────────────────────────────────────────────
    public enum Sound
    {
        BGMusic,
        Jump,
        Land,
        Roll,
        Dash,
        DashHit,
        EnergyBolt,
        EnergyBoltHit,
        Shockwave,
        SpikePlant,
        SpikeHitEnemy,
        PlayerHurt,
        PlayerDeath,
        EnemyDeath,
        Victory,
        GameOver,
        UIClick,
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeAutomatically()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>(); // Awake() will set Instance
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Guarantee there is an AudioListener so sound works in all scenes
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                gameObject.AddComponent<AudioListener>();
            }

            playerHurtClip = Resources.Load<AudioClip>("freesound_community-zombie-bite-96528");

            BuildAudioSources();
            StartBackgroundMusic();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────

    public void Play(Sound sound, float pitchVariance = 0.05f)
    {
        if (sound == Sound.BGMusic) return; // Controlled separately
        if (sound == Sound.Roll)    { StartRoll(); return; }

        AudioClip clip = GenerateClip(sound);
        if (clip == null) return;

        AudioSource src = GetNextSFXSource();
        src.clip   = clip;
        src.volume = sfxVolume * masterVolume;
        src.pitch  = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.loop   = false;
        src.Play();
    }

    public void StopRoll()
    {
        if (rollSource != null && rollSource.isPlaying)
        {
            rollSource.Stop();
            isRolling = false;
        }
    }

    public void SetMusicVolume(float v)  { musicVolume = v; if (musicSource) musicSource.volume = v * masterVolume; }
    public void SetSFXVolume(float v)    { sfxVolume = v; }
    public void SetMasterVolume(float v) { masterVolume = v; SetMusicVolume(musicVolume); }

    // ────────────────────────────────────────────────
    // Internal setup
    // ────────────────────────────────────────────────

    void BuildAudioSources()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop   = true;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.spatialBlend = 0f; // 2D

        // SFX pool
        sfxPool = new AudioSource[SFX_POOL_SIZE];
        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].spatialBlend = 0f;
            sfxPool[i].loop = false;
        }

        // Rolling source (persistent looping)
        rollSource = gameObject.AddComponent<AudioSource>();
        rollSource.loop = true;
        rollSource.spatialBlend = 0f;
        rollSource.volume = sfxVolume * masterVolume * 0.35f;
    }

    AudioSource GetNextSFXSource()
    {
        AudioSource src = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % SFX_POOL_SIZE;
        return src;
    }

    void StartRoll()
    {
        if (isRolling) return;
        rollSource.clip   = GenerateClip(Sound.Roll);
        rollSource.volume = sfxVolume * masterVolume * 0.35f;
        rollSource.Play();
        isRolling = true;
    }

    void StartBackgroundMusic()
    {
        musicSource.clip = GenerateBGMusic();
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    // ────────────────────────────────────────────────
    // Procedural Audio Synthesis
    // ────────────────────────────────────────────────

    const int SAMPLE_RATE = 44100;

    AudioClip GenerateClip(Sound sound)
    {
        switch (sound)
        {
            case Sound.Jump:          return SynthJump();
            case Sound.Land:          return SynthLand();
            case Sound.Roll:          return SynthRoll();
            case Sound.Dash:          return SynthDash();
            case Sound.DashHit:       return SynthDashHit();
            case Sound.EnergyBolt:    return SynthEnergyBolt();
            case Sound.EnergyBoltHit: return SynthEnergyBoltHit();
            case Sound.Shockwave:     return SynthShockwave();
            case Sound.SpikePlant:    return SynthSpikePlant();
            case Sound.SpikeHitEnemy: return SynthSpikeHitEnemy();
            case Sound.PlayerHurt:    return playerHurtClip != null ? playerHurtClip : SynthPlayerHurt();
            case Sound.PlayerDeath:   return SynthPlayerDeath();
            case Sound.EnemyDeath:    return SynthEnemyDeath();
            case Sound.Victory:       return SynthVictory();
            case Sound.GameOver:      return SynthGameOver();
            case Sound.UIClick:       return SynthUIClick();
            default:                  return null;
        }
    }

    // Helper: build an AudioClip from a sample array
    static AudioClip MakeClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SAMPLE_RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Helper: apply an amplitude envelope (attack, decay, sustain, release)
    static float ADSREnvelope(float t, float dur, float attack, float decay, float sustain, float release)
    {
        float rel = dur - release;
        if (t < attack)                  return t / attack;
        if (t < attack + decay)          return Mathf.Lerp(1f, sustain, (t - attack) / decay);
        if (t < rel)                     return sustain;
        return sustain * (1f - (t - rel) / release);
    }

    // Helper: white noise sample
    static float Noise() => Random.Range(-1f, 1f);

    // ──────────────────────────────────
    // Background Music
    // A layered, atmospheric electronic battle theme
    // ──────────────────────────────────
    AudioClip GenerateBGMusic()
    {
        float bpm    = 128f;
        float beat   = 60f / bpm;
        float barLen = beat * 4f;
        float totalDuration = barLen * 8f; // 8-bar loop

        int totalSamples = (int)(totalDuration * SAMPLE_RATE);
        float[] data = new float[totalSamples];

        // Bass note pattern (root + fifth alternating)
        float[] bassNotes = { 65.4f, 82.4f, 65.4f, 73.4f, 65.4f, 82.4f, 73.4f, 55f };

        // Pad chord (rich detuned saw layers)
        float[] chordFreqs = { 130.8f, 164.8f, 196f, 261.6f };

        for (int i = 0; i < totalSamples; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float bar = t / barLen;
            float beatInBar = (t % barLen) / beat;

            float sample = 0f;

            // ── Bass synth ──
            int bassIdx = (int)(bar) % bassNotes.Length;
            float bassFreq = bassNotes[bassIdx];
            float bassEnv  = Mathf.Exp(-((t % barLen) / barLen) * 4f); // decay per bar
            float bassSaw  = 2f * ((t * bassFreq) % 1f) - 1f;
            float bassOct  = 2f * ((t * bassFreq * 2f) % 1f) - 1f;
            sample += (bassSaw * 0.5f + bassOct * 0.25f) * bassEnv * 0.28f;

            // ── Pad (atmospheric) ──
            float padEnv = 0.12f;
            foreach (float f in chordFreqs)
            {
                // Detuned pair for width
                sample += Mathf.Sin(2f * Mathf.PI * f * t)              * padEnv * 0.5f;
                sample += Mathf.Sin(2f * Mathf.PI * (f * 1.003f) * t)   * padEnv * 0.5f;
            }

            // ── Kick drum (every beat on beat 1 and 3) ──
            float tInBeat = t % beat;
            if (beatInBar < 0.05f || (beatInBar >= 2f && beatInBar < 2.05f))
            {
                float kick = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 40f, tInBeat / 0.15f) * tInBeat);
                float kickEnv = Mathf.Exp(-tInBeat * 25f);
                sample += kick * kickEnv * 0.5f;
            }

            // ── Hi-hat (every half-beat) ──
            float tHat = t % (beat * 0.5f);
            float hatEnv = Mathf.Exp(-tHat * 80f);
            sample += Noise() * hatEnv * 0.12f;

            // ── Snare (beat 2 and 4) ──
            float snareTime = t % (beat * 2f);
            if (snareTime > beat - 0.01f && snareTime < beat + 0.08f)
            {
                float snareT   = snareTime - beat;
                float snareEnv = Mathf.Exp(-snareT * 35f);
                sample += (Noise() * 0.5f + Mathf.Sin(2f * Mathf.PI * 200f * snareT) * 0.5f) * snareEnv * 0.35f;
            }

            // ── Arpeggio lead ──
            float[] arpNotes = { 261.6f, 329.6f, 392f, 523.3f, 392f, 329.6f };
            float arpStep  = beat / 3f;
            int   arpIdx   = (int)(t / arpStep) % arpNotes.Length;
            float arpFreq  = arpNotes[arpIdx];
            float tArp     = t % arpStep;
            float arpEnv   = Mathf.Exp(-tArp * 8f);
            sample += Mathf.Sin(2f * Mathf.PI * arpFreq * t) * arpEnv * 0.10f;

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        return MakeClip("BGMusic", data);
    }

    // ──────────────────────────────────
    // Jump — rising frequency chirp
    // ──────────────────────────────────
    AudioClip SynthJump()
    {
        float dur = 0.25f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(220f, 660f, t / dur);
            float env  = ADSREnvelope(t, dur, 0.01f, 0.02f, 0.7f, 0.2f);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
        }
        return MakeClip("Jump", d);
    }

    // ──────────────────────────────────
    // Land — short thud
    // ──────────────────────────────────
    AudioClip SynthLand()
    {
        float dur = 0.15f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(120f, 40f, t / dur);
            float env  = Mathf.Exp(-t * 30f);
            float noise = Noise() * 0.3f;
            d[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + noise) * env * 0.75f;
        }
        return MakeClip("Land", d);
    }

    // ──────────────────────────────────
    // Roll — textured low rumble loop
    // ──────────────────────────────────
    AudioClip SynthRoll()
    {
        // ~0.5 s loop that tiles seamlessly
        float dur = 0.5f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];

        // Pseudo-random state so each layer is independent
        System.Random rng = new System.Random(42);

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SAMPLE_RATE;

            // --- Layer 1: gritty surface texture (band-passed noise) ---
            // Simulate band-pass by mixing two sine-modulated noise samples
            float grain = ((float)rng.NextDouble() * 2f - 1f);
            float band  = grain * Mathf.Sin(2f * Mathf.PI * 180f * t)   // ~180 Hz carrier
                        + grain * Mathf.Sin(2f * Mathf.PI * 320f * t);  // upper harmonic
            band *= 0.18f;

            // --- Layer 2: deep 80 Hz rumble ---
            float rumble = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.22f;

            // --- Layer 3: rhythmic 8 Hz contact bumps (sphere seam hitting floor) ---
            // Creates a subtle periodic "thump thump" pattern
            float bumpPhase = (t * 8f) % 1f;          // 8 bumps per second
            float bump      = Mathf.Exp(-bumpPhase * 14f)  // sharp attack, quick decay
                            * Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.28f;

            // --- Layer 4: very faint high-freq scraping noise ---
            float scrape = ((float)rng.NextDouble() * 2f - 1f)
                         * Mathf.Sin(2f * Mathf.PI * 900f * t) * 0.06f;

            d[i] = (band + rumble + bump + scrape) * 0.55f;
        }

        // Crossfade loop boundaries (20 ms) for seamless looping
        int fade = SAMPLE_RATE / 50;
        for (int i = 0; i < fade; i++)
        {
            float f = (float)i / fade;
            d[i]         *= f;
            d[n - 1 - i] *= f;
        }
        return MakeClip("Roll", d);
    }

    // ──────────────────────────────────
    // Dash — fast whoosh
    // ──────────────────────────────────
    AudioClip SynthDash()
    {
        float dur = 0.3f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t    = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(800f, 200f, t / dur); // falling pitch
            float env  = Mathf.Exp(-t * 6f);
            float saw  = 2f * ((t * freq) % 1f) - 1f;
            d[i] = (saw * 0.4f + Noise() * 0.4f) * env * 0.7f;
        }
        return MakeClip("Dash", d);
    }

    // ──────────────────────────────────
    // Dash Hit — meaty impact
    // ──────────────────────────────────
    AudioClip SynthDashHit()
    {
        float dur = 0.2f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 25f);
            float low = Mathf.Sin(2f * Mathf.PI * 80f * t);
            d[i] = (low * 0.5f + Noise() * 0.6f) * env * 0.85f;
        }
        return MakeClip("DashHit", d);
    }

    // ──────────────────────────────────
    // Energy Bolt — electric zap fire
    // ──────────────────────────────────
    AudioClip SynthEnergyBolt()
    {
        float dur = 0.25f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t    = (float)i / SAMPLE_RATE;
            float freq = 600f + Mathf.Sin(t * 80f) * 200f; // wobbly pitch
            float env  = ADSREnvelope(t, dur, 0.005f, 0.05f, 0.6f, 0.15f);
            phase += freq / SAMPLE_RATE;
            d[i] = (Mathf.Sin(2f * Mathf.PI * phase) + Noise() * 0.3f) * env * 0.65f;
        }
        return MakeClip("EnergyBolt", d);
    }

    // ──────────────────────────────────
    // Energy Bolt Hit — crackling zap
    // ──────────────────────────────────
    AudioClip SynthEnergyBoltHit()
    {
        float dur = 0.2f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 20f);
            float crackle = Noise() * (Mathf.Sin(t * 300f) > 0 ? 1f : -1f);
            d[i] = crackle * env * 0.7f;
        }
        return MakeClip("EnergyBoltHit", d);
    }

    // ──────────────────────────────────
    // Shockwave — big boomy explosion
    // ──────────────────────────────────
    AudioClip SynthShockwave()
    {
        float dur = 0.6f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 7f);
            float freq = Mathf.Lerp(200f, 40f, t / dur);
            float boom = Mathf.Sin(2f * Mathf.PI * freq * t);
            d[i] = (boom * 0.5f + Noise() * 0.5f) * env * 0.9f;
        }
        return MakeClip("Shockwave", d);
    }

    // ──────────────────────────────────
    // Spike Plant — crystalline ting
    // ──────────────────────────────────
    AudioClip SynthSpikePlant()
    {
        float dur = 0.4f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 10f);
            // Metallic bell: sum of harmonics
            float bell = Mathf.Sin(2f * Mathf.PI * 880f * t)    * 0.5f
                       + Mathf.Sin(2f * Mathf.PI * 1320f * t)   * 0.3f
                       + Mathf.Sin(2f * Mathf.PI * 2200f * t)   * 0.15f;
            d[i] = bell * env * 0.6f;
        }
        return MakeClip("SpikePlant", d);
    }

    // ──────────────────────────────────
    // Spike Hit Enemy — crunch zap
    // ──────────────────────────────────
    AudioClip SynthSpikeHitEnemy()
    {
        float dur = 0.15f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 30f);
            d[i] = (Noise() * 0.5f + Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.5f) * env * 0.65f;
        }
        return MakeClip("SpikeHitEnemy", d);
    }

    // ──────────────────────────────────
    // Player Hurt — low grunt thud
    // ──────────────────────────────────
    AudioClip SynthPlayerHurt()
    {
        float dur = 0.3f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 12f);
            float freq = Mathf.Lerp(300f, 150f, t / dur);
            d[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + Noise() * 0.25f) * env * 0.7f;
        }
        return MakeClip("PlayerHurt", d);
    }

    // ──────────────────────────────────
    // Player Death — dramatic descending tone
    // ──────────────────────────────────
    AudioClip SynthPlayerDeath()
    {
        float dur = 1.2f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t    = (float)i / SAMPLE_RATE;
            float freq = Mathf.Lerp(400f, 80f, t / dur);
            float env  = Mathf.Exp(-t * 3f);
            phase += freq / SAMPLE_RATE;
            d[i] = (Mathf.Sin(2f * Mathf.PI * phase) * 0.6f + Noise() * 0.2f) * env * 0.8f;
        }
        return MakeClip("PlayerDeath", d);
    }

    // ──────────────────────────────────
    // Enemy Death — pop + crunch
    // ──────────────────────────────────
    AudioClip SynthEnemyDeath()
    {
        float dur = 0.35f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SAMPLE_RATE;
            float env = Mathf.Exp(-t * 15f);
            float pop = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(500f, 100f, t / dur) * t);
            d[i] = (pop * 0.5f + Noise() * 0.5f) * env * 0.65f;
        }
        return MakeClip("EnemyDeath", d);
    }

    // ──────────────────────────────────
    // Victory — ascending bright fanfare
    // ──────────────────────────────────
    AudioClip SynthVictory()
    {
        float dur = 2.0f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];

        float[] notes     = { 261.6f, 329.6f, 392f, 523.3f, 659.3f, 783.9f };
        float   noteDur   = dur / notes.Length;

        for (int i = 0; i < n; i++)
        {
            float t       = (float)i / SAMPLE_RATE;
            int   noteIdx = Mathf.Min((int)(t / noteDur), notes.Length - 1);
            float tNote   = t - noteIdx * noteDur;
            float env     = Mathf.Exp(-tNote * 5f);

            float freq = notes[noteIdx];
            d[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f +
                    Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f) * env * 0.65f;
        }
        return MakeClip("Victory", d);
    }

    // ──────────────────────────────────
    // Game Over — descending minor chord
    // ──────────────────────────────────
    AudioClip SynthGameOver()
    {
        float dur = 2.0f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];

        float[] notes   = { 440f, 370f, 311f, 233f };
        float   noteDur = dur / notes.Length;

        for (int i = 0; i < n; i++)
        {
            float t       = (float)i / SAMPLE_RATE;
            int   noteIdx = Mathf.Min((int)(t / noteDur), notes.Length - 1);
            float tNote   = t - noteIdx * noteDur;
            float env     = Mathf.Exp(-tNote * 3.5f);
            float freq    = notes[noteIdx];
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.65f;
        }
        return MakeClip("GameOver", d);
    }

    // ──────────────────────────────────
    // UI Click — short tick
    // ──────────────────────────────────
    AudioClip SynthUIClick()
    {
        float dur = 0.05f;
        int n     = (int)(dur * SAMPLE_RATE);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            d[i] = Mathf.Sin(2f * Mathf.PI * 1200f * t) * Mathf.Exp(-t * 80f) * 0.5f;
        }
        return MakeClip("UIClick", d);
    }
}
