﻿using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    [Range(1, 400)] public float bpm;
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [Range(0f, 1f)] public float timingThreshold = 0.2f;
    [Range(-1f, 1f)] public float timingThresholdOffset;
    [Range(0f, 1f)] public float beatStartTimeOffset; // à supprimer pour l'adapter selon la machine qui joue la musique

    public float cameraBeatEffectLerpSpeed;
    public float cameraBeatEffectAmplitude;

    [HideInInspector] public bool onBeatSingleFrame;
    [HideInInspector] public bool onBeatFirstFrame;
    [HideInInspector] public bool onBeatNextFrame;
    private bool firstFrameFlag;
    private bool nextFrameFlag;

    private bool musicStarted;
    [HideInInspector] public float beatTime;
    private float timeBeforeNextBeat;
    [HideInInspector] public float currentBeatProgression;
    private float nextBeatStartTime;
    private float offBeatStartTime;
    private float songStartTime;

    private AudioSource source;

    private float initialCameraSize;
    #endregion


    #region Singleton
    public static BeatManager Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    #endregion

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        source = GetComponent<AudioSource>();
        musicStarted = false;
        beatTime = 60 / bpm;
        onBeatSingleFrame = false;

        initialCameraSize = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (!musicStarted && Input.GetButtonDown("Blink"))
        {
            StartMusic();

            nextBeatStartTime = (float)AudioSettings.dspTime + beatStartTimeOffset;
            offBeatStartTime = nextBeatStartTime;
        }

        if (musicStarted)
        {
            TimeCycle();
        }
    }

    /// <summary>
    /// Manages the time according to beats.
    /// </summary>
    private void TimeCycle()
    {
        if(onBeatSingleFrame)
        {
            onBeatSingleFrame = false;
        }

        if (onBeatFirstFrame)
        {
            onBeatFirstFrame = false;
        }

        if (onBeatNextFrame)
        {
            onBeatNextFrame = false;
        }

        if (OnBeat())
        {
            if(firstFrameFlag)
            {
                onBeatFirstFrame = true;
                firstFrameFlag = false;
            }

            nextFrameFlag = true;
        }
        else
        {
            firstFrameFlag = true;

            if(nextFrameFlag)
            {
                onBeatNextFrame = true;
                nextFrameFlag = false;
            }
        }

        if (nextBeatStartTime < (float)AudioSettings.dspTime)
        {
            nextBeatStartTime += beatTime;
            //StartCoroutine(BeatEffect(1));
            onBeatSingleFrame = true;
        }

        if (offBeatStartTime < (float)AudioSettings.dspTime - beatTime / 2)
        {
            offBeatStartTime += beatTime;
            //StartCoroutine(BeatEffect(0.2f));
        }

        timeBeforeNextBeat = nextBeatStartTime - (float)AudioSettings.dspTime;
        currentBeatProgression = 1 - (timeBeforeNextBeat / beatTime);
    }

    private IEnumerator BeatEffect(float amplitude)
    {
        Camera.main.orthographicSize = initialCameraSize + cameraBeatEffectAmplitude * amplitude;
        while (Camera.main.orthographicSize > initialCameraSize + 0.01f)
        {
            Camera.main.orthographicSize -= cameraBeatEffectLerpSpeed * (Camera.main.orthographicSize - initialCameraSize) * Time.fixedDeltaTime * 50;
            yield return new WaitForFixedUpdate();
        }
        Camera.main.orthographicSize = initialCameraSize;
    }

    public bool OnBeat()
    {
        bool onBeat = false;
        float beatTimeProgression = beatTime - timeBeforeNextBeat;

        if(timingThresholdOffset >= timingThreshold / 2)
        {
            if(beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset && beatTimeProgression > timingThresholdOffset - (timingThreshold / 2))
            {
                onBeat = true;
            }
        }
        else if(timingThresholdOffset <= - timingThreshold / 2)
        {
            if (timeBeforeNextBeat < (timingThreshold / 2) - timingThresholdOffset && timeBeforeNextBeat > - timingThresholdOffset - (timingThreshold / 2))
            {
                onBeat = true;
            }
        }
        else
        {
            if (beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset || timeBeforeNextBeat < (timingThreshold / 2) - timingThresholdOffset)
            {
                onBeat = true;
            }
        }

        return onBeat;
    }

    private void StartMusic()
    {
        musicStarted = true;

        source.Play();

        songStartTime = (float)AudioSettings.dspTime;
    }
}
