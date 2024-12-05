using System.Collections.Generic;
using UnityEngine;

using ROFTIOMANAGEMENT;
using System.Collections;

public class NoteEffector : MonoBehaviour
{
    public static NoteEffector Instance;

    #region Serializable Private/Accessor Members
    [Header("Approach Speed"), Range(1.0f, 10.0f)]
    [SerializeField]
    private double approachSpeed;
    public double ApproachSpeed
    {
        set
        {
            approachSpeed = value;
        }
        get
        {
            return approachSpeed;
        }
    }

    [Header("Alignment")]
    [SerializeField]
    private float alignment = 0; //To shift notes whenever

    /*
     * Perfect - 0
     * Great - 1
     * Good - 2
     * Ok - 3
    */
    [Header("Accuracy"), Range(1f, 10f)]
    [SerializeField]
    private float accuracy = 5f; //5 is the average/standard accuracy in the game.
    public float Accuracy
    {
        set
        {
            accuracy = value;
        }
        get
        {
            return accuracy;
        }
    }

    //We get our images
    [Header("UI ASSET")]
    public GameObject notePrefab;
    #endregion

    #region Private Members
    private float[] accuracyVal = new float[4];
    public float[] AccuracyVal { get { return accuracyVal; } }

    private const int maxOffset = 100000;
    private const int minOffset = 10000;
    private const int standardAccuracy = 5;

    private readonly float[] accuracyDefault = new float[4]
    {
        4000,
        6000,
        10000,
        12000
    }; //Default Accurary; 2-4-2 Format
    #endregion

