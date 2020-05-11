using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SSDigit : MonoBehaviour
{
    public GameObject segmentPrefab;
    public string segmentPrefabsTag = "";
    public float scale = 1.735f;
    public float carSpawnDistance = 15;
    public float carMovementSpeed = 4;
    public int digit = 0;
    public bool mirrorOrientation;

    private int activeDigit = 10;  // no digit

    private Dictionary<int, bool[]> STATES = new Dictionary<int, bool[]>()
    {
        {0, new bool[] { true, true, true, true, true, true, false }},
        {1, new bool[] { false, true, true, false, false, false, false}},
        {2, new bool[] { true, true, false, true, true, false, true }},
        {3, new bool[] { true, true, true, true, false, false, true }},
        {4, new bool[] { false, true, true, false, false, true, true }},
        {5, new bool[] { true, false, true, true, false, true, true }},
        {6, new bool[] { true, false, true, true, true, true, true }},
        {7, new bool[] { true, true, true, false, false, false, false}},
        {8, new bool[] { true, true, true, true, true, true, true }},
        {9, new bool[] { true, true, true, true, false, true, true }},
        {10, new bool[] { false, false, false, false, false, false, false}}  // no digit
    };

    private class Segment
    {
        public float xPos, zPos;
        public int rotation;

        public Segment(float xPos, float zPos, int rotation)
        {
            this.xPos = xPos;
            this.zPos = zPos;
            this.rotation = rotation;
        }
    }

    private Segment[] segmentInfos = new Segment[]
    {
        new Segment(xPos:  0,  zPos:  2,  rotation:  90),
        new Segment(xPos:  1,  zPos:  1,  rotation: 180),
        new Segment(xPos:  1,  zPos: -1,  rotation:   0),
        new Segment(xPos:  0,  zPos: -2,  rotation:  90),
        new Segment(xPos: -1,  zPos: -1,  rotation:   0),
        new Segment(xPos: -1,  zPos:  1,  rotation: 180),
        new Segment(xPos:  0,  zPos:  0,  rotation:  90)
    };

    private GameObject[] segmentGameObjects;

    void OnEnable()
    {
        segmentGameObjects = new GameObject[7];
    }

    void Update()
    {
        if (digit != activeDigit) OnDigitChanged();
    }

    void OnDigitChanged()
    {
        bool[] activeSegments = STATES[activeDigit];
        bool[] desiredSegments = STATES[digit];
        float delay;
        const float interval = 0.5f;
        activeDigit = digit;
        // Clear unneeded segments
        delay = 0f;
        for (int i = 0; i < activeSegments.Length; i++)
        {
            if (activeSegments[i] && !desiredSegments[i])
            {
                GameObject segmentObject = segmentGameObjects[i];
                segmentGameObjects[i] = null;
                StartCoroutine(SegmentOut(segmentObject, delay));
                delay += interval;
            }
        }
        // Activate needed segments
        delay = 0f;
        for (int i = 0; i < desiredSegments.Length; i++)
        {
            if (!activeSegments[i] && desiredSegments[i])
            {
                StartCoroutine(SegmentIn(i, delay));
                delay += interval;
            }
        }
    }

    IEnumerator<WaitForSeconds> SegmentIn(int segmentIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Position and rotation
        Segment segment = segmentInfos[segmentIndex];
        float yRotation = segment.rotation;
        if (mirrorOrientation && (segmentIndex == 0 || segmentIndex == 3))
        {
            yRotation = (yRotation + 180) % 360;
        }
        // Instantiate car object
        GameObject segmentObject = segmentGameObjects[segmentIndex] = Instantiate(
            GetPrefab(), gameObject.transform, false);
        segmentObject.transform.localPosition = new Vector3(segment.xPos * scale, 0, segment.zPos * scale);
        segmentObject.transform.Rotate(new Vector3(0, yRotation, 0));
        // Add car script to car object
        SSDigitCar car = segmentObject.AddComponent<SSDigitCar>();
        car.digit = this;
        car.segmentIndex = segmentIndex;
        if (segmentIndex == 0) car.spawnXOffset = carSpawnDistance / 2f;
        if (segmentIndex == 3) car.spawnXOffset = -carSpawnDistance / 2f;
    }

    IEnumerator<WaitForSeconds> SegmentOut(GameObject segmentObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        segmentObject.GetComponent<SSDigitCar>().Depart();
    }

    private GameObject GetPrefab()
    {
        return segmentPrefab;  // This just doesn't work, so it's disbaled for now

        //if (segmentPrefabsTag == "") return segmentPrefab;

        //var prefabs = new List<GameObject>();
        //foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(segmentPrefabsTag))
        //{
        //    Debug.Log(obj);
        //    if (obj is GameObject gobj)
        //    {
        //        prefabs.Add(gobj);
        //    }
        //}

        //if (prefabs.Count == 0)
        //{
        //    Debug.Log(String.Format("No prefabs found at \"{0}\"", segmentPrefabsTag));
        //    return segmentPrefab;
        //}

        ////var prefabsQuery = Resources.FindObjectsOfTypeAll(typeof(GameObject))
        ////    .Cast<GameObject>()
        ////    .Where(g => g.tag == segmentPrefabsTag);
        ////var prefabs = new List<GameObject>(prefabsQuery);

        //int idx = UnityEngine.Random.Range(0, prefabs.Count);
        //return prefabs[idx];
    }
}
