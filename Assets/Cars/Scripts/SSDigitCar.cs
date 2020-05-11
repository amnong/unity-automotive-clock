using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class SSDigitCar : MonoBehaviour
{
    public SSDigit digit;
    public float spawnDistanceRandomFactor = 1;
    public float movementSpeedRandomFactorMin = 0.6f;
    public float movementSpeedRandomFactorMax = 1.4f;
    public float spawnXOffset = 0f;
    public float spawnXOffsetRandom = 5f;
    public float parkingDistance = 2f;
    public int segmentIndex;

    private float movementSpeed;
    private float spawnDistance;
    private NavMeshAgent navAgent;
    private Queue<Tuple<Vector3, State?>> destinations;
    private Tuple<Vector3, State?> targetDestination;

    internal enum State { ARRIVING, PARKING, PARKED, DEPARTING }
    internal State state;

    private LineRenderer line;
    private bool pathDebug = false;

    private void Log(string message, params object[] formatArgs)
    {
        //if (segmentIndex != 0) return;
        Debug.Log(string.Format("SEG-{0}: ", segmentIndex) + string.Format(message, formatArgs));
    }

    private void OnEnable()
    {
        if (pathDebug) line = gameObject.AddComponent<LineRenderer>();
        navAgent = gameObject.AddComponent<NavMeshAgent>();
        destinations = new Queue<Tuple<Vector3, State?>>();
        state = State.ARRIVING;
    }

    void Start()
    {
        movementSpeed = digit.carMovementSpeed * UnityEngine.Random.Range(
            movementSpeedRandomFactorMin, movementSpeedRandomFactorMax);
        spawnDistance = digit.carSpawnDistance * UnityEngine.Random.Range(
            1.0f, spawnDistanceRandomFactor);
        navAgent.speed = movementSpeed;
        navAgent.radius = 0.25f;
        navAgent.avoidancePriority += UnityEngine.Random.Range(-5, 5);
        Vector3 targetPosition = transform.position;
        Vector3 parkingPosition = transform.TransformPoint(new Vector3(0, 0, -parkingDistance));
        spawnXOffset += UnityEngine.Random.Range(-spawnXOffsetRandom, spawnXOffsetRandom);
        transform.Translate(spawnXOffset, 0, -spawnDistance);
        //Log("Spawn: {0}", transform.position);
        AddDestination(parkingPosition, State.PARKING);
        AddDestination(targetPosition, State.PARKED);
    }

    void Update()
    {
        if (!(navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)) return;

        if (AdvanceDestination())
        {
            if (pathDebug) StartCoroutine(DrawPath());
        }
    }

    void AddDestination(Vector3 position, State? targetState)
    {
        //Log("Add destination: {0}", position);
        destinations.Enqueue(new Tuple<Vector3, State?>(position, targetState));
    }

    bool AdvanceDestination()
    {
        bool reachedCurrentTarget = ReachedCurrentTarget();
        if (reachedCurrentTarget && targetDestination != null && targetDestination.Item2 != null)
        {
            // Previous target state is now the current state
            state = (State)targetDestination.Item2;
            targetDestination = null;
            if (pathDebug) line.positionCount = 0;
        }
        if (targetDestination == null && destinations.Count != 0) {
            Tuple<Vector3, State?> next = destinations.Peek();
            if (navAgent.SetDestination(next.Item1))
            {
                // Remove item from queue because it's already dequeued
                destinations.Dequeue();
                targetDestination = next;
                return true;
            }
            //Log("Failed to set destination");
            NavMeshPath path = new NavMeshPath();
            navAgent.CalculatePath(next.Item1, path);
            Log("Failed to set destination: status={0}, corners={1}", path.status, path.corners);
            return false;
        }
        return false;
    }

    bool ReachedCurrentTarget()
    {
        return navAgent.remainingDistance != Mathf.Infinity &&
               navAgent.pathStatus == NavMeshPathStatus.PathComplete &&
               //navAgent.remainingDistance <= 0.02f;
               navAgent.remainingDistance == 0;
    }

    IEnumerator DrawPath()
    {
        if (navAgent.path.corners.Length >= 2)
        {
            yield return new WaitForEndOfFrame();
            line.positionCount = navAgent.path.corners.Length;
            line.SetPosition(0, transform.position);
            for (var i = 1; i < navAgent.path.corners.Length; i++)
            {
                line.SetPosition(i, navAgent.path.corners[i]);
            }
        }
    }

    internal void Depart()
    {
        StartCoroutine(SetDeparting());
        state = State.DEPARTING;
        //navAgent.Move(new Vector3(0, 0, spawnDistance));
        AddDestination(transform.TransformPoint(new Vector3(0, 0, spawnDistance)) + new Vector3(-spawnXOffset, 0, 0), null);
    }

    private IEnumerator SetDeparting()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
