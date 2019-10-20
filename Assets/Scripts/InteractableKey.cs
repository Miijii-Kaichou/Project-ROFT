﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableKey : MonoBehaviour
{
    SpriteRenderer sprite;
    Color originalColor;
    int keyNum;
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;
    }

    private void OnMouseOver()
    {
        sprite.color = Color.green;
        //Collect Data
        string data = keyNum + ", " + EditorToolClass.musicSource.timeSamples + ", " + 0.ToString();
        Key_Layout.Instance.dataText.text = data;
    }

    private void OnMouseDown()
    {
        #region Write to RFTM File
        if (EditorToolClass.Instance.record)
        {
            string data =
                keyNum.ToString() + ","
                 + EditorToolClass.musicSource.timeSamples.ToString() + ","
                + 0.ToString();

            EditorToolClass.Instance.WriteToRFTM(EditorToolClass.musicSource.clip.name, Application.streamingAssetsPath + "/", data);
        }
        #endregion
    }

    private void OnMouseExit()
    {
        sprite.color = originalColor;
    }

    public void SetKeyNum(int _value)
    {
        keyNum = _value;
    }
}