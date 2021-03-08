using UnityEngine;
using UnityEngine.AI;

public class PersonMouseController : MonoBehaviour
{
    public Camera cam;

    private NavMeshAgent agent;
    private NavMeshPath navMeshPath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        navMeshPath = new NavMeshPath();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
            Debug.Log("Has Path: " + agent.hasPath);

            if (CalculateNewPath(hit.transform.position))
            {
                Debug.Log("Path available");
            }
            else
            {
                Debug.Log("Path NOT available");
            }
            Debug.Log("Path distance: " + GetPathLength(hit.transform.position));
        }
        Debug.Log("Remaining Distance: " + agent.remainingDistance);
        // Debug.Log(agent.remainingDistance == Mathf.Infinity);
    }
    bool CalculateNewPath(Vector3 targetPos)
    {
        agent.CalculatePath(targetPos, navMeshPath);
        Debug.Log("New path calculated");
        if (navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        return true;
    }

    public float GetPathLength(Vector3 targetPos)
    {
        float pathLength = 0;
        if (CalculateNewPath(targetPos))  // Resulting path stored in navMeshPath
        {
            for (int i = 1; i < navMeshPath.corners.Length; i++)
            {
                pathLength += Vector3.Distance(navMeshPath.corners[i - 1], navMeshPath.corners[i]);
            }
        }
        return pathLength;
    }
}