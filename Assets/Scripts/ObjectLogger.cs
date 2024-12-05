using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ROFTIOMANAGEMENT;
using System;
using System.Text;
using Extensions;
using System.Collections;
using UnityEngine.EventSystems;

public class ObjectLogger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static ObjectLogger Instance;

    [Serializable]
    public class NoteStack
    {
        public NoteObj[] NoteObjs;
        int stackValue = 0;

        public void Init(int size, float initSample)
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



        public void Init(int size, float initSample)
        {

            NoteStacks = new NoteStack[MAX_OBJECT_STACK];

            for (int i = 0; i < NoteStacks.Length; i++)
            {
                NoteStacks[i] = new NoteStack();
                NoteStacks[i].Init(size, initSample);
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

    [SerializeField]
    private TextMeshProUGUI TMP_Tick;
    
    [SerializeField]
    private TextMeshProUGUI TMP_PatternSet;
    
    [SerializeField]
    private TextMeshProUGUI TMP_StackValue;

    [SerializeField]
    private Key_Layout _keyLayout;

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

    [SerializeField]
    private LoggerSize loggerSize = LoggerSize.WHOLE;

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
    int _keyData;
    long _sampleData;
    int _typeData;

    //Unique data
    long _finishSample; //For hold type
    List<TrackPoint> _trackPoints; //For track type
    int _burstDirection; //For burst type
    List<float> _fixedSampleData = new List<float>();

    string dataFormat;

    public static NoteTool noteTool;
    public static float TickValue { get; private set; }
    float currentTick;
    public static float CurrentPatternSet { get; private set; }
    public static float CurrentStack { get; private set; } = 1;

    bool sequenceDone = false;

    public static string ObjectData;

    //Keyboard controls
    readonly Dictionary<KeyCode, int> keyCodeStackDictionary = new Dictionary<KeyCode, int>();
    readonly KeyCode removeNote = KeyCode.Backspace;

    bool enableScrolling = false;

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

    IEnumerator LoggerDataCycle()
    {
        while (true)
        {


            if (enableScrolling)
            {
                try
                {
                    float scrollDirection = Input.GetAxis("Mouse ScrollWheel");

                    if (scrollDirection != 0)
                    {
                        TickValue += Mathf.Sign(scrollDirection);
                        RefreshLoggerData();
                        RoftPlayer.musicSource.timeSamples = (int)_fixedSampleData[(int)currentTick];
                        CheckTick();
                    }
                    else Next();
                }
                catch { }
            }

            else Next();
            yield return null;

        }
    }

    void Next()
    {
        if (RoftPlayer.musicSource.isPlaying)
        {
            inGameTime = RoftPlayer.musicSource.time - offsetInSeconds;

            TickValue = inGameTime / (60f / (bpm * rate * ((float)loggerSize / (float)LoggerSize.WHOLE)));

            RefreshLoggerData();

            CheckTick();
        }
    }

    void UpdateUI()
    {
        TMP_Tick.text = ((tick % (int)loggerSize) + 1).ToString();
        TMP_PatternSet.text = (CurrentPatternSet + 1).ToString();
        TMP_StackValue.text = CurrentStack.ToString();
    }

    void CheckTick()
    {
        if (tick != currentTick && inGameTime > 0)
        {
            tick = currentTick;
            UpdateUI();
            if (Mathf.Floor(inGameTime % (60f / bpm)) == 0)
                Tick();
        }
    }

    IEnumerator KeyboardResponseCycle()
    {
        while (true)
        {
            //Keyboard Responses
            if (Input.GetKeyDown(removeNote))
                RemoveNoteObject();

            foreach (KeyValuePair<KeyCode, int> input in keyCodeStackDictionary)
            {
                if (Input.GetKeyDown(input.Key))
                {
                    KeyCode key = input.Key;
                    CurrentStack = input.Value;
                    Instance.RefreshLog();
                    ShowMarksInPatternSet();
                }
            }
            yield return null;
        }
    }

    IEnumerator ScrollingCycle()
    {
        while (true)
        {

            yield return new WaitForEndOfFrame();
        }
    }

    void RefreshLoggerData()
    {
        currentTick = Mathf.Floor(TickValue);
        CurrentPatternSet = Mathf.Floor(TickValue / (float)loggerSize);
    }

    public void OnPointerEnter(PointerEventData pe)
    {
        enableScrolling = true;

    }

    public void OnPointerExit(PointerEventData pe)
    {
        enableScrolling = false;
    }


    void Init(int size)
    {
        StartCoroutine(LoggerDataCycle());
        StartCoroutine(KeyboardResponseCycle());
        StartCoroutine(ScrollingCycle());

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


        //In order to achieve how many pattern set's we need, we need to take the sample rate of the some
        //get the song length in samples, and iterate as much as we can until we're beyond the song length
        //The number we get is the possible amount of PatternSet we can have when creating the Songmap.
        float sampleRate = 0.000075f;
        float songLength = RoftPlayer.musicSource.clip.length - offsetInSeconds;
        float trackPosition = 0f;

        float totalPatternSets = 0f;
        float tickValue = 0f;


        while (trackPosition < songLength)
        {
            trackPosition += sampleRate;
            float timePosition = trackPosition - offsetInSeconds;
            if (timePosition >= offsetInSeconds)
            {
                float samplesPerBeat = Mathf.Round(trackPosition / (60f / (bpm * rate * ((float)loggerSize / (float)LoggerSize.WHOLE))));

                if (tickValue != samplesPerBeat && samplesPerBeat % 1 == 0)
                {
                    float sample = timePosition * RoftPlayer.musicSource.clip.frequency;
                    tickValue = samplesPerBeat;
                    _fixedSampleData.Add(sample);
                }

                totalPatternSets = Mathf.Round(samplesPerBeat / (float)loggerSize);
            }
        }

        for (int i = 0; i < totalPatternSets; i++)
        {
            PatternSet newSet = new PatternSet();
            newSet.Init((int)loggerSize, _fixedSampleData[i]);
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

                Instance._keyData = (int)args[0];
                Instance._sampleData = (long)args[1];
                Instance._typeData = (int)args[2];

                TapObj tapObj = new TapObj((uint)Instance._keyData, Instance._sampleData);

                LogIntoSet(tapObj);
                return;

            case NoteTool.HOLD:
                Instance._keyData = (int)args[0];
                Instance._sampleData = (long)args[1];
                Instance._typeData = (int)args[2];
                Instance._finishSample = (long)args[3];

                HoldObj holdObj = new HoldObj((uint)Instance._keyData, Instance._sampleData, Instance._finishSample);

                LogIntoSet(holdObj);
                return;

            case NoteTool.TRACK:
                Instance._keyData = (int)args[0];
                Instance._sampleData = (long)args[1];
                Instance._typeData = (int)args[2];

                //TODO: Start adding points
                return;

            case NoteTool.BURST:
                Instance._keyData = (int)args[0];
                Instance._sampleData = (long)args[1];
                Instance._typeData = (int)args[2];
                Instance._burstDirection = (int)args[3];

                BurstObj burstObj = new BurstObj((uint)Instance._keyData, Instance._sampleData, (uint)Instance._burstDirection);

                LogIntoSet(burstObj);
                return;

            default:
                return;
        }


    }

    static void LogIntoSet(NoteObj obj)
    {
        Instance.patternSets[(int)CurrentPatternSet].LogObject(obj, (int)CurrentStack.ZeroBased(), ((int)Instance.tick % (int)Instance.loggerSize));
        Instance.RefreshLog();
        ShowMarksInPatternSet();
    }

    public void RemoveNoteObject()
    {
        Instance.patternSets[(int)CurrentPatternSet].RemoveObject((int)CurrentStack.ZeroBased(), ((int)Instance.tick % (int)Instance.loggerSize));
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

        Debug.Log(ObjectData);

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
        for (int cell = 0; cell < Instance.patternSets[(int)CurrentPatternSet].NoteStacks[(int)CurrentStack.ZeroBased()].Size(); cell++)
        {
            int index = (int)CurrentStack.ZeroBased();

            NoteStack stack = Instance.patternSets[(int)CurrentPatternSet].NoteStacks[index];

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

    public static long GetFinishSample() => Instance._finishSample;
    public static List<TrackPoint> GetTrackPoints() => Instance._trackPoints;
    public static int GetBurstDirection() => Instance._burstDirection;
    public static bool IsNull() => Instance == null;

    public static float Get_BPM() => Instance.bpm;
    public static float Get_Offset() => Instance.offsetInSeconds;

    static float CalculateDifficultyRating(int notesCounted)
    {
            int totalNotes = notesCounted;
            float songLengthInSec = RoftPlayer.musicSource.clip.length;
            float notesPerSec = (totalNotes / songLengthInSec);
            float totalKeys = (float)RoftCreator.GetTotalKeys();
            float approachSpeedInPercent = (float)NoteEffector.Instance.ApproachSpeed / 100;
            float gameModeBoost = 0;
            const int maxKeys = 30;

            float calculatedRating = notesPerSec +
                (totalKeys / maxKeys) +
                approachSpeedInPercent +
                (RoftPlayer.musicSource.pitch / 2) +
                gameModeBoost;

           return calculatedRating;
    }
}
