// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip gameplayMusic;
    public AudioClip spinachMusic;
    public AudioClip punchThud;
    public AudioClip heartDing;
    public AudioClip bottleSmash;

    private void OnEnable()
    {
        GameManager.OnGameStart += PlayGameplayMusic;
        HeartItem.OnHeartCollected += PlayHeartDing;
        BottleItem.OnBottleSmashed += PlayBottleSmash;
        PopeyeController.OnPunch += PlayPunchThud;
        BlutoController.OnHeavyPunch += PlayPunchThud;
        SpinachItem.OnSpinachEaten += PlaySpinachMusic;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= PlayGameplayMusic;
        HeartItem.OnHeartCollected -= PlayHeartDing;
        BottleItem.OnBottleSmashed -= PlayBottleSmash;
        PopeyeController.OnPunch -= PlayPunchThud;
        BlutoController.OnHeavyPunch -= PlayPunchThud;
        SpinachItem.OnSpinachEaten -= PlaySpinachMusic;
    }

    private void PlayGameplayMusic()
    {
        musicSource.clip = gameplayMusic;
        musicSource.Play();
    }

    private void PlaySpinachMusic()
    {
        musicSource.clip = spinachMusic;
        musicSource.Play();
        Invoke(nameof(PlayGameplayMusic), 10f); 
    }

    private void PlayHeartDing() => sfxSource.PlayOneShot(heartDing);
    private void PlayBottleSmash() => sfxSource.PlayOneShot(bottleSmash);
    private void PlayPunchThud() => sfxSource.PlayOneShot(punchThud);
}