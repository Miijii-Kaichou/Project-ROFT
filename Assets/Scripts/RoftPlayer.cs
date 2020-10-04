﻿using System;
using System.Globalization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class RoftPlayer : MonoBehaviour
{
    public static RoftPlayer Instance;

    #region Public Members
    //The EditorTool is going to be used to make creating music so much easier.
    //First, we'll get a reference of a song
    public AudioClip music;

    public bool record;

    public ObjectLogger objectLogger;

    public enum TickSignature
    {
        Normal = 4,
        Waltz = 3
    }

    public TickSignature tickSignature;

    //Then, we'll reference our UI
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;

    //We'll then want to reference our time in samples text
    public TextMeshProUGUI samplesText;
    public TextMeshProUGUI songPercentage;


    //For the rest of the stuff like the Ui Slider and such
    public Slider songTrackPosition;

    //And the Dropdown for playback rate
    public TMP_Dropdown playbackRate;

    //Check if we're changing positions
    public bool caretIsBeingDragged;

    //And a pitch for play back speed;
    public AudioMixerGroup pitchMixer;

    public static AudioSource musicSource;
    #endregion
   

    #region Private Members
    private bool musicIsPlaying = false;
    private float timeInSamples;
    private float musicLengthInSamples;
    private float sampleDivisor = 1;
    private float seconds, minutes;

    //Recording and getting the bpm and then to get the pulse
    public int taps;
    public int inactiveTaps;

    public int ticks; //This will carry on the value of taps
    public int totalTicksSinceStart;
    public float firstTickSample; //The sample in which the designer started tapping

    public float oldPulse;
    public float newPulse;
    public float determinedPulse;
    public float bpm;
    public bool tapCalulationDone = false;

    public NoteEffector noteEffector;

    readonly private KeyCode tapKey = KeyCode.T;
    readonly private int tapLimit = 16;
    readonly private int doneTapLimit = 3;

    float time = 0f;

    public Action loadAndPlay;

    #endregion

    private void Awake()
    {

        #region Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
        loadAndPlay += LoadMusic;
        loadAndPlay += PlayMusic;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && record)
            PlayMusic();

        if (Input.GetKeyDown(KeyCode.Return) && !musicSource.isPlaying && !record)
            StartCoroutine(Wait(3, loadAndPlay));

        if (musicSource != null)
            musicIsPlaying = musicSource.isPlaying;
    }

    void FixedUpdate()
    {
        if (record)
        {
            if (Input.GetAxis("Mouse ScrollWheel") == 0)
                caretIsBeingDragged = false;

            if (!caretIsBeingDragged) UpdateCaret();

            if (songTrackPosition != null && songTrackPosition.value > 1)
            {
                PauseMusic();
                musicSource.timeSamples = (int)musicLengthInSamples;
            }

            TimeStamp();

            
        }
    }


    public void LoadMusic()
    {
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.outputAudioMixerGroup = MusicManager.GetInstance().musicMixer;

        if (record)
            musicSource.clip = RoftCreator.Instance.GetAudioFile();

        else
            musicSource.clip = MapReader.SongEntityBeingRead.AudioFile;

        musicLengthInSamples = musicSource.clip.length * musicSource.clip.frequency;
        music = musicSource.clip;
    }

    //Play the song
    public void PlayMusic()
    {
        musicSource.Play();
        MusicManager.NowPlaying = musicSource.clip.name;
    }

    //Pause the song
    public void PauseMusic()
    {
        musicSource.Pause();
        if (!musicIsPlaying) musicSource.Play();
    }

    //Stop the Music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    public float GetMusicLengthInSamples() => musicLengthInSamples;

    IEnumerator Wait(int duration, Action function = null)
    {
        yield return new WaitForSeconds(duration);
        if (function != null) function.Invoke();
        GameManager.inSong = true;
    }

    public void UpdateCaret()
    {
       

        if (musicSource != null && record && !caretIsBeingDragged)
        {
            //So, here's how this is going to work...
            //The Track Position is between values 0 and 1
            //We have to find a way to convers a percentage of something to samples...
            musicSource.pitch = sampleDivisor;

            musicSource.outputAudioMixerGroup.audioMixer.SetFloat("pitchBend", 1 / musicSource.pitch);

            timeInSamples = musicSource.timeSamples;

            songTrackPosition.value = timeInSamples / musicLengthInSamples;

            songPercentage.text = (songTrackPosition.value * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";

            samplesText.text = minutes.ToString() + ":" + Mathf.FloorToInt(seconds).ToString("D2");
        }
    }

    public void ScrollCaret()
    {
        songTrackPosition.value -= Input.GetAxis("Mouse ScrollWheel");
        UpdateSampleFromCaretPosition();
    }

    public void UpdateSampleFromCaretPosition()
    {
        ToggleCaretDrag(true);

        //We're doing the opposite of the UpdateCaret
        musicSource.timeSamples = (int)(songTrackPosition.value * musicLengthInSamples);

        timeInSamples = musicSource.timeSamples;

        songPercentage.text = (songTrackPosition.value * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";

        samplesText.text = minutes.ToString() + ":" + Mathf.FloorToInt(seconds).ToString();
    }

    public void UpdatePlayBackSpeed()
    {
        for (int option = 0; option < playbackRate.options.Count; option++)
        {
            if (option == playbackRate.value)
            {
                switch (option)
                {
                    case 0:
                        sampleDivisor = 1;
                        musicSource.outputAudioMixerGroup.audioMixer.SetFloat("fftSize", 927f);
                        break;
                    case 1:
                        sampleDivisor = 0.75f;
                        musicSource.outputAudioMixerGroup.audioMixer.SetFloat("fftSize", 1060f);
                        break;
                    case 2:
                        sampleDivisor = 0.5f;
                        musicSource.outputAudioMixerGroup.audioMixer.SetFloat("fftSize", 3608f);
                        break;
                    case 3:
                        sampleDivisor = 0.25f;
                        break;
                }
            }
        }
    }

    public void ToggleCaretDrag(bool _on)
    {
        caretIsBeingDragged = _on;
    }

    void TimeStamp()
    {
        if (record)
        {
            if (seconds > 59.99f)
            {
                seconds = 0;
                minutes++;
            }
            else if (seconds < 0)
            {
                seconds = 59;
                minutes--;
            }

            seconds = musicSource.time - (60 * minutes);
        }
    }
}