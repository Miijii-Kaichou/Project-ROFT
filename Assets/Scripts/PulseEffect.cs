﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public Transform m_transform;
    Vector3 scale;
    Vector3 normalScale;
    bool waiting = false;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = GetComponent<Transform>();
        scale = m_transform.localScale;
        normalScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        if (waiting) Wait(0.05f);
    }

    //When I hit a note, this function will be called
    public void DoPulseReaction(float _size = 0.2f)
    {
        Vector3 pulseSize = new Vector3(_size, _size, _size);
        scale += pulseSize;
        m_transform.localScale = scale;
        waiting = true;
    }

    public void Wait(float _duration)
    {
        
        time += Time.deltaTime;
        scale = new Vector3(scale.x - 0.05f, scale.y - 0.05f, 1f);
        m_transform.localScale = scale;
        if (time >= _duration)
        {
            scale = normalScale;
            m_transform.localScale = scale;
            waiting = false;
            time = 0;
        }
    }
}
