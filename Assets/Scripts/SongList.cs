using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SongList : MonoBehaviour
{
    public static SongList Instance;

    public enum SORTING
    {
        NONE,
        NAME,
        ARTIST,
        INITIAL_DIFFICULTY
    }

    public SORTING sortingOrder = SORTING.NONE;

    [SerializeField]
    private SongContent content;

    [SerializeField]
    private PreviewSong songPreviewer;

    [SerializeField]
    private SongSelectionNavigator songNavigator;

    [SerializeField]
    private TextMeshProUGUI TMP_SongTitle, TMP_SongTitleSecondary;

    [SerializeField]
    private Image[] songCovers;

    [SerializeField]
    private TMP_Dropdown TMPDD_SortingOrder;

    GameObject songEntryObj;
    SongEntry songEntry;

    float fadeVelocity;

    private void Awake()
    {
        Instance = this;
    }

    

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        #region Variable Declaration
        Song_Entity currentEntity;

        List<Song_Entity> orderedSongs = new List<Song_Entity>();
        #endregion

        #region Sorting
        switch (sortingOrder)
        {
            case SORTING.NONE:
                orderedSongs = MusicManager.Songs;
                break;

            case SORTING.NAME:
                var nameSorting = from song
                                  in MusicManager.Songs
                                  orderby song.SongTitle
                                  select song;

                orderedSongs = nameSorting.ToList();
                break;

            case SORTING.ARTIST:
                var artistSorting = from song
                                    in MusicManager.Songs
                                    orderby song.SongArtist, song.SongTitle
                                    select song;

                orderedSongs = artistSorting.ToList();
                break;

            case SORTING.INITIAL_DIFFICULTY:
                var diffSorting = from song
                                  in MusicManager.Songs
                                  orderby song.InitialDifficultyRating, song.SongArtist
                                  select song;

                orderedSongs = diffSorting.ToList();
                break;

            default:
                orderedSongs = MusicManager.Songs;
                break;
        }
        #endregion

        #region Initialization
        for (int index = 0; index < orderedSongs.Count; index++)
        {
            try
            {
                songEntryObj = content.GetObjectPooler().GetMember("Log");

                songEntry = songEntryObj.GetComponent<SongEntry>();

                if (songEntryObj != null && !songEntryObj.activeInHierarchy)
                {
                    //Set it to Active
                    songEntryObj.SetActive(true);

                    //We will not reposition it. ObjectPooler from SongContent should put the entries for us.
                    //Assigning important values.
                    currentEntity = orderedSongs[index];

                    songEntry.UpdateSongTitle(string.Format("{0} - {1}", currentEntity.SongArtistUnicode, currentEntity.SongTitleUnicode));
                    songEntry.UpdateSongCover(currentEntity.BackgroundImage);
                    songEntry.UpdateSongClip(currentEntity.AudioFile);
                    songEntry.UpdateRanking(null);

                    //Assign Index ID
                    songEntry.AssignIndexID(orderedSongs[index].ScoutEntryValue);
                }
            }
            catch (IOException e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
    }

    public void ReInit()
    {
        //Iterate through pooled objects, and set them inactive;
        foreach (GameObject log in content.objectPooler.pooledObjects)
        {
            if (log.activeInHierarchy)
                log.SetActive(false);
        }

        //Update Sorting Order from DropDown value
        sortingOrder = (SORTING)TMPDD_SortingOrder.value;

        //Initialize again
        Init();
    }

    public void FadeInImages()
    {
        StartCoroutine(FadeInImagesCycle());
    }

    private IEnumerator FadeInImagesCycle()
    {
        var animating = true;
        var i = 0f;
        while(animating)
        {
            i = Mathf.SmoothDamp(i, 255f, ref fadeVelocity, 0.1f);

            //Index 0: Main Cover Display
            songCovers[0].color = new Color(songCovers[0].color.r, songCovers[0].color.g, songCovers[0].color.b, i / 255f);

            //Index 1: Background Cover Display
            songCovers[1].color = new Color(songCovers[1].color.r, songCovers[1].color.g, songCovers[1].color.b, i / 255f);

            yield return new WaitForFixedUpdate();

            animating = i < 255;
        }
    }

    public TextMeshProUGUI GetTMP() => TMP_SongTitle;
    public TextMeshProUGUI GetSecondaryTMP() => TMP_SongTitleSecondary;
    public Image[] GetSongCovers() => songCovers;
    public PreviewSong GetSongPreviewer() => songPreviewer;

    public SongSelectionNavigator GetSongNavigator() => songNavigator;
}
