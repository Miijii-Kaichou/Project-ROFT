﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

using static ROFTIOMANAGEMENT.RoftIO;

public class MapReader : MonoBehaviour
{
    public static MapReader Instance;

    [SerializeField] private string m_name;

    [SerializeField] private float difficultyRating;

    [SerializeField] private float totalNotes;

    [SerializeField] private float totalKeys;

    [SerializeField] private long maxScore;

    public int keyLayoutEnum;

    public List<NoteObj> noteObjs = new List<NoteObj>();

    public Key_Layout keyLayoutClass = Key_Layout.Instance;

    //All Object Readers
    [Header("Object Readers")]
    [SerializeField] private TapObjectReader tapObjectReader;
    [SerializeField] private HoldObjectReader holdObjectReader;
    [SerializeField] private BurstObjectReader burstObjectReader;
    [SerializeField] private TrailObjectReader trailObjectReader;
    [SerializeField] private ClickObjectReader clickObjectReader;

    //I'll have to keep track of which files the MapReader is actually reading, or else I can't grab the information
    public static Song_Entity SongEntityBeingRead { get; set; }
    public static int DifficultyIndex { get; set; }

    readonly Thread readKeyThread;
    static bool keysReaded = false;
    public static bool KeysReaded
    {
        get
        {
            return keysReaded;
        }
    }
    private readonly object keyReadingLock = new object();

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

    }

    private void Start()
    {
        GameManager.Instance.ExecuteScouting();

        if (!GameManager.SongsNotFound)
        {
            SongEntityBeingRead = MusicManager.GetSongEntity()[1];
            AssignRFTMNameToRead(SongEntityBeingRead, _difficultValue: 2);
        }
        else
        {
            Initialize();
            CalculateDifficultyRating();
        }
    }

    void Initialize()
    {
        if (keyLayoutClass != null)
            KeyLayoutAwake();

        //Get other values such as Approach Speed, Stress Build, and Accuracy Harshness
        if (RoftPlayer.Instance != null && RoftPlayer.Instance.record == false)
        {
            int difficultyTag = InRFTMJumpTo("Difficulty", m_name);
            NoteEffector.Instance.ApproachSpeed = ReadPropertyFrom<float>(difficultyTag, "ApproachSpeed", m_name);
            GameManager.Instance.stressBuild = ReadPropertyFrom<float>(difficultyTag, "StressBuild", m_name);
            NoteEffector.Instance.Accuracy = ReadPropertyFrom<float>(difficultyTag, "AccuracyHarshness", m_name);
            maxScore = CalculateMaxScore();
        }
        else
        {
            Debug.Log("For some reason, this is not being read....");
        }
    }

    void ReadRFTMKeys(string _name)
    {
        lock (keyReadingLock)
        {
            string line;

            string rftmFilePath = _name;

            int maxKey = 0;

            #region Read .rftm data
            if (File.Exists(rftmFilePath))
            {
                //Read each line, split with a array string
                //Name it separator
                //Then use string.Split(separator, ...)
                const char separator = ',';
                int filePosition = 0;
                int targetPosition = InRFTMJumpTo("Objects", m_name);
                using (StreamReader rftmReader = new StreamReader(rftmFilePath))
                {
                    while (true)
                    {
                        line = rftmReader.ReadLine();
                        if (line == null)
                        {
                            keysReaded = true;
                            return;
                        }


                        if (filePosition > targetPosition)
                        {
                            #region Parse and Convert Information
                            //We'll count the frequency of commas to determine
                            //that they are more values in the object
                            int countCommas = 0;
                            foreach (char c in line)
                                if (c == ',')
                                    countCommas++;

                            //We create a new key, and assign our data value to our key
                            NoteObj newNoteObj = new NoteObj();

                            newNoteObj.SetInstanceID((uint)Convert.ToInt32(line.Split(separator)[0]));
                            newNoteObj.SetInstanceSample(Convert.ToInt32(line.Split(separator)[1]));
                            newNoteObj.SetInstanceType((NoteObj.NoteObjType)Convert.ToInt32(line.Split(separator)[2]));

                            //Check for any miscellaneous values
                            if (countCommas > 2)
                                newNoteObj.miscellaneousValue1 = Convert.ToInt32(line.Split(separator)[3]);

                            else if (countCommas > 3)
                                newNoteObj.miscellaneousValue2 = Convert.ToInt32(line.Split(separator)[4]);
                            #endregion

                            /*This looks at the noteID
                             which is simply the number that is linked
                             to the keyID in the game.
                             */
                            if (newNoteObj.GetInstanceID() > maxKey)
                            {
                                maxKey = (int)newNoteObj.GetInstanceID();
                                totalKeys = maxKey + 1;
                            }

                            //Lastly, add our new key to the list
                            noteObjs.Add(newNoteObj);

                            //We'll be integrating our new Object Readers around this area.
                            //We'll distribute the basic values to each one.
                            #region Distribution to Readers
                            switch (newNoteObj.GetInstanceType())
                            {
                                case NoteObj.NoteObjType.Tap:
                                    DistributeTypeTo(tapObjectReader, newNoteObj);
                                    break;

                                case NoteObj.NoteObjType.Hold:
                                    DistributeTypeTo(holdObjectReader, newNoteObj);
                                    break;

                                case NoteObj.NoteObjType.Burst:
                                    DistributeTypeTo(burstObjectReader, newNoteObj);
                                    break;

                                default:
                                    break;
                            }
                            #endregion

                            //Update total Notes
                            totalNotes = noteObjs.Count;
                        }
                        filePosition++;
                    }
                }
            }
        }
        #endregion
    }

    void CalculateDifficultyRating()
    {
        RoftPlayer.Instance.LoadMusic();
        int totalNotes = noteObjs.Count;
        float songLengthInSec = RoftPlayer.musicSource.clip.length;
        float notesPerSec = (totalNotes / songLengthInSec);
        float totalKeys = keyLayoutClass.primaryBindedKeys.Count;
        float approachSpeedInPercent = (float)NoteEffector.Instance.ApproachSpeed / 100;
        float gameModeBoost = 0;
        const int maxKeys = 30;

        float calculatedRating = notesPerSec +
            (totalKeys / maxKeys) +
            approachSpeedInPercent +
            (RoftPlayer.musicSource.pitch / 2) +
            gameModeBoost;

        difficultyRating = calculatedRating;
    }

    long CalculateMaxScore()
    {
        long ini_Score = 0;
        for (long ini_Combo = 1; ini_Combo < totalNotes + 1; ini_Combo++)
        {
            ini_Score += 300 * ini_Combo;
        }
        return ini_Score;
    }

    void KeyLayoutAwake()
    {
        int difficultyTag = InRFTMJumpTo("Difficulty", m_name);
        int keyCount = ReadPropertyFrom<int>(difficultyTag, "KeyCount", m_name);
        totalKeys = keyCount;
        if (!keyLayoutClass.gameObject.activeInHierarchy)
        {
            keyLayoutClass.gameObject.SetActive(true);
        }

        #region Reading KeyCount

        switch (keyCount)
        {
            case 4:
                keyLayoutClass.keyLayout = Key_Layout.KeyLayoutType.Layout_1x4;
                break;
            case 8:
                keyLayoutClass.keyLayout = Key_Layout.KeyLayoutType.Layout_2x4;
                break;
            case 12:
                keyLayoutClass.keyLayout = Key_Layout.KeyLayoutType.Layout_3x4;
                break;
            case 16:
                keyLayoutClass.keyLayout = Key_Layout.KeyLayoutType.Layout_4x4;
                break;
        }
        #endregion

        if
           (GameManager.Instance.GetGameMode == GameManager.GameMode.TECHMEISTER ||
            GameManager.Instance.GetGameMode == GameManager.GameMode.STANDARD)
            Key_Layout.Instance.SetUpLayout();

    }

    void DistributeTypeTo(ObjectTypes _objectReader, NoteObj _key)
    {
        if (_objectReader != null)
            _objectReader.objects.Add(_key);
    }

    public void AssignRFTMNameToRead(Song_Entity _song, int _difficultValue)
    {
        /*We want to be able to get retrieve a song entity, and wiht access to the song
         entity, we are able to read the .rftm (which are the difficulties) of the song.
         We will now call the ReadRFTM function to read the data that's been provided to us, thus
         ROFTPlayer can then retrieve information that is sorted on this object.

         That's at least what I hope this will do.*/

        int count = 0;
        foreach (Song_Entity.Song_Entity_Difficulty diffVal in _song.Difficulties)
        {
            if (count == _difficultValue)
            {
                SongEntityBeingRead = _song;
                DifficultyIndex = _difficultValue;
                m_name = diffVal.RFTMFile;
                ReadRFTMKeys(m_name);

                Initialize();
                CalculateDifficultyRating();

                return;
            }

            count++;
        }

        Debug.Log("An Error had occured... ");
        return;
    }

    #region Set Methods
    public void SetName(string _value)
    {
        m_name = _value;
    }
    #endregion

    #region Get Methods
    public string GetName() => m_name;

    public float GetDifficultyRating() => difficultyRating;

    public float GetTotalNotes() => totalNotes;

    public float GetTotalKeys() => totalKeys;

    public long GetMaxScore() => maxScore;

    public ObjectTypes GetReaderType<T>()
    {
        if (typeof(T) == tapObjectReader.GetType())
            return tapObjectReader;

        if (typeof(T) == holdObjectReader.GetType())
            return holdObjectReader;

        if (typeof(T) == burstObjectReader.GetType())
            return burstObjectReader;

        return null;
    }

    #endregion
}
