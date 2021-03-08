using UnityEngine;

// Source: https://www.youtube.com/watch?v=_nRzoTzeyxU&t

[System.Serializable]
public class Dialogue
{
	// public string name;

	[TextArea(3, 10)]
	public string[] sentences;
	public float[] sentenceTimes;  // If negative, the absolute value is the times needed to click a tile. If over 1000, time scale is set to 0 and the value minus 1000 seconds are waited
	public GameObject[] tilesToClick;
}