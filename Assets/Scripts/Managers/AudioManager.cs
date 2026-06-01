// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource variedSfxSource;

    [Header("Music Clips")]
    [Tooltip("Add as many tracks here as you want. One will be picked randomly per match.")]
    public AudioClip[] gameplayMusicTracks;
    public AudioClip spinachMusic;

    private AudioClip currentMatchMusic;

    [Header("Item & UI Clips")]
    public AudioClip heartDing;
    public AudioClip bottleSmash;
    public AudioClip spinachEatSFX;

    [Header("Popeye Clips")]
    public AudioClip punchThud;
    public AudioClip popeyeJump;
    public AudioClip popeyeHit;
    public AudioClip popeyeWin;
    public AudioClip popeyeWalk;

    [Header("Bluto Clips")]
    public AudioClip blutoPunch;
    public AudioClip blutoJump;
    public AudioClip blutoHit;
    public AudioClip blutoThrowBottle;
    public AudioClip blutoCollectBottle;
    public AudioClip blutoWin;
    public AudioClip blutoWalk;

    [Header("Olive Clips")]
    public AudioClip oliveThrow;
    public AudioClip oliveWalk;

    private void OnEnable()
    {
        GameManager.OnGameStart += StartNewMatchMusic;
        GameManager.OnGameOver += PlayGameOverMusic;

        HeartItem.OnHeartCollected += PlayHeartDing;
        BottleItem.OnBottleSmashed += PlayBottleSmash;
        SpinachItem.OnSpinachEaten += PlaySpinachMusic;
        SpinachItem.OnSpinachEaten += PlaySpinachEatSFX;

        PopeyeController.OnPunch += PlayPunchThud;
        PopeyeController.OnJump += PlayPopeyeJump;
        PopeyeController.OnHit += PlayPopeyeHit;
        PopeyeController.OnWalk += PlayPopeyeWalk;

        BlutoController.OnHeavyPunch += PlayBlutoPunch;
        BlutoController.OnJump += PlayBlutoJump;
        BlutoController.OnHit += PlayBlutoHit;
        BlutoController.OnThrowBottle += PlayBlutoThrowBottle;
        BlutoController.OnBottleCollected += PlayBlutoCollectBottle;
        BlutoController.OnWalk += PlayBlutoWalk;

        OliveController.OnThrowHeart += PlayOliveThrow;
        OliveController.OnWalk += PlayOliveWalk;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= StartNewMatchMusic;
        GameManager.OnGameOver -= PlayGameOverMusic;

        HeartItem.OnHeartCollected -= PlayHeartDing;
        BottleItem.OnBottleSmashed -= PlayBottleSmash;
        SpinachItem.OnSpinachEaten -= PlaySpinachMusic;
        SpinachItem.OnSpinachEaten -= PlaySpinachEatSFX;

        PopeyeController.OnPunch -= PlayPunchThud;
        PopeyeController.OnJump -= PlayPopeyeJump;
        PopeyeController.OnHit -= PlayPopeyeHit;
        PopeyeController.OnWalk -= PlayPopeyeWalk;

        BlutoController.OnHeavyPunch -= PlayBlutoPunch;
        BlutoController.OnJump -= PlayBlutoJump;
        BlutoController.OnHit -= PlayBlutoHit;
        BlutoController.OnThrowBottle -= PlayBlutoThrowBottle;
        BlutoController.OnBottleCollected -= PlayBlutoCollectBottle;
        BlutoController.OnWalk -= PlayBlutoWalk;

        OliveController.OnThrowHeart -= PlayOliveThrow;
        OliveController.OnWalk -= PlayOliveWalk;
    }

    // ─── MUSIC LOGIC ─────────────────────────────────────────────────────────

    private void StartNewMatchMusic()
    {
        if (gameplayMusicTracks != null && gameplayMusicTracks.Length > 0)
        {
            int randomIndex = Random.Range(0, gameplayMusicTracks.Length);
            currentMatchMusic = gameplayMusicTracks[randomIndex];
        }

        PlayGameplayMusic();
    }

    private void PlayGameplayMusic()
    {
        if (currentMatchMusic != null)
        {
            musicSource.clip = currentMatchMusic;
            musicSource.Play();
        }
    }

    private void PlaySpinachMusic()
    {
        musicSource.clip = spinachMusic;
        musicSource.Play();
        Invoke(nameof(PlayGameplayMusic), 10f);
    }

    private void PlayGameOverMusic(bool popeyeWins)
    {
        musicSource.Stop();
        if (popeyeWins) sfxSource.PlayOneShot(popeyeWin);
        else sfxSource.PlayOneShot(blutoWin);
    }

    // ─── PITCH RANDOMIZATION HELPER ──────────────────────────────────────────

    private void PlayWithRandomPitch(AudioClip clip, float minPitch = 0.85f, float maxPitch = 1.15f)
    {
        variedSfxSource.pitch = Random.Range(minPitch, maxPitch);
        variedSfxSource.PlayOneShot(clip);
    }

    // ─── SFX WRAPPERS ────────────────────────────────────────────────────────

    private void PlayHeartDing() => PlayWithRandomPitch(heartDing, 0.9f, 1.1f);
    private void PlayBottleSmash() => PlayWithRandomPitch(bottleSmash, 0.9f, 1.1f);
    private void PlaySpinachEatSFX() => PlayWithRandomPitch(spinachEatSFX, 0.9f, 1.1f);
    private void PlayPopeyeJump() => PlayWithRandomPitch(popeyeJump, 0.9f, 1.1f);
    private void PlayPopeyeHit() => PlayWithRandomPitch(popeyeHit, 0.9f, 1.1f);
    private void PlayBlutoJump() => PlayWithRandomPitch(blutoJump, 0.9f, 1.1f);
    private void PlayBlutoHit() => PlayWithRandomPitch(blutoHit, 0.9f, 1.1f);
    private void PlayBlutoThrowBottle() => PlayWithRandomPitch(blutoThrowBottle, 0.9f, 1.1f);
    private void PlayBlutoCollectBottle() => PlayWithRandomPitch(blutoCollectBottle, 0.9f, 1.1f);
    private void PlayOliveThrow() => PlayWithRandomPitch(oliveThrow, 0.9f, 1.1f);

    private void PlayPunchThud() => PlayWithRandomPitch(punchThud, 0.9f, 1.1f);
    private void PlayBlutoPunch() => PlayWithRandomPitch(blutoPunch, 0.9f, 1.1f);
    private void PlayPopeyeWalk() => PlayWithRandomPitch(popeyeWalk, 0.8f, 1.2f);
    private void PlayBlutoWalk() => PlayWithRandomPitch(blutoWalk, 0.8f, 1.1f);
    private void PlayOliveWalk() => PlayWithRandomPitch(oliveWalk, 0.9f, 1.1f);
}