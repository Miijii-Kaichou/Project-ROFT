using UnityEngine;
using UnityEngine.UI;

public class CloseInEffect : NoteEffector
{
    public float initiatedNoteSample;
    public float initiatedNoteOffset;
    public float noteSpawnOffset;

    public int keyNumPosition; //So we know which key the notes are closing into
    public int keyNum; //We know what note we're on!!

    public GameObject attachedArrow;

    public Image image;
    public SpriteRenderer innerSprite;

    public int index;

    public string accuracyString;

    private readonly int m_break = 0;

    public bool dispose;

    const int possibleAccuracy = 4;

    public bool dontEffectMe = false;

    public int mapReaderSeqPos;

    public ObjectReader objReader;

    public Vector3 initScale;

    //keyInputDownReceived will be given a true for one frame
    //keyInputReceived will be given so long as it's being pressed down for
    private bool keyInputDownReceived, keyInputReceived;

    //Get Circle Modifier
    private CircleTypeModifier modifier;
    public CircleTypeModifier Modifier
    {
        get
        {
            return modifier;
        }
    }

    //So they don't have to look screwed up
    protected Color originalAppearance;

    private void Awake()
    {
        image = GetComponent<Image>();

        modifier = GetComponent<CircleTypeModifier>();
    }

    //Start
    private void OnEnable()
    {
        initScale = transform.localScale;
        initiatedNoteSample = noteSample;
        initiatedNoteOffset = base.noteSpawnOffset;
        keyNumPosition = mapReaderSeqPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!RoftPlayer.Record && GameManager.Instance.IsInteractable())
            StartClosingIn();

    }

    void FixedUpdate()
    {
        //Because Fixed Update goes faster than Update at a fixed rate, we're able to 
        //prevent getting multiple notes on 1 single tap.
        if (dispose && ClosestObjectClass.closestObject[keyNumPosition] != null & GameManager.Instance.IsInteractable())
        {
            if (attachedArrow != null)
            {
                attachedArrow.SetActive(false);
                attachedArrow = null;
            }

            ClosestObjectClass.closestObject[keyNumPosition].SetActive(false);
            ClosestObjectClass.closestObject[keyNumPosition] = null;
        }
    }

    void StartClosingIn()
    {
        var scale = new Vector3(1f/ GetPercentage(), 1f / GetPercentage(), 1f);
        transform.localScale = scale;

        if (RoftPlayer.musicSource.timeSamples < initiatedNoteSample)
            image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Sin((GetPercentage() * 2f) - 0.25f));
        else
            image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Sin((GetPercentage() * 2f) - 0.5f));

        InHitRange();

        if (CheckAutoPlay())
            RunAutoPlay();
        else 
            CheckTimeWindow();
    }

    void RunAutoPlay()
    {
        #region Auto Play
        if (RoftPlayer.musicSource.timeSamples > (initiatedNoteSample + AccuracyVal[3]))
        {
            if (ClosestObjectClass.closestObject[keyNumPosition] == null)
                ClosestObjectClass.closestObject[keyNumPosition] = gameObject;

            BreakComboChain();

            GameManager.consecutiveMisses++;

            dispose = true;
        }

        if (accuracyString == "Perfect")
        {
            if (ClosestObjectClass.closestObject[keyNumPosition] == null)
            {
                ClosestObjectClass.closestObject[keyNumPosition] = gameObject;

                Pulse();
                AudioManager.Play("Normal", 100, true);


                GameManager.Instance.IncrementCombo();
                SendAccuracyScore();
                BuildStress(index);

                GameManager.consecutiveMisses = 0;

                dispose = true;
            }
        }
        #endregion
    }

    void CheckTimeWindow()
    {
        CheckIfOutOfWindow();

        bool tapType = (MapReader.noteObjs[keyNum].GetNoteType() == NoteObj.NoteObjType.Tap &&
            DetectArrowKeyInput(0));

        bool BurstType = (MapReader.noteObjs[keyNum].GetNoteType() == NoteObj.NoteObjType.Burst &&
            attachedArrow != null &&
            DetectArrowKeyInput(1) &&
            GameManager.multiInputValue == 2);

        if (accuracyString != "" && (tapType || BurstType))
        {
            if (ClosestObjectClass.closestObject[keyNumPosition] == null)
            {

                if (gameObject.CompareTag("approachCircle")) ClosestObjectClass.closestObject[keyNumPosition] = gameObject;

                if (GetPercentage() > 0.99f) targetKey = gameObject;

                //Check type and add sound
                if (tapType)
                    AudioManager.Play("Normal", 100, true);

                if (BurstType)
                {
                    if (attachedArrow.GetComponent<ArrowDirectionSet>().Detect())
                        AudioManager.Play("Ding", 100, true);
                    else return;
                }

                IncrementComboChain();

                GameManager.consecutiveMisses = 0;

                GameManager.Instance.ResetMultiInputDelay();

                GameManager.multiInputValue = 0;

                SendAccuracyScore();

                BuildStress(index);

                Pulse();

                dispose = true;
            }
        }
    }

    private bool DetectArrowKeyInput(int index)
    {
        switch (index)
        {
            case 0:
                return keyInputDownReceived = Input.GetKeyDown(Key_Layout.Instance.primaryBindedKeys[keyNumPosition]);

            case 1:
                return keyInputReceived = Input.GetKey(Key_Layout.Instance.primaryBindedKeys[keyNumPosition]);

            default: break;
        }
        return false;
    }

    protected override float GetPercentage()
    {
        percentage = (RoftPlayer.musicSource.timeSamples - noteSpawnOffset) / (initiatedNoteSample - noteSpawnOffset);
        return percentage;
    }

    public string InHitRange()
    {
        for (int range = 0; range < AccuracyVal.Length; range++)
        {
            bool beforePerfect = RoftPlayer.musicSource.timeSamples >= (initiatedNoteSample) - (AccuracyVal[range]);
            bool afterPerfect = RoftPlayer.musicSource.timeSamples <= (initiatedNoteSample) + (AccuracyVal[range]);

            if (beforePerfect && afterPerfect)
            {
                accuracyString = GameManager.accuracyString[range];

                index = range;

                return accuracyString;
            }
        }
        return null;
    }

    public void SendAccuracyScore()
    {
        GameObject sign = GetComponentInParent<ObjectPooler>().GetMember("Signs");
        if (!sign.activeInHierarchy)
        {
            sign.SetActive(true);
            sign.transform.position = ClosestObjectClass.closestObject[keyNumPosition].transform.position;
            sign.GetComponent<AccuracySign>().ShowSign(index);

            GameManager.sentScore = GameManager.accuracyScore[index];

            GameManager.Instance.accuracyStats[index] += 1;
            float inverse = ((possibleAccuracy - (index)));
            float percent = inverse / possibleAccuracy;
            GameManager.UpdateAccuracyPercentile(percent * 100f, true);
        }
    }

    public void IncrementComboChain()
    {
        GameManager.Instance.IncrementCombo();

    }

    //This is unique from actually sending the accuracy score
    //There's different data to be handled and different actions to perform
    //which is why this may look like a copy-pasted SendAccuracyScore
    public void BreakComboChain()
    {
        GameManager.Instance.accuracyStats[4] += 1;
        GameManager.UpdateAccuracyPercentile((((possibleAccuracy - index) / possibleAccuracy) * 100), true, true);
        GameManager.Instance.SetCombo(m_break);

        GameObject sign = GetComponentInParent<ObjectPooler>().GetMember("Signs");

        if (!sign.activeInHierarchy)
        {
            sign.SetActive(true);
            sign.transform.position = gameObject.transform.position;
            sign.GetComponent<AccuracySign>().ShowSign(4);
        }

        BuildStress(4);
    }

    void CheckIfOutOfWindow()
    {
        if (RoftPlayer.musicSource.timeSamples > (initiatedNoteSample + AccuracyVal[3]))
        {
            if (ClosestObjectClass.closestObject[keyNumPosition] == null)
                ClosestObjectClass.closestObject[keyNumPosition] = gameObject;

            BreakComboChain();

            GameManager.consecutiveMisses++;

            dispose = true;
        }
    }

    public void BuildStress(int _index)
    {
        GameManager.sentStress = GameManager.stressAmount[_index] + (GameManager.Instance.stressBuild / 100f);
    }

    //We want to reset all values that may
    //show itself when it reactivates.
    //We want it to be clean and refresh when we respond
    private void OnDisable()
    {
        if (image != null)
            image.color = originalAppearance * new Color(1,1,1,0);

        if (innerSprite != null) innerSprite.color = originalAppearance;

        percentage = 0;
        index = 0;
        initiatedNoteSample = 0;
        initiatedNoteOffset = 0;
        noteSpawnOffset = 0;
        accuracyString = "";
        keyNum = 0; //We know what note we're on!!
        transform.localScale = initScale;
        dispose = false;
    }

    bool CheckAutoPlay() => GameManager.Instance.isAutoPlaying;

    void Pulse()
    {
        GameObject key;

        key = Key_Layout.keyObjects[keyNumPosition];
        key.GetComponent<PulseEffect>().DoPulseReaction(0.15f);
        key.GetComponentInChildren<PulseEffect>().DoPulseReaction(0.15f);

        CreateRipple();

        GameManager.Instance.GetTMCombo().GetComponent<PulseEffect>().DoPulseReaction(0.05f);
        GameManager.Instance.GetTMComboUnderlay().GetComponent<PulseEffect>().DoPulseReaction();
        GameManager.Instance.GetScreenOverlay().GetComponent<OverlayPulseEffect>().DoPulseReaction();
    }

    void CreateRipple()
    {
        GameObject key, effectObj;

        RippleEffect effect;

        ObjectPooler pooler;

        //Get the key object based on position
        key = Key_Layout.keyObjects[keyNumPosition];

        //Reference the object pooler so we can get the ripple effect
        pooler = key.GetComponent<ObjectPooler>();

        //Use the pooler object, and get the RippleEffect member
        effectObj = pooler.GetMember("RippleEffect");

        //Enable the effectObj
        if (!effectObj.activeInHierarchy)
        {
            effectObj.SetActive(true);
            effectObj.transform.position = transform.position;
            effectObj.transform.rotation = Quaternion.identity;
        }

        //Get the ripple effect component
        effect = effectObj.GetComponent<RippleEffect>();

        //do the ripple affect
        effect.DoRippleEffect(image.color);
    }
}