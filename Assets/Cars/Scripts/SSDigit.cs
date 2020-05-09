using System.Collections.Generic;
using UnityEngine;

public class SSDigit : MonoBehaviour
{
    public GameObject segmentPrefab;
    public string segmentPrefabsTag = "";
    public float scale = 1.735f;
    public float carSpawnDistance = 15;
    public float carMovementSpeed = 4;
    public int digit = 0;

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

    private Vector2[] segmentPositions = new Vector2[7] {
        new Vector2(0, 2),    // A - top
        new Vector2(1, 1),    // B - right top
        new Vector2(1, -1),   // C - right bottom
        new Vector2(0, -2),   // D - bottom
        new Vector2(-1, -1),  // E - left bottom
        new Vector2(-1, 1),   // F - left top
        new Vector2(0, 0)     // G - center
    };

    private float[] segmentRotations = new float[7]
    {
        90,
        0,
        0,
        270,
        180,
        180,
        270
    };

    private GameObject[] segmentGameObjects;

    private int activeDigit = 10;  // no digit

    private void OnEnable()
    {
        segmentGameObjects = new GameObject[7];
    }

    void Start()
    {
        //OnDigitChanged();  // I think this isn't needed...
    }

    void Update()
    {
        if (digit != activeDigit) OnDigitChanged();
    }

    void OnDigitChanged()
    {
        bool[] activeSegments = STATES[activeDigit];
        bool[] desiredSegments = STATES[digit];
        activeDigit = digit;
        // Clear unneeded segments
        for (int i = 0; i < activeSegments.Length; i++)
        {
            if (activeSegments[i] && !desiredSegments[i]) SegmentOut(i);
        }
        // Activate needed segments
        for (int i = 0; i < desiredSegments.Length; i++)
        {
            if (!activeSegments[i] && desiredSegments[i]) SegmentIn(i);
        }
    }

    void SegmentIn(int segmentIndex)
    {
        // Position and rotation
        Vector2 segVector = segmentPositions[segmentIndex];
        float xDelta = segVector.x * scale;
        float zDelta = segVector.y * scale;  // `z = y`, this is not a mistake
        float yRotation = segmentRotations[segmentIndex];
        // Instantiate car object
        GameObject segmentObject = segmentGameObjects[segmentIndex] = Instantiate(
            GetPrefab(), gameObject.transform, false);
        segmentObject.transform.localPosition = new Vector3(xDelta, 0, zDelta);
        segmentObject.transform.Rotate(new Vector3(0, yRotation, 0));
        // Add car script to car object
        SSDigitCar car = segmentObject.AddComponent<SSDigitCar>();
        car.digit = this;
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

    void SegmentOut(int segmentIndex)
    {
        segmentGameObjects[segmentIndex].GetComponent<SSDigitCar>().Depart();
    }
}
