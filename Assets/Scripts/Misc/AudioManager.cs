using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public float masterVolume { get; private set; }
    public float sfxVolume { get; private set; }
    public float musicVolume { get; private set; }

    AudioSource sfx;
    AudioSource level1Source;
    AudioSource level2Source;
    AudioSource level3Source;
    AudioSource level4Source;
    AudioSource engineSource;

    Transform audioListener;
    Transform playerT;

    public enum Sound
    {
        Skid,
        Stop,
        Crash
    }

    [Header("Clips")]
    public AudioClip level1;
    public AudioClip level2;
    public AudioClip level3;
    public AudioClip level4;

    public AudioClip engine;
    public AudioClip skid;
    public AudioClip stop;
    public AudioClip crash;

    private bool waitingChange = false;
    private int levelToChange;
    private float timeChange;

    void Awake()
    {
        GameObject sfx2DS = new GameObject("SFX_Source");
        sfx = sfx2DS.AddComponent<AudioSource>();
        sfx2DS.transform.parent = transform;

        audioListener = FindObjectOfType<AudioListener>().transform;
        if (FindObjectOfType<Bus>() != null)
        {
            playerT = FindObjectOfType<Bus>().transform;
        }

        GameObject source1 = new GameObject("MusicSource Level1");
        level1Source = source1.AddComponent<AudioSource>();
        level1Source.volume = 1;
        GameObject source2 = new GameObject("MusicSource Level2");
        level2Source = source2.AddComponent<AudioSource>();
        level2Source.volume = 0;
        GameObject source3 = new GameObject("MusicSource Level3");
        level3Source = source3.AddComponent<AudioSource>();
        level3Source.volume = 0;
        GameObject source4 = new GameObject("MusicSource Level4");
        level4Source = source4.AddComponent<AudioSource>();
        level4Source.volume = 0;
        GameObject sourceEngine = new GameObject("MusicSource Engine");
        engineSource = sourceEngine.AddComponent<AudioSource>();
        engineSource.volume = 0.4f;
        PlayMusic();
    }

    void Update()
    {
        if (playerT != null)
        {
            audioListener.position = playerT.position;
        }
        if(waitingChange && level1Source.time > timeChange)
        {
            level1Source.volume = 0;
            level2Source.volume = 0;
            level3Source.volume = 0;
            level4Source.volume = 0;

            switch (levelToChange)
            {
                case 1:
                    level1Source.volume = 1;
                    break;
                case 2:
                    level2Source.volume = 1;
                    break;
                case 3:
                    level3Source.volume = 1;
                    break;
                case 4:
                    level4Source.volume = 1;
                    break;
            }
            waitingChange = false;
        }
    }

    public void SetVolume(int level)
    {
        waitingChange = true;
        levelToChange = level;
        float barDur = 60f / 125f * 4;
        timeChange = (level1Source.time / barDur) - (level1Source.time % barDur) + barDur;
    }

    public void PlayMusic()
    {
        level1Source.clip = level1;
        level1Source.loop = true;
        level1Source.Play();
        level1Source.time = 60;

        level2Source.clip = level2;
        level2Source.loop = true;
        level2Source.Play();
        level2Source.time = 60;

        level3Source.clip = level3;
        level3Source.loop = true;
        level3Source.Play();
        level3Source.time = 60;

        level4Source.clip = level4;
        level4Source.loop = true;
        level4Source.Play();
        level4Source.time = 60;

        engineSource.clip = engine;
        engineSource.loop = true;
        engineSource.Play();
    }

    public void PlaySound(Sound clipName)
    {
        AudioClip clip = null;
        switch (clipName)
        {
            case Sound.Skid:
                clip = skid;
                break;
            case Sound.Stop:
                clip = stop;
                break;
            case Sound.Crash:
                clip = crash;
                break;
        }
        if (clip != null)
        {
            sfx.clip = clip;
            sfx.time = 0.32f;
            sfx.loop = false;
            sfx.Play();
        }
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode sceneMode)
    {
        if (playerT == null)
        {
            if (FindObjectOfType<Bus>() != null)
            {
                playerT = FindObjectOfType<Bus>().transform;
            }
        }
    }
}