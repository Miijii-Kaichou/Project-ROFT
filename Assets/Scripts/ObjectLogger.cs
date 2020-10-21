﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ROFTIOMANAGEMENT;
using System;
using System.Text;
using Extensions;
using System.Collections;

public class ObjectLogger : MonoBehaviour
{
    private static ObjectLogger Instance;

    [Serializable]
    public class NoteStack
    {
        public NoteObj[] NoteObjs;
        int stackValue = 0;

        public void Init(int size)
        {
            NoteObjs = new NoteObj[size];
        }
        public void AddItem(NoteObj obj)
        {
            NoteObjs[stackValue] = obj;
            stackValue = NoteObjs.Length;
        }

        public void AddItem(NoteObj obj, int position)
        {
            NoteObjs[position] = obj;
            stackValue = NoteObjs.Length;
        }

        public void RemoveItem()
        {
            NoteObjs[stackValue].Clear();
            stackValue = NoteObjs.Length;
        }

        public void RemoveItem(int position)
        {
            NoteObjs[position].Clear();
            stackValue = NoteObjs.Length;
        }

        /// <summary>
        /// Return the size of the amount of stacks in this pattern set
        /// </summary>
        /// <returns></returns>
        public int Size() => NoteObjs.Length;

        /// <summary>
        /// Check if Cell is Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsCellEmpty(int value)
        {
            try
            {
                return NoteObjs != null && NoteObjs[value].Empty();
            }
            catch { return true; }
        }
    }

    /// <summary>
    /// This class holds all object placement and timing
    /// </summary>
    [Serializable]
    public class PatternSet
    {
        //We represent a NoteObj at this cell with a given
        //Tick value and Pattern value

        public NoteStack[] NoteStacks;

        const int MAX_OBJECT_STACK = 4;

        public void Init(int size)
        {
            NoteStacks = new NoteStack[MAX_OBJECT_STACK];

            for (int i = 0; i < NoteStacks.Length; i++)
            {
                NoteStacks[i] = new NoteStack();
                NoteStacks[i].Init(size);
            }
        }

        public void LogObject(NoteObj obj, int stackValue, int position)
        {
            NoteStacks[stackValue].AddItem(obj, position);
        }

        /// <summary>
        /// Remove item from set stack
        /// </summary>

        public void RemoveObject(int stackValue, int position)
        {
            NoteStacks[stackValue].RemoveItem(position);
        }


        /// <summary>
        /// Return the size of the amount of stacks in this pattern set
        /// </summary>
        /// <returns></returns>
        public int Size() => NoteStacks.Length;


        /// <summary>
        /// Turn all object data into a string
        /// </summary>
        /// <returns></returns>
        public string ExtractData()
        {
            StringBuilder data = new StringBuilder();

            List<NoteObj> objectsInOrder = new List<NoteObj>();

            //Add the object notes from each stack into a list
            for (int row = 0; row < NoteStacks.Length; row++)
            {
                NoteStack notestack = NoteStacks[row];

                for (int col = 0; col < notestack.NoteObjs.Length; col++)
                {
                    NoteObj obj = null;

                    if (notestack.NoteObjs[col] != null)
                    {
                        obj = notestack.NoteObjs[col];

                        Debug.Log(obj.GetNoteType());
                        objectsInOrder.Add(obj);
                    }
                }
            }

            //Order the list by sample position
            var orderedList = from o in objectsInOrder orderby o.GetInitialeSample() select o;
            objectsInOrder = orderedList.ToList();

            //Loop throw again, and build string
            for (int i = 0; i < objectsInOrder.Count; i++)
            {
                NoteObj obj = objectsInOrder[i];
                if (obj != null && !obj.Empty())
                {
                    data.Append(obj.AsString() + "\n");
                    Debug.Log("Doing the thing...");
                }
            }

            //Return the constructed string
            return data.ToString();
        }
    }

    public TextMeshProUGUI TMP_Tick;
    public TextMeshProUGUI TMP_PatternSet;
    public TextMeshProUGUI TMP_StackValue;
    public enum LoggerSize
    {
        WALTZ = 3,
        WHOLE = 4,
        SIXTH = 6,
        EIGHTH = 8,
        SIXTEEN = 16,
        THRITY_SECNOND = 32,
        SIXTY_FORTH = 64,
        ONE_HUNTRED_TWENTY_EIGHTH = 128
    }

