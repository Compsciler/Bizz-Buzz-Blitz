using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] bool isRandomDirection;

    void Start()
    {
        if (isRandomDirection)
        {
            if (Random.Range(0, 2) == 1)
            {
                rotationSpeed = -rotationSpeed;
            }
        }
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}