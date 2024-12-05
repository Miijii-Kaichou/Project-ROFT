using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using ROFTIO = ROFTIOMANAGEMENT.RoftIO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static bool inSong = true;

    public static string[] accuracyString =
   {
        "Perfect",
        "Great",
        "Good",
        "Ok",
        "Miss"
    };

    public static int[] accuracyScore =
    {
        300,
        100,
        50,
        10,
        0
    };

    public static float[] stressAmount =
    {
       0.2f,
        0f,
        -0.1f,
        -0.2f,
        -0.3f
    };

    public static int sentScore;

    public static float sentStress;

    public static int consecutiveMisses;

    public static bool SongsNotFound { get; set; } = false;

    /*There's 3 modes:
     * STANDARD will be Keyboard and Mouse incorporated
     * KEY_ONLY will be what I've been having for ages
     * And TBR refers to "Type-By-Region"
     */
    [SerializeField]
    private SongList SongList;

    [SerializeField]
    public Sprite unknownSongCover;

    //What you can do to a song
    enum SongSelectionMode
    {
        Play,
        Edit
    }

    private static SongSelectionMode songMode;
    public static uint SongMode
    {
        get
        {
            return (uint)songMode;
        }
        set
        {
            songMode = (SongSelectionMode)value;
        }
    }


    [Header("Game Pause Overlay")]
    [SerializeField] private bool isGamePaused = false;
    public bool IsGamePaused
    {
        get
        {
            return isGamePaused;
        }
    }

    [SerializeField] private GameObject PAUSE_OVERLAY = null;

    [Header("UI TEXT MESH PRO")]
    [SerializeField] private TextMeshProUGUI TM_SONGNAME = null;
    [SerializeField] private TextMeshProUGUI TM_TOTALNOTES = null;
    [SerializeField] private TextMeshProUGUI TM_TOTALKEYS = null;
    [SerializeField] private TextMeshProUGUI TM_SCORE = null;
    [SerializeField] private TextMeshProUGUI TM_MAXSCORE = null;
    [SerializeField] private TextMeshProUGUI TM_COMBO = null;
    [SerializeField] private TextMeshProUGUI TM_COMBO_UNDERLAY = null;
    [SerializeField] private TextMeshProUGUI TM_MAXCOMBO = null;
    [SerializeField] private TextMeshProUGUI TM_DIFFICULTY = null;
    [SerializeField] private TextMeshProUGUI TM_PERFECT = null;
    [SerializeField] private TextMeshProUGUI TM_GREAT = null;
    [SerializeField] private TextMeshProUGUI TM_GOOD = null;
    [SerializeField] private TextMeshProUGUI TM_OK = null;
    [SerializeField] private TextMeshProUGUI TM_MISS = null;
    [SerializeField] private TextMeshProUGUI TM_ACCURACYPERCENTILE = null;
    [SerializeField] private TextMeshProUGUI TM_AUTOPLAY = null;
    public TextMeshProUGUI TM_ACCURACYGRADE = null;
    [SerializeField] private TextMeshProUGUI DEBUG_FILEDIR = null;

    [Header("UI IMAGES")]
    [SerializeField] private Image IMG_STRESS = null;
    [SerializeField] private Image IMG_STRESSBACKGROUND = null;
    [SerializeField] private Image IMG_SCREEN_OVERLAY = null;
    [SerializeField] private Image IMG_PROGRESSION_FILL = null;

    [Header("In Game Background")]
    [SerializeField] private Image IMG_INGAMEBACKGROUND = null;

    [Header("In-Game Statistics and Values")]
    private long totalScore;
    private long previousScore; //This will be used for a increasing effect
    public int Combo { set; get; }
    public static Color LogNormalColor { get; internal set; }
    public float GAME_DELTA { get; private set; } = 0.01f;

    private int maxCombo;
    private float overallAccuracy = 100.00f; //The average accuracy during the song
    private float accuracyPercentile; //The data in which gets accuracy in percent;
    private float overallGrade = 0f;
    [Range(1f, 10f)] public float stressBuild = 5f;
    public int[] accuracyStats = new int[5];
    public bool isAutoPlaying;
    public bool isNoFail;

    private readonly int reset = 0;

    private int initialGain = 1;

    //Time value
    private float inputDelayTime = 0;

    private readonly float multiInputDuration = 0.1f;

    //Check for multiple input
    public static int multiInputValue;

    [Header("In-Game Control Keys")]
    readonly KeyCode[] inGameControlKeys = {
        KeyCode.Backspace,
        KeyCode.Escape,
        KeyCode.Q,
        KeyCode.P,
        KeyCode.Semicolon,
        KeyCode.Comma,
        KeyCode.F5
    };

    //This will be used for the GameManager to assure that
    //when we restart, all approach circles are inactive
    private readonly List<GameObject> activeApproachObjects = new List<GameObject>();

    //Countdown for player to prepare
    public bool isCountingDown = false;

    private delegate void Main();
    private Main core;

    EventManager.Event scoutingDelegate;
    internal static bool ErrorDetected = false;
    public bool detected;

    private void Awake()
    {
        //Check for existing files and directories
        VerifyDirectories();

        #region Singleton
        if (Instance == null)
        {
            Instance = this;
            CreateRecords();
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(gameObject);
        #endregion

        /*I want to verify the existence of certain files whenever it runs on any computer.
         If the files do not exist, those files will automatic be generated back in.*/

        //Multicasting core delegate
        //When Invoked, it'll run all of these functions.
        //NEVER USE AN = SIGN WITH MUTLITASKING
        //It'll reset in reseting all castings that
        //it had in order to add a new one.
        #region Core Delegate MultiCasting
        core += RunScoreSystem;
        core += RunUI;
        core += RunInGameControls;
        core += CheckSignsOfInput;
        core += CheckStressMaxed;
        #endregion
    }

    private void OnEnable()
    {
        if (IMG_STRESS != null) IMG_STRESS.fillAmount = 0f;

        StartCoroutine(RUN_GAME_MANAGEMENT());
        StartCoroutine(StressBuildRoutine());
    }

    private void Update()
    {
        detected = ErrorDetected;
    }

    //GAME_MANAGEMENT Update
    IEnumerator RUN_GAME_MANAGEMENT()
    {
        while (true)
        {

            //Invoke core and all methods associated with it
            core.Invoke();

            yield return null;
        }
    }

    void RunScoreSystem()
    {
        //Raising effect
        if (previousScore < totalScore && !isGamePaused)
        {
            previousScore += Combo ^ initialGain;
            initialGain += Combo;
        }
        else
        {
            previousScore = totalScore;
            initialGain = reset + 1;
        }
    }

    void UpdateMaxCombo()
    {
        if (Combo > maxCombo) maxCombo = Combo;
    }

    void RunUI()
    {

        TM_SCORE.text = previousScore.ToString("D10");
        TM_COMBO.text = "x" + Combo.ToString();
        TM_COMBO_UNDERLAY.text = "x" + Combo.ToString();
        TM_DIFFICULTY.text = "DIFFICULTY: " + MapReader.GetDifficultyRating().ToString("F2", CultureInfo.InvariantCulture);
        TM_MAXSCORE.text = "MAX SCORE:     " + MapReader.GetMaxScore().ToString();
        
        //This will be temporary
        #region DEBUG_STATS_UI
        TM_PERFECT.text = "PERFECT:   " +
            accuracyStats[0].ToString() +
            " (" + Mathf.Floor((accuracyStats[0] / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_GREAT.text = "GREAT:       " +
            accuracyStats[1].ToString() +
            " (" + Mathf.Floor((accuracyStats[1] / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_GOOD.text = "GOOD:         " +
            accuracyStats[2].ToString() +
            " (" + Mathf.Floor((accuracyStats[2] / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_OK.text = "OK:            " +
            accuracyStats[3].ToString() +
            " (" + Mathf.Floor((accuracyStats[3] / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_MISS.text = "MISSES:       " +
            accuracyStats[4].ToString() +
            " (" + Mathf.Floor((accuracyStats[4] / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_MAXCOMBO.text = "MAX COMBO:     "
            + maxCombo.ToString() +
            " (" + Mathf.Floor((maxCombo / MapReader.GetTotalNotes()) * 100).ToString("F0", CultureInfo.InvariantCulture) + "%)";

        TM_ACCURACYPERCENTILE.text = "ACCURACY: "
           + overallAccuracy.ToString("F2", CultureInfo.InvariantCulture) + "%";
        #endregion

        //Be able to enable and disable Pause_Overlay
        if (isCountingDown == false)
            PAUSE_OVERLAY.SetActive(isGamePaused);
    }

    IEnumerator StressBuildRoutine()
    {
        while (true)
        {
            try
            {
                if (!RoftPlayer.IsNull() &&
                    RoftPlayer.musicSource.isPlaying &&
                    SongProgression.isPassedFirstNote &&
                    !SongProgression.isFinished)
                    ManageStressMeter();
            }
            catch { }

            yield return new WaitForSeconds(1f / 60f);
        }
    }

    void ManageStressMeter()
    {
        const uint millsInSec = 1000;
        float subtleProgression = (stressBuild / millsInSec);

        IMG_STRESS.fillAmount += (subtleProgression / (consecutiveMisses + stressBuild)) - sentStress;

        //Okay, so I have to think about this....
        //I think I have an idea for the Stress Build...
        /*I have two options for how this system should work
         * 
         * 1) Stress Build will be the multiplier for the sentStress, meaning I'll use
         *    sentStress * stressBuild
         *    
         *    They will definitely be added to the consecutiveMisses, because I want to get progressively worse
         *    as the player misses more.
         *    
         *    As for the subtle increase, stress progressiveness will be the stress build divided by
         *    the amount of milliseconds in a second (which is 1000)
         */
        sentStress = 0;
    }

    void RunInGameControls()
    {
        if (Input.GetKeyDown(inGameControlKeys[0]))
            RestartSong();

        if (Input.GetKeyDown(inGameControlKeys[1]))
        {
            switch (isGamePaused)
            {
                case false:
                    if (RoftPlayer.musicSource.isPlaying)
                        Pause();
                    return;

                case true:
                    UnPause();
                    return;
            }
        }

        if (Input.GetKeyDown(inGameControlKeys[6]))
            ExecuteScouting();
    }

    public void RestartSong()
    {
        inSong = true;

        MapReader.GetReaderType<TapObjectReader>().SequencePositionReset();
        MapReader.GetReaderType<HoldObjectReader>().SequencePositionReset();
        MapReader.GetReaderType<BurstObjectReader>().SequencePositionReset();

        for (int stat = 0; stat < accuracyStats.Length; stat++)
            accuracyStats[stat] = reset;

        //Reset all values
        Combo = reset;
        maxCombo = reset;
        totalScore = reset;
        sentScore = reset;
        IMG_STRESS.fillAmount = reset;
        sentStress = reset;
        consecutiveMisses = reset;
        accuracyPercentile = reset;
        overallAccuracy = reset;
        RoftPlayer.musicSource.timeSamples = reset;

        //Now we make sure all approach circles are inactive
        CollectApproachCircles();
        foreach (var activeCirlces in activeApproachObjects)
            activeCirlces.SetActive(false);

        //Now we clear our list.
        activeApproachObjects.Clear();


        SongProgression.ResetProgression();
    }

    public void Pause()
    {
        //When we pause, we have to stop music, and turn isPaused to true
        if (RoftPlayer.musicSource.isPlaying)
        {
            RoftPlayer.PauseMusic();
            isGamePaused = true;
            Time.timeScale = 0;
        }
        return;
    }

    public void UnPause(bool countdown = true)
    {
        //Now we give the player some time to prepare
        if (countdown)
            StartCoroutine(CountDown());
        else
        {
            RestartSong();
            isGamePaused = false;
            isCountingDown = false;
            PAUSE_OVERLAY.SetActive(isGamePaused);
            //Set time scale back to 1, so everything should show motion over time
            Time.timeScale = 1;
        }
        return;
    }

    public void UpdateScore()
    {
        UpdateMaxCombo();
        RankSystem.UpdateGrade();
        initialGain++;
        totalScore += sentScore * Combo;
    }



    public int GetSumOfStats()
    {
        //Do a calculation on accuracy stats
        int sumOfStats = 0;
        foreach (int value in accuracyStats)
        {
            sumOfStats += value;
        }
        return sumOfStats;
    }

    public static void SetInGameBackground(Sprite sprite)
    {
        try
        {
            //If the background image is inactive, set it active
            if (sprite != null)
            {
                Instance.IMG_INGAMEBACKGROUND.enabled = true;
                Instance.IMG_INGAMEBACKGROUND.sprite = sprite;
                Instance.IMG_INGAMEBACKGROUND.color = sprite.name == "NoCoverImage" ? Color.black : new Color(1f, 1f, 1f, 46f / 255f);
            }
        }
        catch
        {
            Debug.Log("Can't set an image if it doesn't exist");
        }
    }

    void CheckSignsOfInput()
    {
        try
        {
            multiInputValue = MouseEvent.Instance.GetMouseInputValue() + KeyPress.Instance.GetKeyPressInputValue();

            //Check for second input
            if (multiInputValue > 0)
                StartMultiInputDelay();
        }
        catch { }
    }

    void StartMultiInputDelay()
    {
        inputDelayTime += Time.deltaTime;
        if (inputDelayTime > multiInputDuration)
            ResetMultiInputDelay();
    }

    void CheckStressMaxed()
    {
        if (IMG_STRESS.fillAmount >= 0.99f && isNoFail == false)
            RestartSong();
    }

    void CollectApproachCircles()
    {
        GameObject[] discoveredObjs = GameObject.FindGameObjectsWithTag("approachCircle");
        foreach (var obj in discoveredObjs)
        {
            activeApproachObjects.Add(obj);
        }
    }

    public void ResetMultiInputDelay()
    {
        KeyPress.Instance.keyPressInput = reset;
        MouseEvent.Instance.mouseMovementInput = reset;
        inputDelayTime = reset;
    }

    public IEnumerator CountDown()
    {
        isGamePaused = false;
        isCountingDown = true;
        PAUSE_OVERLAY.SetActive(isGamePaused);
        for (int num = 3; num > reset; num--)
        {
            AudioManager.Play("Tick", _oneShot: true);
            yield return new WaitForSecondsRealtime(1f);
        }
        //Now we play music again, and stat
        RoftPlayer.PlayMusic();

        //Update isGamePaused
        isGamePaused = !RoftPlayer.musicSource.isPlaying;

        //Set time scale back to 1, so everything should show motion over time
        Time.timeScale = 1;

        isCountingDown = false;

        yield return null;
    }

    public bool IsInteractable() => (isGamePaused == false && isCountingDown == false);

    public void ExecuteScouting()
    {


        //We'll be using the EventManager to perform this method.
        //We have to create a Callback Method (aka; our delegate)
        //We store the function we want into our delegate
        scoutingDelegate = EventManager.AddNewEvent(000, "ON_BEGIN_SCOUT", () =>
        {
            RoftScouter.OnStart();
        }
        );

        //And then we add our delegate (which plays as a listner)


        //Now we execute it, and then remove it.
        EventManager.TriggerEvent("ON_BEGIN_SCOUT");
        EventManager.RemoveEvent("ON_BEGIN_SCOUT");
    }

    void CreateRecords()
    {
        //This is when records.rft is nowhere to be seen
        string roftRecordPath = Application.persistentDataPath + @"/records.rft";
        if (!File.Exists(roftRecordPath))
        {
            File.Create(roftRecordPath);
            Debug.Log(roftRecordPath + " created.");
        }
    }

    public void SetCombo(int _value) => Combo = _value;

    public void IncrementCombo() => Combo++;

    public static void UpdateLogColor(Color newColor)
    {
        LogNormalColor = newColor;
    }

    public static void DelayAction(float duration, Action action)
    {
        IEnumerator delay = WaitFor(duration, action);
        Instance.StartCoroutine(delay);
    }
    static IEnumerator WaitFor(float duration, Action action)
    {
        yield return new WaitForSecondsRealtime(duration);
        action.Invoke();
    }
    public static int GetCombo() => Instance.Combo;
    public static int GetMaxCombo() => Instance.maxCombo;
    public static SongList GetSongList() => Instance.SongList;

    #region Get Methods
    public Image GetSongProgressionFill() => IMG_PROGRESSION_FILL;

    public static long GetScore() => Instance.totalScore;
    public static int[] GetAccuracyStats() => Instance.accuracyStats;
    public static float GetOverallAccuracy() => Instance.overallAccuracy;

    public TextMeshProUGUI GetTMCombo() => TM_COMBO;

    public TextMeshProUGUI GetTMComboUnderlay() => TM_COMBO_UNDERLAY;

    public Image GetScreenOverlay() => IMG_SCREEN_OVERLAY;

    /// <summary>
    /// Check if the game's directory are present in PersistantPath
    /// </summary>
    public void VerifyDirectories()
    {
        //Check if song directory exists
        if (!ROFTIO.DirectoryExists(ROFTIO.GAME_DIRECTORY + @"/Songs"))
        {
            ROFTIO.GenerateDirectory(ROFTIO.GAME_DIRECTORY + @"/Songs");
        }
    }

    /// <summary>
    /// Turn on Cursor
    /// </summary>
    public static void TurnOnCursor() => Cursor.visible = true;

    /// <summary>
    /// Turn off Cursor
    /// </summary>
    public static void TurnOffCursor() => Cursor.visible = false;

    public static void ChangeSongSelectionMode(uint value)
    {
        SongMode = (value == 0 || value == 1) ? value : (value / value) - 1;
    }

    public static float GetAccuracyPercentile() => Instance.accuracyPercentile;
    public static void UpdateAccuracyPercentile(float value, bool relative = false, bool comboBreak = false)
    {
        Instance.accuracyPercentile = relative ? Instance.accuracyPercentile + value : value;
        Instance.overallAccuracy = Instance.accuracyPercentile / Instance.GetSumOfStats();
        if(comboBreak == false) Instance.UpdateScore();
    }

    public static TextMeshProUGUI GetAutoplayText() => Instance.TM_AUTOPLAY;
    #endregion
}