    public enum NoteTool
    {
        TAP,
        HOLD,
        TRACK,
        BURST
    }

    public LoggerSize loggerSize = LoggerSize.WHOLE;

    [Range(1f, 4f)]
    public float rate = 1f;

    public float offsetInSeconds = 0;

    public GameObject prefab;

    readonly Color c_currentTick = Color.white;
    readonly Color c_emptyTick = new Color(25f / MAX_ALPHA, 25f / MAX_ALPHA, 25f / MAX_ALPHA);
    readonly Color c_emptyTickGrey = Color.grey;
    readonly Color c_emptyTickDarkGrey = new Color(50f / MAX_ALPHA, 50f / MAX_ALPHA, 50f / MAX_ALPHA);
    readonly Color c_tickNoteData = new Color(16f / MAX_ALPHA, 210f / MAX_ALPHA, 213f / MAX_ALPHA);
    readonly Color c_tickTimeData = new Color(247f / MAX_ALPHA, 244f / MAX_ALPHA, 40f / MAX_ALPHA);

    [SerializeField]
    public float bpm = 120;
    float oldBPM = 0;

    GameObject[] blocks;

    RectTransform rectTransform;

    const float MINUTEINSEC = 60;

    const float MAX_ALPHA = 255f;

    float tick = -1;
    float inGameTime = 0;

    [SerializeField]
    List<NoteObj> objects = new List<NoteObj>();

    List<PatternSet> patternSets = new List<PatternSet>();

    const float reset = 0f;

    //Data
    public int keyData;
    public long sampleData;
    public int typeData;

    //Unique data
    public long finishSample; //For hold type
    public List<TrackPoint> trackPoints; //For track type
    public int burstDirection; //For burst type

    string dataFormat;

    public static NoteTool noteTool;
    float tickValue;
    float currentTick;
    float currentPatternSet;
    float currentStack = 1;

    bool sequenceDone = false;

    public static string ObjectData;

    //Keyboard controls
    readonly Dictionary<KeyCode, int> keyCodeStackDictionary = new Dictionary<KeyCode, int>();
    readonly KeyCode removeNote = KeyCode.Backspace;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        keyCodeStackDictionary.Add(KeyCode.Alpha1, 1);
        keyCodeStackDictionary.Add(KeyCode.Alpha2, 2);
        keyCodeStackDictionary.Add(KeyCode.Alpha3, 3);
        keyCodeStackDictionary.Add(KeyCode.Alpha4, 4);
        Init((int)loggerSize);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        inGameTime = RoftPlayer.musicSource.time - offsetInSeconds;

        tickValue = inGameTime / (60f / (bpm * rate * ((float)loggerSize / (float)LoggerSize.WHOLE)));
        currentTick = Mathf.Floor(tickValue);
        currentPatternSet = Mathf.Floor(tickValue / (float)loggerSize);

