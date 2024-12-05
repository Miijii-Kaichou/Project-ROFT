using UnityEngine;
using TMPro;
using static Extensions.Convenience;
using UnityEngine.UI;
using Extensions;
using System.Collections.Generic;

[RequireComponent(typeof(Key_Layout))]
public class KeyPress : MonoBehaviour
{
    public static KeyPress Instance;

    //Taking the binded keys, and making them interactive
    [Cakewalk.IoC.Dependency]
    public Key_Layout key_Layout;

    public Sprite keyActive, keyInActive;

    public Color[] keyActiveColor = new Color[2];

    public TextMeshProUGUI debugText;

    //Input Value for KeyPress
    public int keyPressInput = 0;
    const int activeInput = 1;
    const int inactiveInput = 0;
    readonly Color defaultColor = Color.white;
    const float inactiveOpacity = 128f;
    const float activeOpacity = 255f;

    //Cache Info
    Dictionary<int, KeyControls> keyCache = new();


    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        if (key_Layout == null)
            key_Layout = GetComponent<Key_Layout>();
    }

    // Update is called once per frame
    void Update()
    {
        RunInteractivity();
    }

    /*This is how key presses are being registered in game.
    This is also how beatmaps can be recorded manually by the user.*/
    void RunInteractivity()
    {

        //Check if we can interact with keys
        if (GameManager.Instance.IsInteractable() != true) return;

        for (int keyNum = 0; keyNum < key_Layout.primaryBindedKeys.Count; keyNum++)
        {
            bool bindKeys = Input.GetKey(key_Layout.primaryBindedKeys[keyNum]);
            bool bindKeysPressed = Input.GetKeyDown(key_Layout.primaryBindedKeys[keyNum]);

            if (bindKeys)
            {
                /*If we happen to be recording, and we hit the second set of binded keys, the
                data will be written to a file.*/
                #region Write to RFTM File
                if (RoftPlayer.Record && bindKeysPressed)
                {
                    string data =
                        keyNum.ToString() + ","
                         + RoftPlayer.musicSource.timeSamples.ToString() + ","
                        + 0.ToString();
                }
                #endregion

                ToggleKeyActivity(keyNum, true);

                if (bindKeys)
                    keyPressInput = activeInput;
            }
            else
                ToggleKeyActivity(keyNum, false);
        }
    }

    //This will turn the keys on, signifying that the key is being pressed
    public bool ToggleKeyActivity(int _keyNum, bool _on)
    {
        //To not have to create another class that holds KeyControl, just cache it if not
        //in our dictionary, and reuse it.

        if (keyCache.ContainsKey(_keyNum) == false) 
            keyCache[_keyNum] = Key_Layout.keyObjects[_keyNum].GetComponent<KeyControls>();

        KeyControls key = keyCache[_keyNum];

        //int keyZoneValue = -1;
        //for (int keyNum = 0; keyNum < key_Layout.primaryBindedKeys.Count; keyNum++)
        //{
        //    if (Input.GetKey(key_Layout.primaryBindedKeys[keyNum]))
        //    {
        //        keyZoneValue = 0;
        //    }

        //    else if (Input.GetKey(key_Layout.secondaryBindedKeys[keyNum]))
        //    {
        //        keyZoneValue = 1;
        //    }

        //}

        Image graphics = key.GetGraphics();
        var alpha = _on ? (activeOpacity / activeOpacity) : (inactiveOpacity / activeOpacity);

        graphics.sprite = _on ? keyActive : keyInActive;
        graphics.color = new Color(graphics.color.r, graphics.color.g, graphics.color.b, alpha);

        if (debugText != null && _on)
            debugText.text = "Key " + key_Layout.primaryBindedKeys[_keyNum] + " pressed." + " Key Num: " + _keyNum;

        //graphics.color = defaultColor;

        return _on;
    }

    public int GetKeyPressInputValue() => keyPressInput;
}