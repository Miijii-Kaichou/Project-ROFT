using System.IO;
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
    public float[] keyXOffset = new float[]
    {
        -9.75f,
        -9.25f,
        -9.25f,
        -13f,
        -13f,
        -17.5f,
        -16.5f
    };

    public float[] keyYOffset = new float[]
    {
        3f,
        5f,
        6.75f,
        4.75f,
        6.75f,
        4.75f,
        6.75f
    };

    public float[] keyLayoutScale = new float[]
    {
        140f,
        108f,
        90f,
        106f,
        85f,
        85f,
        90f
    };


    public float[] keyHorizontalSpread = new float[]
    {
        3f,
        2.5f,
        3f,
        2.6f,
        2.5f,
        2.5f,
        2.0f
    };

    public float[] keyVerticalSpread = new float[]
    {
        3f,
        2.75f,
        2.4f,
        2.6f,
        2.5f,
        2.5f,
        2.0f
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
        if (!File.Exists(Application.persistentDataPath + "/keyConfig.json"))
            CreateJSON();
    }

    private void CreateJSON()
    {
        //Simply make this into a JSON
        json = JsonUtility.ToJson(Instance, true);
        File.WriteAllText(Application.persistentDataPath + "/keyConfig.json", json);
    }

    public float[] GetKeyLayoutScale => keyLayoutScale;
    public float[] GetHorizontalSpread => keyHorizontalSpread;
    public float[] GetVerticalSpread => keyVerticalSpread;
    public float[] GetXOffset => keyXOffset;
    public float[] GetYOffset => keyYOffset;
    public string GetJSONString() => File.ReadAllText(Application.persistentDataPath + "/keyConfig.json");

}
