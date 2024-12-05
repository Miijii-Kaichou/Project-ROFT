using UnityEngine;
using TMPro;

public class RoftFPS : Singleton<RoftFPS>
{
    //FrameRate Options
    public enum FrameRate
    {
        FPS30 = 30,
        FPS60 = 60,
        FPS120 = 120,
        UNLIMITED = -1
    }

    //Shows the current fps of the game
    [SerializeField]
    private FrameRate targetFrameRate = FrameRate.FPS60;
    private static int fpsAccumulator = 0;
    private static float fpsNextPeriod = 0f;
    private static int currentFPS;

    //TextMeshPro
    [SerializeField]
    private TextMeshProUGUI fpsText;

    //Constants
    const float fpsMeasurePeriod = 0.5f;
    const string display = "[0] FPS";

    void Awake()
    {
        Application.targetFrameRate = (int)targetFrameRate;
    }

    // Start is called before the first frame update
    void Start()
    {
        fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        Application.targetFrameRate = (int)targetFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        //measure average frames per second
        fpsAccumulator++;

        if(fpsText != null && Time.realtimeSinceStartup > fpsNextPeriod)
        {
            currentFPS = (int)(fpsAccumulator / fpsMeasurePeriod);
            fpsAccumulator = 0;
            fpsNextPeriod += fpsMeasurePeriod;
            fpsText.text = "[" + currentFPS.ToString() + "] FPS";
        }
    }

    public static int GetCurrectFPS() => currentFPS;
}
