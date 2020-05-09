using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSDigitCar : MonoBehaviour
{
    public SSDigit digit;
    public Vector3 targetPosition;
    public float spawnDistanceRandomFactor = 1;
    public float movementSpeedRandomFactorMin = 0.6f;
    public float movementSpeedRandomFactorMax = 1.4f;
    private float movementSpeed;
    private float spawnDistance;
    private bool arrived;
    private bool departing;

    private void OnEnable()
    {
        arrived = false;
        departing = false;
    }

    void Start()
    {
        movementSpeed = digit.carMovementSpeed * UnityEngine.Random.Range(
            movementSpeedRandomFactorMin, movementSpeedRandomFactorMax);
        spawnDistance = digit.carSpawnDistance * UnityEngine.Random.Range(
            1.0f, spawnDistanceRandomFactor);
        targetPosition = transform.position;
        transform.Translate(0, 0, -spawnDistance);
    }

    void Update()
    {
        if (arrived) return;
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > 0.001f)  // In some cases it never reaches absolute zero
        {
            float step = Time.deltaTime * movementSpeed;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }
        else
        {
            arrived = true;
            if (departing) Destroy(gameObject);
        }
    }

    internal void Depart()
    {
        targetPosition = transform.TransformPoint(new Vector3(0, 0, spawnDistance));
        arrived = false;
        departing = true;
    }
}
