using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AutomotiveClock : MonoBehaviour
{
    public GameObject segmentPrefab;
    public string segmentPrefabsTag = "";
    public float scale = 1.735f;
    public float carSpawnDistance = 30;
    public float carMovementSpeed = 4;

    private int hour = 0;
    private int minute = 0;

    private void OnEnable()
    {
        // Initialize children
        foreach (SSDigit digit in gameObject.GetComponentsInChildren<SSDigit>())
        {
            digit.segmentPrefab = segmentPrefab;
            digit.segmentPrefabsTag = segmentPrefabsTag;
            digit.scale = scale;
            digit.carSpawnDistance = carSpawnDistance;
            digit.carMovementSpeed = carMovementSpeed;
        }
    }

    void Start()
    {
        // Set initial time
        TimeChanged(DateTime.Now);
    }

    void Update()
    {
        if (DEBUG_MODE)
        {
            DebugModeUpdate();
            return;
        }
        DateTime now = DateTime.Now;
        if (now.Hour != hour || now.Minute != minute)
        {
            TimeChanged(now);
        }
    }

    public bool DEBUG_MODE = false;
    public float DEBUG_UPDATE_INTERVAL = 5;
    private float timeSinceLastUpdate = 0;
    private DateTime debugTime = DateTime.Now;

    void DebugModeUpdate()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= DEBUG_UPDATE_INTERVAL)
        {
            timeSinceLastUpdate -= DEBUG_UPDATE_INTERVAL;
            debugTime = debugTime.AddMinutes(1);
            TimeChanged(debugTime);
        }
    }

    void TimeChanged(DateTime now)
    {
        hour = now.Hour;
        minute = now.Minute;
        SetDigit(0, hour / 10);
        SetDigit(1, hour % 10);
        SetDigit(2, minute / 10);
        SetDigit(3, minute % 10);
    }

    void SetDigit(int index, int digit)
    {
        gameObject.transform.GetChild(index).GetComponent<SSDigit>().digit = digit;
    }
}
