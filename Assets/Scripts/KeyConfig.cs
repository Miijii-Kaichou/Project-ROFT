﻿using System.IO;
using UnityEngine;

[SerializeField]
public class KeyConfig
{
    public static KeyConfig Instance;

    //This class will be responsible of saving all configurations of how the keys should actually be layed out when
    //playing a song in the game.
    //This is only for entering a game. This gives the player to be able to adjust the scale, as well as the positioning of the
    //keyLayout. This will be saved as a JSON class, just for the sake of learning how to use it.
    //Of course we need a reference to the Key_Layout class.

    //We want to get all the values that keyLayout also has, because that will be important... I think...
    //No! Actually, what we want to do is take all of those values out from Key_Layout, and move it to this class.
    //Now that I think about it, having a separate class to handle the configuration of the layout of the keys
    //sounds a lot more to handle, and I really do approve that message! Yay!!!!!!
    private float[] keyXOffset { get; } = new float[5]
    {
        -11f,
        -9.25f,
        -8.5f,
        -8.5f,
        -17f
    };

    private float[] keyYOffset { get; } = new float[5]
    {
        -3f,
        2f,
        4.2f,
        6f,
        1.5f
    };

    private float[] defaultKeyScale { get; } = new float[5]
    {
        2.25f,
        1.75f,
        1.4f,
        1.4f,
        1f
    };

    private float[] keyHorizontalSpread { get; } = new float[5] {
        3.5f,
        2.5f,
        2f,
        2.25f,
        1.5f
    };

    private float[] keyVerticalSpread { get; } = new float[5]
    {
        3.5f,
        2.5f,
        2f,
        1.75f,
        1.5f
    };

    //String info
    private static string json;

    public KeyConfig()
    {
        Instance = this;
        SaveConfig();
    }

    private void SaveConfig()
    {
        /*SaveConfig method will be responsible of saving our scale, the position of different
         key totals, and the scale and distribution of it. That will then be save and send into the playerprefs,
         saving those preferences onto the user's computer.*/
        if (!File.Exists(Application.dataPath + "/keyConfig.json"))
            CreateJSON();
    }

    private static void CreateJSON()
    {
        //Simply make this into a JSON
        json = JsonUtility.ToJson(Instance);
        File.WriteAllText(Application.dataPath + "/keyConfig.json", json);
    }

    public float[] GetDefaultKeyScale => defaultKeyScale;
    public float[] GetHorizontalSpread => keyHorizontalSpread;
    public float[] GetVerticalSpread => keyVerticalSpread;
    public float[] GetXOffset => keyXOffset;
    public float[] GetYOffset => keyYOffset;
    public string GetJSONString() => File.ReadAllText(Application.dataPath + "/keyConfig.json");

}
