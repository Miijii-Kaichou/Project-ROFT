using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cakewalk.IoC;
using UnityEngine.UI;
using System;
//This is going to be an abstract class. This will be the base class of all other layouts in the game
public class Key_Layout : MonoBehaviour
{
    [Dependency]
    public static Key_Layout Instance;

    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;

    [SerializeField]
    private RectTransform _keyLayoutTransform;

    //So I want to be able to allow the player to freely keybind key layouts (even though there's hardly any reason besides 4x4, 8x8, and 12x12)
    //Other than that, I want to go through a process of getting all homerow, toprow, and bottomrow keys.
    //KeyLayout will be given an enumerator

    public bool recordKeyInput;

    #region Public Members

    public KeyLayoutType KeyLayout;

    public bool autoBindKeys = true;

    [Header("Data Ui")]
    public TextMeshProUGUI dataText;

    //After iterating through strings, we'll return a Input corresponding avaliable keys.
    public List<KeyCode> primaryBindedKeys = new List<KeyCode>();

    public static List<GameObject> keyObjects = new List<GameObject>();

    [Header("Creating Layout")]
    public GameObject key;
    public ObjectPooler pooler;

    #endregion

    #region Private Members

    //TODO: Create new readonly string for Burst Direction Keys
    //and for the Alternation Keys (the keys in the middle of each keyrow)

    //primaryLayout is the key layout where you control
    //both ends of the in-game layout
    private readonly string[] primaryLayout = new string[7]
    {
        "qwopasl;", // 2 x 4
        "qwopasl;zx./", // 3 x 4
        "1290qwopasl;zx./", // 4 x 4
        "qweiopasdkl;zxc,./", // 3 x 6
        "123890qweiopasdkl;zxc,./", // 4 x 6
        "qweruiopasdfjkl;zxcvm,./.", // 3 x 8
        "12347890qweruiopasdfjkl;zxcvm,./" // 4 x 8
    };

    private KeyConfig keyConfig;

    //newX and newY are for Abstract Layout
    private float newXPosition = 0f;
    private float newYPosition = 0f;
    private uint numCols = 0;
    private uint numRows = 0;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
            Instance = this;

        keyConfig = new KeyConfig();
        keyConfig = JsonUtility.FromJson<KeyConfig>(keyConfig.GetJSONString());
    }

    void InitiateAutoKeyBind()
    {
        //I want to first bind the primary layout
        for (int keyID = 0; keyID < primaryLayout[(int)KeyLayout].Length; keyID++)
            InvokeKeyBind(primaryLayout[(int)KeyLayout][keyID]);
    }

    //This will simply take any character, and keybind it.
    public KeyCode InvokeKeyBind(char m_char, bool _addToList = true)
    {

        KeyCode key;
        key = (KeyCode)m_char;

        if (!_addToList) return key;
        primaryBindedKeys.Add(key);

        return key;
    }

    //This takes any ASCII integer that exists on the keyboard
    //if the player so desires to manually keybind
    public KeyCode InvokeKeyBind(int m_int, bool _addToList = true)
    {
        KeyCode key;
        key = (KeyCode)m_int;

        if (!_addToList) return key;
        primaryBindedKeys.Add(key);

        return key;

    }

    public void SetUpLayout()
    {
        //So I figured out the problem to the build problem, and it has to do with the referencing of keys.
        //I don't want to manually do it for all of them, so instead, I'll create a function that'll do it for me.
        //Probably 2 or 3 times the work, but I don't have to click and drag stuff, and all that good stuff

        //We need a variable that spreads the individual keys out
        //Almost like Procedural Generating, but uniformed... I think...

        //I don't think I want it adjustable by the designer... but for the implementation of it, we will.

        //We do a double for loop! Columns and Rows (At least for the 8x8, 12x12, and 3Row

        if (RoftPlayer.Record)
            KeyLayout = RoftCreator.GetKeyLayout();

        //float xOffset = setXOffset[(int)keyLayout];
        //float yOffset = setYOffset[(int)keyLayout];

        float xOffset = keyConfig.GetXOffset[(int)KeyLayout];
        float yOffset = keyConfig.GetYOffset[(int)KeyLayout];

        GameObject newKey;

        //Determine how many columns and rows before setting up
        switch (KeyLayout)
        {
            case KeyLayoutType.Layout_2x4:
                numRows = 2; numCols = 4;
                break;

            case KeyLayoutType.Layout_3x4:
                numRows = 3; numCols = 4;
                break;

            case KeyLayoutType.Layout_4x4:
                numRows = 4; numCols = 4;
                break;

            case KeyLayoutType.Layout_3x6:
                numRows = 3; numCols = 6;
                break;

            case KeyLayoutType.Layout_4x6:
                numRows = 4; numCols = 6;
                break;

            case KeyLayoutType.Layout_3x8:
                numRows = 3; numCols = 8;
                break;

            case KeyLayoutType.Layout_4x8:
                numRows = 4; numCols = 8;
                break;

            default:
                break;
        }

        gridLayoutGroup.cellSize = new Vector2(keyConfig.keyHorizontalSpread[(int)KeyLayout], keyConfig.keyVerticalSpread[(int)KeyLayout]);
        gridLayoutGroup.constraintCount = (int)numCols;

        _keyLayoutTransform.localScale = new Vector3(keyConfig.keyLayoutScale[(int)KeyLayout], keyConfig.keyLayoutScale[(int)KeyLayout], 1f);

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                newKey = pooler.GetMember("keysEnhanced");

                ShowLetter letter = newKey.GetComponent<ShowLetter>();

                if (!newKey.activeInHierarchy)
                {
                    newKey.SetActive(true);
                }

                keyObjects.Add(newKey);

            }
        }

        //After setting up the keys,  bring them to center 
        //And then autobind keys
        for (int keyNum = 0; keyNum < keyObjects.Count; keyNum++)
        {
            //Check if these notes are interactable
            if (RoftPlayer.Record)
            {
                InteractableKey newInteractable = keyObjects[keyNum].gameObject.AddComponent<InteractableKey>();
                newInteractable.SetKeyNum(keyNum);
                keyObjects[keyNum].GetComponent<CircleCollider2D>().enabled = true;
            }

        }
        InitiateAutoKeyBind();
    }

    internal void Flush()
    {
        UnBindKeys();

        pooler.FlushPool();

        KeyLayout = default;
    }

    private void UnBindKeys()
    {
        //I want to first bind the primary layout
        foreach (GameObject keyObject in keyObjects)
        {
            keyObject.GetComponent<KeyId>().pooler.FlushPool();
        }
        keyObjects.Clear();
        primaryBindedKeys.Clear();
    }
}