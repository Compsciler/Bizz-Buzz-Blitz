using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HospitalBarrier : MonoBehaviour
{
    private Vector3 hospitalTilePos = new Vector3(0, 1.67f, 0);
    public float warpMaxDistance = 6.17f;

    private float unoccupyHospitalTime = 2f;

    void Start()
    {
        // transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
    }

    // Agent has isTrigger enabled
    /*
    void OnCollisionEnter(Collision collision)
    {
        NavMeshAgent agent = collision.gameObject.GetComponent<NavMeshAgent>();
        Debug.Log(collision.gameObject);
        if (agent == null)
        {
            return;
        }
        Debug.Log(agent.agentTypeID);
        Debug.Log(HospitalTile.isOccupied);
        
        if (agent.agentTypeID == recentlyHealedAgentID && !HospitalTile.isOccupied)  // Change for different difficulty settings
        {
            Physics.IgnoreCollision(GetComponent<BoxCollider>(), collision.collider, true);
            Debug.Log("IGNORED");
            HospitalTile.isOccupied = true;
            // agent.SetDestination(new Vector3(0, 1.67f, 0));
        }
    }
    */
    
    void OnTriggerStay(Collider other)  // Changed from OnTriggerEnter  // Will this trigger once for multiple GameObjects?
    {
        NavMeshAgent agent = other.gameObject.GetComponent<NavMeshAgent>();
        // Debug.Log(other.gameObject);
        if (agent == null)
        {
            return;
        }
        // Debug.Log(agent.agentTypeID);
        // Debug.Log(HospitalTile.isOccupied);
        if (GameManager.instance.canHealthyHeal)
        {
            if ((agent.agentTypeID == Constants.healthyUnboundAgentID || agent.agentTypeID == Constants.infectedAgentID) && !HospitalTile.isOccupied)
            {
                if (agent.agentTypeID == Constants.healthyUnboundAgentID)
                {
                    agent.agentTypeID = Constants.priorityHealthyAgentID;
                }
                else
                {
                    agent.agentTypeID = Constants.priorityInfectedAgentID;
                }
                agent.SetDestination(hospitalTilePos);
                Debug.Log("OCCUPIED " + other.gameObject.name);
                HospitalTile.isOccupied = true;
            }
            else if (agent.agentTypeID == Constants.healthyUnboundAgentID && HospitalTile.isOccupied && !agent.gameObject.GetComponent<PersonController>().hasRecentlyHealed)
            {
                agent.Warp(GetWarpPos(agent.transform.position));
            }
        }
        else
        {
            if (agent.agentTypeID == Constants.infectedAgentID && !HospitalTile.isOccupied)  // agent.agentTypeID == recentlyHealedAgentID && !HospitalTile.isOccupied  // Change for different difficulty settings
            {
                agent.agentTypeID = Constants.priorityInfectedAgentID;
                agent.SetDestination(hospitalTilePos);
                // Physics.IgnoreCollision(GetComponent<BoxCollider>(), other, true);  // Need a NavMeshModifier Ignore
                Debug.Log("OCCUPIED " + other.gameObject.name);
                HospitalTile.isOccupied = true;
                // HealProgressBar.isNewlyOccupied = true;  // Put in PersonController.cs
                // agent.SetDestination(new Vector3(0, 1.67f, 0));
            }
        }
        UnoccupyHospital(agent.gameObject);
    }

    Vector3 GetWarpPos(Vector3 oldPos)
    {
        if (Mathf.Abs(oldPos.x) > Mathf.Abs(oldPos.z))
        {
            if (oldPos.x > 0)
            {
                return new Vector3(warpMaxDistance, oldPos.y, oldPos.z);
            }
            return new Vector3(-warpMaxDistance, oldPos.y, oldPos.z);
        }
        if (oldPos.z > 0)
        {
            return new Vector3(oldPos.x, oldPos.y, warpMaxDistance);
        }
        return new Vector3(oldPos.x, oldPos.y, -warpMaxDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        NavMeshAgent agent = other.gameObject.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            return;
        }
        // Debug.Log("COLLISION BY " + other.gameObject.name);
    }

    IEnumerator<float> UnoccupyHospital(GameObject priorityPerson)  // Should rarely be called
    {
        yield return Timing.WaitForSeconds(unoccupyHospitalTime);
        if (!priorityPerson.GetComponent<PersonController>().isHealing)
        {
            HospitalTile.isOccupied = false;
            Debug.Log("UNOCCUPIED 3 BY " + priorityPerson.name);
        }
    }
}