    #region Protected Members
    [SerializeField]
    protected MapReader mapReader;
    protected float percentage; //Lerping for effects
    protected int keyObjPosition = 0;
    protected float noteSpawnOffset; //When our note should start appearing
    protected int noteSample; //The note where you actually hit with timing
    protected int noteSampleForKey; //The time the key is at full capacity (for TBR Modes)
    protected KeyCode randomKey;
    protected GameObject lastKeySpawned;
    protected List<GameObject> spawnedKeysHistory = new List<GameObject>();
    protected GameObject targetKey;
    
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RetrieveEffectConfigs();
        UpdateAccuracyHarshness();
        StartCoroutine(SpawnCycle());
    }

    IEnumerator SpawnCycle()
    {
        while (true)
        {
            UpdateNoteOffset();
            UpdateAccuracyHarshness();

            if (!RoftPlayer.Record && RoftPlayer.musicSource != null)
            {
                if (ManageObjTypeSequence(MapReader.GetReaderType<TapObjectReader>()))
                    SpawnNoteObj(MapReader.GetReaderType<TapObjectReader>());

                if (ManageObjTypeSequence(MapReader.GetReaderType<HoldObjectReader>()))
                    SpawnNoteObj(MapReader.GetReaderType<HoldObjectReader>());

                if (ManageObjTypeSequence(MapReader.GetReaderType<BurstObjectReader>()))
                    SpawnNoteObj(MapReader.GetReaderType<BurstObjectReader>());
            }

            yield return null;
        }
    }

    /// <summary>
    /// Spawns a specified Note Object Type into
    /// the playing filed
    /// </summary>
    /// <param name="_objReader">Object Reader of a certain type.</param>
    void SpawnNoteObj(ObjectReader _objReader = null)
    {

        int sequencePos = (int)_objReader.GetSequencePosition();

        if (sequencePos < _objReader.objects.Count)
        {
            #region HELP?
            //This grabs our note objects from the "key object" based on what
            //note instance is occuring, as well as if it's 
            //the first "key" on the screen, or the third.
            //Most of this information is from the MapReader, in which that object
            //reads from the song file. 
            #endregion

            NoteObj objToBeSpawned = _objReader.objects[sequencePos];

            int objId = (int)objToBeSpawned.GetKey();

            ObjectPooler keyPooler = Key_Layout.keyObjects[objId].GetComponent<ObjectPooler>();

            //We want to get our game objects from the same key for both the approach circle, and the arrow
            GameObject approachCircle = _objReader.GetTypeFromPool(keyPooler);

            //We have two effects for the approach circle, and for the arrows
            CloseInEffect effect = approachCircle.GetComponent<CloseInEffect>();

            //Check if Approach Cirlce is not active
            if (!approachCircle.activeInHierarchy)
            {
                WakeUpNoteMember(ref approachCircle, _objReader);
                AssignPosition(effect, _objReader);
            }

            UpdateToNextNote(_objReader);
        }
    }

    /// <summary>
    /// This will wake up a note from the Game Key's object pooler
    /// </summary>
    /// <param name="_obj">This object must be a gameObject assigned from an ObjectPooler
    /// mainly with GetTypeFromPool()</param>
    /// <param name="_objReader">Object Reader of a certain type.</param>
    private void WakeUpNoteMember(ref GameObject _obj, ObjectReader _objReader)
    {
        int sequencePos = (int)_objReader.GetSequencePosition();

        NoteObj targetObj = _objReader.objects[sequencePos];

        GameObject currentKeyObject = Key_Layout.keyObjects[(int)targetObj.GetKey()];

        _obj.SetActive(true);
        _obj.transform.position = currentKeyObject.transform.position;
        _obj.transform.localScale = currentKeyObject.transform.localScale;
    }

    /// <summary>
    /// Change the sequence number of a specified Object Reader
    /// </summary>
    /// <param name="_objReader"></param>
    private void UpdateToNextNote(ObjectReader _objReader)
    {
        _objReader.Next();
    }

    /// <summary>
    /// Keep track of when a Note Object can be spawned onto
    /// the game view
    /// </summary>
    /// <param name="_objReader">Object Reader of a certain type.</param>
    /// <returns></returns>
    bool ManageObjTypeSequence(ObjectReader _objReader)
    {
        int sequencePos = (int)_objReader.GetSequencePosition();

        if (sequencePos < _objReader.objects.Count)
        {
            NoteObj targetObj = _objReader.objects[sequencePos];

            noteSample = (int)targetObj.GetInitialeSample() - (int)alignment;

            float offsetStart = noteSample - noteSpawnOffset;

            //This is strictly for checking when notes should appear
            if (RoftPlayer.musicSource.timeSamples > offsetStart)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Change the offset of when Note Object should
    /// spawn into the game view.
    /// </summary>
    void UpdateNoteOffset()
    {
        noteSpawnOffset = maxOffset - (minOffset * (((float)approachSpeed / 1.10f) - 1));
    }

    /// <summary>
    /// Change the window of accuracy when hitting
    /// Note Objects.
    /// </summary>
    void UpdateAccuracyHarshness()
    {
        for (int accuracyScoreIndex = 0; accuracyScoreIndex < accuracyVal.Length; accuracyScoreIndex++)
        {
            accuracyVal[accuracyScoreIndex] = accuracyDefault[accuracyScoreIndex] + (500 * (standardAccuracy - accuracy));
        }
    }

    /// <summary>
    /// Assign the spawned Note Object to a key in the
    /// game view, the inital time that it appears,
    /// and the point that the player should hit the
    /// respective key
    /// </summary>
    /// <param name="_effect">The effect responsible for the Note Objects closing in.</param>
    /// <param name="_objReader">Object Reader of a certain type.</param>
    void AssignPosition(CloseInEffect _effect, ObjectReader _objReader)
    {
        int sequencePos = (int)_objReader.GetSequencePosition();

        NoteObj noteObj;

        _effect.initiatedNoteSample = noteSample;
        _effect.initiatedNoteOffset = noteSpawnOffset;
        _effect.noteSpawnOffset = noteSample - noteSpawnOffset;
        _effect.accuracyVal = accuracyVal;

        //This is referring to list index in the MapReader
        _effect.keyNum = sequencePos;
        _effect.mapReaderSeqPos = _effect.keyNum;

        //This is the index regarding each "key" on screen
        noteObj = _objReader.objects[_effect.keyNum];

        _effect.keyNumPosition = (int)noteObj.GetKey();

        //Change look of circle
        _effect.Modifier.ChangeType((int)noteObj.GetNoteType());
    }

    /// <summary>
    /// Return the percentage of a Note Object.
    /// </summary>
    /// <returns></returns>
    protected virtual float GetPercentage() => 0f;

    /// <summary>
    /// Get effect configs from read RFTM File
    /// </summary>
    void RetrieveEffectConfigs()
    {
        if (RoftPlayer.Record == false)
        {
            //Get Position of Difficulty Tag in File
            int difficultyTag = RoftIO.InRFTMJumpTo("Difficulty", MapReader.GetName());

            //Use difficultyTag to read its property values
            accuracy = RoftIO.ReadPropertyFrom<float>(difficultyTag, "AccuracyHarshness", MapReader.GetName());
            approachSpeed = RoftIO.ReadPropertyFrom<float>(difficultyTag, "ApproachSpeed", MapReader.GetName());
        }
    }
}
