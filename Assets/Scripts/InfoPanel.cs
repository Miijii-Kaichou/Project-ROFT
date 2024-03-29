﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoPanel : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    public static bool IsVisible = false;


    private void Start()
    {
        StartCoroutine(SystemCycle());
    }

    IEnumerator SystemCycle()
    {
        while (true)
        {
            animator.SetBool("isVisible", IsVisible);
            if (SceneManager.GetActiveScene().name != SceneManager.GetSceneByBuildIndex(0).name && 
                SceneManager.GetActiveScene().name != SceneManager.GetSceneByBuildIndex(3).name)
                IsVisible = true;
            else
                IsVisible = false;
            yield return null;
        }
    }
}
