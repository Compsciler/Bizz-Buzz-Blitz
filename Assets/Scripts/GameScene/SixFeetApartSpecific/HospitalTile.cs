using System.Collections.Generic;
using UnityEngine;
using MEC;

public class HospitalTile : MonoBehaviour
{
    public float waitTime;
    internal static bool isOccupied = false;
    [SerializeField] bool isOccupiedSerialized = false;
    public GameObject hospitalBarriers;
    // public GameObject[] barriers;

    void Update()
    {
        isOccupiedSerialized = isOccupied;
    }

    public IEnumerator<float> HospitalQueue()
    {
        // hospitalBarriers.SetActive(true);
        // isOccupied = true;  // Done in HospitalBarrier.cs
        // Debug.Log("OCCUPIED");
        yield return Timing.WaitForSeconds(waitTime);
        // hospitalBarriers.SetActive(false);
        // Debug.Log("UNOCCUPIED");
        // isOccupied = false;  // Done in PersonController.cs
    }

    /*
    void SetBarriersActive(bool value)
    {
        foreach (GameObject barrier in barriers)
        {
            barrier.SetActive(value);
        }
    }
    */
}