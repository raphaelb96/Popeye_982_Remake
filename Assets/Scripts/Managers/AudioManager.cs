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

    private void PlayHeartDing() => sfxSource.PlayOneShot(heartDing);
    private void PlayBottleSmash() => sfxSource.PlayOneShot(bottleSmash);
    private void PlaySpinachEatSFX() => sfxSource.PlayOneShot(spinachEatSFX);
    private void PlayPopeyeJump() => sfxSource.PlayOneShot(popeyeJump);
    private void PlayPopeyeHit() => sfxSource.PlayOneShot(popeyeHit);
    private void PlayBlutoJump() => sfxSource.PlayOneShot(blutoJump);
    private void PlayBlutoHit() => sfxSource.PlayOneShot(blutoHit);
    private void PlayBlutoThrowBottle() => sfxSource.PlayOneShot(blutoThrowBottle);
    private void PlayBlutoCollectBottle() => sfxSource.PlayOneShot(blutoCollectBottle);
    private void PlayOliveThrow() => sfxSource.PlayOneShot(oliveThrow);

    private void PlayPunchThud() => PlayWithRandomPitch(punchThud, 0.9f, 1.1f);
    private void PlayBlutoPunch() => PlayWithRandomPitch(blutoPunch, 0.9f, 1.1f);
    private void PlayPopeyeWalk() => PlayWithRandomPitch(popeyeWalk, 0.8f, 1.2f);
    private void PlayBlutoWalk() => PlayWithRandomPitch(blutoWalk, 0.8f, 1.1f);
    private void PlayOliveWalk() => PlayWithRandomPitch(oliveWalk, 0.9f, 1.1f);
}