        if (tick != currentTick && inGameTime > 0f)
        {
            tick = currentTick;
            UpdateUI();
            if (Mathf.Floor(inGameTime % (60f / bpm)) == 0)
                Tick();
        }
    }

    void UpdateUI()
    {
        TMP_Tick.text = ((tick % (int)loggerSize) + 1).ToString();
        TMP_PatternSet.text = (currentPatternSet + 1).ToString();
        TMP_StackValue.text = currentStack.ToString();
    }

    IEnumerator KeyboardResponseCycle()
    {
        while (true)
        {
            //Keyboard Responses
            if (Input.GetKeyDown(removeNote))
                RemoveNoteObject();

            if (Input.GetKeyDown(keyCodeStackDictionary.GetKey(1)))
            {
                KeyCode key = keyCodeStackDictionary.GetKey(1);
                currentStack = keyCodeStackDictionary.GetValue(key);
                Instance.RefreshLog();
                ShowMarksInPatternSet();
            }

            if (Input.GetKeyDown(keyCodeStackDictionary.GetKey(2)))
            {
                KeyCode key = keyCodeStackDictionary.GetKey(2);
                currentStack = keyCodeStackDictionary.GetValue(key);
                Instance.RefreshLog();
                ShowMarksInPatternSet();
            }

            if (Input.GetKeyDown(keyCodeStackDictionary.GetKey(3)))
            {
                KeyCode key = keyCodeStackDictionary.GetKey(3);
                currentStack = keyCodeStackDictionary.GetValue(key);
                Instance.RefreshLog();
                ShowMarksInPatternSet();
            }

            if (Input.GetKeyDown(keyCodeStackDictionary.GetKey(4)))
            {
                KeyCode key = keyCodeStackDictionary.GetKey(4);
                currentStack = keyCodeStackDictionary.GetValue(key);
                Instance.RefreshLog();
                ShowMarksInPatternSet();
            }
            yield return null;
        }
    }


    void Init(int size)
    {
        StartCoroutine(KeyboardResponseCycle());

        #region BlockUI Setup
        blocks = new GameObject[size];
        float width = 0;
        for (int iter = 0; iter < size; iter++)
        {
            blocks[iter] = Instantiate(prefab, transform);
            blocks[iter].transform.position = transform.position + new Vector3(1.05f * iter, 0f, 0f);
            blocks[iter].transform.rotation = Quaternion.identity;
            blocks[iter].transform.localScale = Vector3.one;
            width += blocks[iter].GetComponent<RectTransform>().rect.width + 1.05f;

            blocks[iter].GetComponent<Button>().image.color = iter == 0 ? c_currentTick :
                (int)loggerSize != 4 && iter % ((int)loggerSize / 8) != 0 ? c_emptyTickGrey :
                iter % ((int)loggerSize / 4) != 0 ? c_emptyTickDarkGrey :
                c_emptyTick;
        }
        #endregion

        #region PatternSets Initialization
        //With the given information from BPM, we have to see how many times we'll go over each measure
        //starting when the song begins (or where our offset is)

        //This already gives us how many seconds it takes to go between each beat.
        var tickValue = inGameTime / (60f / (bpm * rate * ((float)loggerSize / (float)LoggerSize.WHOLE)));

        //In order to achieve how many pattern set's we need, we need to take the sample rate of the some
        //get the song length in samples, and iterate as much as we can until we're beyond the song length
        //The number we get is the possible amount of PatternSet we can have when creating the Songmap.
        var sampleRate = 1f;
        var songLength = RoftPlayer.musicSource.clip.length - offsetInSeconds;
        var trackPosition = 0f;

        var totalPatternSets = 0f;
        while (trackPosition < songLength)
        {
            trackPosition += sampleRate;

            var samplesPerBeat = trackPosition / (60f / (bpm * rate * ((float)loggerSize / (float)LoggerSize.WHOLE)));

            totalPatternSets = Mathf.Floor(samplesPerBeat / (float)loggerSize);
        }

        for (int i = 0; i < totalPatternSets; i++)
        {
            PatternSet newSet = new PatternSet();
            newSet.Init((int)loggerSize);
            patternSets.Add(newSet);
        }
        #endregion
    }

    void Tick()
    {
        for (int iter = 0; iter < blocks.Length; iter++)
        {
            if (tick % (int)loggerSize == iter)
            {
                blocks[iter].GetComponent<Button>().image.color = c_currentTick;
            }
            else
            {
                blocks[iter].GetComponent<Button>().image.color = (int)loggerSize != 4 && iter % ((int)loggerSize / 8) != 0 ? c_emptyTickGrey :
                iter % ((int)loggerSize / 4) != 0 ? c_emptyTickDarkGrey :
                c_emptyTick;

            }
        }

        ShowMarksInPatternSet();
    }

    void RefreshLog()
    {
        UpdateUI();
        var _objects = from o in objects orderby o.GetInitialeSample() select o;
        objects = _objects.ToList();
    }

    public static void LogInNoteObject(NoteTool note, params object[] args)
    {
        switch (note)
        {
            case NoteTool.TAP:

                Instance.keyData = (int)args[0];
                Instance.sampleData = (long)args[1];
                Instance.typeData = (int)args[2];

                TapObj tapObj = new TapObj((uint)Instance.keyData, Instance.sampleData);

                LogIntoSet(tapObj);
                return;

            case NoteTool.HOLD:
                Instance.keyData = (int)args[0];
                Instance.sampleData = (long)args[1];
                Instance.typeData = (int)args[2];
                Instance.finishSample = (long)args[3];

                HoldObj holdObj = new HoldObj((uint)Instance.keyData, Instance.sampleData, Instance.finishSample);

                LogIntoSet(holdObj);
                return;

            case NoteTool.TRACK:
                Instance.keyData = (int)args[0];
                Instance.sampleData = (long)args[1];
                Instance.typeData = (int)args[2];

                //TODO: Start adding points
                return;

            case NoteTool.BURST:
                Instance.keyData = (int)args[0];
                Instance.sampleData = (long)args[1];
                Instance.typeData = (int)args[2];
                Instance.burstDirection = (int)args[3];

                BurstObj burstObj = new BurstObj((uint)Instance.keyData, Instance.sampleData, (uint)Instance.burstDirection);

                LogIntoSet(burstObj);
                return;

            default:
                return;
        }


    }

    static void LogIntoSet(NoteObj obj)
    {
        Instance.patternSets[(int)Instance.currentPatternSet].LogObject(obj, (int)Instance.currentStack.ZeroBased(), ((int)Instance.tick % (int)Instance.loggerSize));
        Instance.RefreshLog();
        ShowMarksInPatternSet();
    }

    public void RemoveNoteObject()
    {
        Instance.patternSets[(int)Instance.currentPatternSet].RemoveObject((int)currentStack.ZeroBased(), ((int)Instance.tick % (int)Instance.loggerSize));
        Instance.RefreshLog();
        ShowMarksInPatternSet();
    }

    public void ChangeNoteToolType(int value)
    {
        noteTool = (NoteTool)value;
    }

    public void SaveToFile()
    {
        //Write all data collected 
        StringBuilder data = new StringBuilder();

        foreach (PatternSet set in Instance.patternSets)
        {
            data.Append(set.ExtractData());
        }

        StackSavedData(data.ToString());

        RoftIO.CreateNewRFTM(RoftCreator.filename, RoftCreator.newSongDirectoryPath);
    }

    static void StackSavedData(string data)
    {
        ObjectData = data;
    }

    public static int GetObjectCount() => Instance.objects.Count + 1;

    public static void ShowMarksInPatternSet()
    {
        //Given the PatternSet, we have to look through all blocks in that pattern set, and
        //mark the blocks accordingly
        for (int cell = 0; cell < Instance.patternSets[(int)Instance.currentPatternSet].NoteStacks[(int)Instance.currentStack.ZeroBased()].Size(); cell++)
        {
            int index = (int)Instance.currentStack.ZeroBased();

            NoteStack stack = Instance.patternSets[(int)Instance.currentPatternSet].NoteStacks[index];

            if (stack != null && !stack.IsCellEmpty(cell))
                Instance.blocks[cell].GetComponent<Button>().image.color = Instance.c_tickNoteData;

            else if (Instance.tick % (int)Instance.loggerSize == cell)
                Instance.blocks[cell].GetComponent<Button>().image.color = Instance.c_currentTick;

            else
            {
                Instance.blocks[cell].GetComponent<Button>().image.color = (int)Instance.loggerSize != 4 && cell % ((int)Instance.loggerSize / 8) != 0 ? Instance.c_emptyTickGrey :
                cell % ((int)Instance.loggerSize / 4) != 0 ? Instance.c_emptyTickDarkGrey :
                Instance.c_emptyTick;
            }
        }
    }

    public static void MarkTickAsTimeAlter()
    {
        Instance.blocks[(int)Instance.tick].GetComponent<Button>().image.color = Instance.c_tickTimeData;
    }

    public static long GetFinishSample() => Instance.finishSample;
    public static List<TrackPoint> GetTrackPoints() => Instance.trackPoints;
    public static int GetBurstDirection() => Instance.burstDirection;
    public static bool IsNull() => Instance == null;
}
