﻿using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    /*Music Manager will essentially collect all music files
     * from all SongEntities detected by RoftScouter,
     * and it will preview it for the player.
     * 
     * Music Manager will later be changed to
     * SongEntityManager, because instead of just Music clips,
     * it'll be a class that holds not only the song (in which it'll go through and play),
     * but give us all kinds of different information.
     */

    private static MusicManager Instance;

    public List<Song_Entity> songs;

    public static List<Song_Entity> Songs;

    public Slider musicVolumeAdjust, soundVolumeAdjust; //Reference to our volume sliders

    public static string NowPlaying;

    // Start is called before the first frame update
    public float timeSamples;

    public float[] positionSeconds;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameManager.Instance.ExecuteScouting();
    }

    public static List<Song_Entity> GetSongEntity() => Songs;

    public static void DisplaySongs(List<Song_Entity> _songs)
    {
        Instance.songs = _songs;
    }
}
