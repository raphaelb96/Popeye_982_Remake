// AudioManager.cs
// Centralized sound manager. Listens to game events via static C# events and plays
// the appropriate audio clip in response. Uses two separate AudioSources:
//   - musicSource: looping background music (only one track at a time)
//   - sfxSource:   one-shot sound effects (can overlap with music)
// All audio clips are assigned via the Inspector — no hardcoded paths.
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    // Plays looping background music — attach an AudioSource component set to Loop = true
    public AudioSource musicSource;
    // Plays one-shot sound effects on top of the music — Loop = false
    public AudioSource sfxSource;

    [Header("Clips")]
    // Background music during normal gameplay (assigned by Michael in Inspector)
    public AudioClip gameplayMusic;
    // Replaces gameplay music for 10 seconds when Popeye eats spinach
    public AudioClip spinachMusic;
    // Short thud played when either player lands a punch
    public AudioClip punchThud;
    // Chime played when Popeye collects a heart
    public AudioClip heartDing;
    // Crash sound played when any bottle is smashed or destroyed
    public AudioClip bottleSmash;

    // ─── EVENT SUBSCRIPTIONS ────────────────────────────────────────────────
    // Subscribe in OnEnable and unsubscribe in OnDisable to prevent memory leaks
    // if this object is disabled/re-enabled mid-game

    private void OnEnable()
    {
        GameManager.OnGameStart       += PlayGameplayMusic; // Start music after the 3s countdown
        HeartItem.OnHeartCollected    += PlayHeartDing;     // Ding on every heart collected
        BottleItem.OnBottleSmashed    += PlayBottleSmash;   // Crash on bottle destruction
        PopeyeController.OnPunch      += PlayPunchThud;     // Thud on Popeye punch
        BlutoController.OnHeavyPunch  += PlayPunchThud;     // Same thud for Bluto punch
        SpinachItem.OnSpinachEaten    += PlaySpinachMusic;  // Switch music when spinach eaten
    }

    private void OnDisable()
    {
        // Mirror of OnEnable — always unsubscribe to avoid ghost callbacks
        GameManager.OnGameStart       -= PlayGameplayMusic;
        HeartItem.OnHeartCollected    -= PlayHeartDing;
        BottleItem.OnBottleSmashed    -= PlayBottleSmash;
        PopeyeController.OnPunch      -= PlayPunchThud;
        BlutoController.OnHeavyPunch  -= PlayPunchThud;
        SpinachItem.OnSpinachEaten    -= PlaySpinachMusic;
    }

    // ─── PLAYBACK METHODS ────────────────────────────────────────────────────

    // Assign and play the standard gameplay music on the looping musicSource
    private void PlayGameplayMusic()
    {
        musicSource.clip = gameplayMusic;
        musicSource.Play(); // Restarts playback from the beginning
    }

    // Switch to spinach music for 10 seconds, then revert to normal gameplay music
    private void PlaySpinachMusic()
    {
        musicSource.clip = spinachMusic;
        musicSource.Play();
        // Invoke queues a method call after a delay (10f = 10 seconds = spinach buff duration)
        Invoke(nameof(PlayGameplayMusic), 10f);
    }

    // PlayOneShot plays a clip without interrupting the current music or other SFX
    // It spawns a temporary audio voice that runs in parallel
    private void PlayHeartDing()    => sfxSource.PlayOneShot(heartDing);
    private void PlayBottleSmash()  => sfxSource.PlayOneShot(bottleSmash);
    private void PlayPunchThud()    => sfxSource.PlayOneShot(punchThud);
}
