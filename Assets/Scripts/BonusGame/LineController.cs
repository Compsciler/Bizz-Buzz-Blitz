using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public float rotationSpeed;
    public float rotationChance;
    public bool canRotateOnFirstGeneration;

    [SerializeField] internal float distance;
    private bool isRotating = false;
    private bool isClockwise;

    void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        GenerateRandomLine();
    }

    void Update()
    {
        if (isRotating)
        {
            if (isClockwise)
            {
                transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void GenerateRandomLine()
    {
        transform.rotation = Quaternion.identity;

        Vector3 rawPos0 = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
        Vector3 rawPos1 = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax));

        Vector3 midpoint = (rawPos0 + rawPos1) / 2;

        Vector3 pos0 = rawPos0 - midpoint;
        Vector3 pos1 = rawPos1 - midpoint;

        distance = (pos1 - pos0).magnitude;

        transform.position = midpoint;
        lineRenderer.SetPosition(0, pos0);
        lineRenderer.SetPosition(1, pos1);

        if (Random.Range(0f, 1f) < rotationChance && (canRotateOnFirstGeneration || GameManager2.instance.score != 0))
        {
            isRotating = true;
            if (Random.Range(0, 2) == 1)
            {
                isClockwise = true;
            }
            else
            {
                isClockwise = false;
            }
        }
        else
        {
            isRotating = false;
        }
    }
}