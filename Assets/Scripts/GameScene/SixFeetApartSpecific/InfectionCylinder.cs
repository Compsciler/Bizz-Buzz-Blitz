using MEC;
using System.Collections.Generic;
using UnityEngine;

public class InfectionCylinder : MonoBehaviour
{
    public float minRadius;
    public float maxRadius;
    public float scalingDuration;

    void Update()
    {
        if (!GameManager.instance.isGameActive)  // Script disables
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator<float> ExpandRadius()  // Runs once, unused
    {
        Vector3 startScale = new Vector3(minRadius, transform.localScale.y, minRadius);
        Vector3 endScale = new Vector3(maxRadius, transform.localScale.y, maxRadius);
        gameObject.SetActive(true);
        float timer = 0;
        while (timer < scalingDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, timer / scalingDuration);
            timer += Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }
        gameObject.SetActive(false);
        gameObject.transform.localScale = startScale;
    }

    public IEnumerator<float> SinusoidalRadius()  // Runs continuously
    {
        Vector3 maxScale = new Vector3(maxRadius, transform.localScale.y, maxRadius);
        float midRadius = (minRadius + maxRadius) / 2;
        Vector3 midScale = new Vector3(midRadius, transform.localScale.y, midRadius);
        gameObject.SetActive(true);
        while (true)
        {
            yield return Timing.WaitForOneFrame;
            float sineTime = Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI * 2);
            transform.localScale = Vector3.LerpUnclamped(midScale, maxScale, sineTime);
        }
    }

    void OnEnable()
    {
        if (GameManager.instance.areSymptomsDelayed)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}