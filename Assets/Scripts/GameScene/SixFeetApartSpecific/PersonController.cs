using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MEC;

public class PersonController : MonoBehaviour
{
    public Camera cam;
    private Camera mainCamera;

    private NavMeshAgent agent;
    private NavMeshPath navMeshPath;
    public float minDistanceRelocation;
    public float xPosRange = 45f;
    public float zPosRange = 25f;
    public float yPos = 1.67f;

    private float tileSize = 10f;
    private Vector3 hospitalTilePos = new Vector3(0, 1.67f, 0);
    private float navMeshSurfaceAgentRadius = 1f;

    // private float hospitalEnterPosSideMax = 4.85f;

    // I should remember to use enums in my next project...
    public bool startsInfected;
    [SerializeField] bool isInfected;
    private bool isRecentlyInfected;
    internal bool isHealing = false;  // Should be needed only when canHealthyHeal is true
    private bool isInfectedWithoutSymptoms = false;

    public float infectionDeathDuration = 40f;
    public float healTime;  // Value also exists in HealProgressBar.cs

    internal MeshRenderer meshRenderer;

    [Header("Materials")]
    public Material healthyMaterial;
    public Material[] infectedMaterials;
    public Material deadMaterial;
    public Material unknownMaterial;

    internal InfectionCylinder infectionCylinderScript;
    private HospitalTile hospitalTileScript;

    internal float hospitalTileDistance;

    public float infectedSetPathTime = 1f;
    private float infectedSetPathTimer = -1f;

    private bool hasStartedHealing = false;
    internal bool hasRecentlyHealed = false;  // Needed only when canHealthyHeal is true

    private float healthyAcceleration = 15f;
    private float infectedAcceleration = 400f;
    // private CoroutineHandle handle;

    public int hospitalPathMode = 2;
    Vector3[][] pathModeDestinations = new Vector3[3][];
    private NavMeshAgent tempAgent;
    private float tempAgentYPos = -4.41f;

    private bool isNearHospital = false;  // Unused
    private float farInfectedHospitalBorderWidth = 1.5f;  // Try 2f if range is too close
    private float wallYPos = 1f;

    // private bool isGameActivePreviousFrame = true;
    private bool wasAgentStopped = false;

    public float priorityHospitalReachTime = 0.2f;
    private float priorityHospitalReachTimer;

    // public float speedThreshold = 1f;
    private int defaultAvoidancePriority = 50;
    private int highestAvoidancePriority = 25;
    private int lowestAvoidancePriority = 75;

    public AudioClip healSound;
    public AudioClip newInfectionSound;
    public float healSoundVolume;
    public float newInfectionSoundVolume;

    public ParticleSystem infectionParticles;
    public ParticleSystem healParticles;
    private float infectionParticleStartTime;
    public float infectionParticleStartTimeRatio = 0.625f;

    private Color defaultBackgroundColor;
    private Color dangerBackgroundColor = new Color32(150, 0, 24, 0);  // Carmine
    private float backgroundFlashTime = 0.05f;

    internal static int infectedPeopleTotal = 0;  // Person on Hospital tile healing does not count as infected
    private int infectedPeopleBackgroundThreshold = 16;

    private float changeBackToHealthyUnboundAgentTime = 5f;
    private bool mustStayHealthyBoundAgent = false;

    private bool arePathDestinationsSet = false;
    public Vector3[] pathDestinations1;
    public Vector3[] pathDestinations2;
    private Vector3[] pathDestinations;
    private int pathDestinationIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        navMeshPath = new NavMeshPath();
        meshRenderer = GetComponent<MeshRenderer>();

        infectionDeathDuration = GameManager.instance.infectionDeathDuration;
        isInfected = startsInfected;
        isRecentlyInfected = startsInfected;

        infectionCylinderScript = GetComponentInChildren<InfectionCylinder>(true);
        hospitalTileScript = FindObjectOfType<HospitalTile>();

        GeneratePathModeDestinations(true);
        tempAgent = GameObject.Find("Temp Pathing Agent").GetComponent<NavMeshAgent>();

        priorityHospitalReachTimer = priorityHospitalReachTime;

        defaultBackgroundColor = mainCamera.backgroundColor;

        if (GameManager.instance.areSymptomsDelayed)
        {
            meshRenderer.material = unknownMaterial;
            if (isInfected)
            {
                if (GameManager.instance.canHealthyHeal)
                {
                    agent.agentTypeID = Constants.healthyUnboundAgentID;
                }
                else
                {
                    agent.agentTypeID = Constants.healthyAgentID;
                }
            }
        }

        infectionParticleStartTime = infectionDeathDuration * infectionParticleStartTimeRatio;
        arePathDestinationsSet = GameManager.instance.isTutorial;
        if (arePathDestinationsSet)
        {
            switch (gameObject.name.Replace("Person ", ""))
            {
                case "1":
                    pathDestinations = pathDestinations1;
                    break;
                case "2":
                    pathDestinations = pathDestinations2;
                    break;
            }
        }
    }

    void Update()
    {
        isInfectedWithoutSymptoms = (isInfected && (agent.agentTypeID == Constants.healthyAgentID || agent.agentTypeID == Constants.healthyUnboundAgentID || agent.agentTypeID == Constants.priorityHealthyAgentID));
        if (!GameManager.instance.isGameActive)
        {
            if (GameManager.instance.isGameActivePreviousFrame)
            {
                wasAgentStopped = true;  // For later use with revives
            }
            agent.isStopped = true;  // Occasionally Person 1 and Person 2 (and maybe others as well) are not stopped with GameManager.instance.isGameActivePreviousFrame added in the largest if-block
            // isGameActivePreviousFrame = false;
            // Debug.Log(gameObject.name + " STOPPED");
        }
        else if (wasAgentStopped)
        {
            agent.isStopped = false;
            wasAgentStopped = false;
        }
        /*
        if (GameManager.instance.isGameActive && !isGameActivePreviousFrame)
        {
            agent.isStopped = false;
            isGameActivePreviousFrame = true;
        }
        */
        if (arePathDestinationsSet)
        {
            if (agent.remainingDistance <= minDistanceRelocation && pathDestinationIndex < pathDestinations.Length)
            {
                // Debug.Log("Path destination: " + pathDestinationIndex);
                agent.SetDestination(pathDestinations[pathDestinationIndex]);
                pathDestinationIndex++;
            }
            else if (pathDestinationIndex >= pathDestinations.Length)
            {
                arePathDestinationsSet = false;
            }
        }
        if (agent.remainingDistance <= minDistanceRelocation && (!isInfected || isInfectedWithoutSymptoms) && !isHealing && !arePathDestinationsSet)  // Returns 0 when reached destination or when blocked, as close as it can get
        {
            if (GameManager.instance.canHealthyHeal)  // Bad solution, but it should work
            {
                if (HealProgressBar.isNewlyOccupied || mustStayHealthyBoundAgent)
                {
                    agent.agentTypeID = Constants.healthyAgentID;
                }
                else
                {
                    agent.agentTypeID = Constants.healthyUnboundAgentID;
                }
            }
            float xPos = Random.Range(-xPosRange, xPosRange);
            float zPos = Random.Range(-zPosRange, zPosRange);
            Vector3 newPos = new Vector3(xPos, yPos, zPos);
            if (CalculateNewPath(newPos, false))
            {
                agent.SetDestination(newPos);
                // Debug.Log("Path available: " + newPos);
            }
            else
            {
                // Debug.Log("Path NOT available");
            }

            /*
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (CalculateNewPath(hit.transform.position))
                {
                    agent.SetDestination(hit.point);
                    Debug.Log("Path available: " + hit.transform.position);
                }
                else
                {
                    Debug.Log("Path NOT available");
                }
            }
            */

            /*
            NavMeshHit hit2;
            if (NavMesh.SamplePosition(hit.transform.position, out hit2, 1f, NavMesh.AllAreas))
            {
                UnityEngine.Vector3 result = hit2.position;
                Debug.Log(result);
            }
            else
            {
                Debug.Log(false);
            }
            */
            // OPTIONS: CalculatePath every frame, every second, or every wall rotated
            // OTHER OPTIONS: Move Random Direction until hit wall and repeat
            // https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html
            // NavMeshPath path = new NavMeshPath();
            // bool hasPath = NavMesh.CalculatePath(transform.position, hit.transform.position, NavMesh.AllAreas, path);
            // How to have healthy people avoid going on top of hospital tile in Normal difficulty: https://www.youtube.com/watch?v=CHV1ymlw-P8
        }
        if (IsOnHospitalTile(0) && !hasStartedHealing)
        {
            Vector3 exitPos = transform.position;
            HealProgressBar.isNewlyOccupied = true;
            HealProgressBar.isOccupiedByInfectedPerson = isInfected;
            Heal();
            Debug.Log("HEALING " + gameObject.name);
            // Timing.RunCoroutine(hospitalTileScript.HospitalQueue());  // Moved from outside if-block in Heal();
            Timing.RunCoroutine(CheckIfOffHospitalTile());
            isHealing = true;
            isRecentlyInfected = false;  // Just in case
            Timing.RunCoroutine(HealingProcess());  // Only show bar when previously infected? INSIDE?
            hasStartedHealing = true;
            if (GameManager.instance.canHealthyHeal)
            {
                hasRecentlyHealed = true;
            }
            // Debug.Log("AFTER COROUTINE");
            /*
            exitPos.x = -exitPos.x;
            exitPos.z = -exitPos.z;
            agent.Warp(exitPos);
            */
        }
        else if (!IsOnHospitalTile(0))
        {
            hasStartedHealing = false;
        }
        if (isInfected && agent.agentTypeID != Constants.recentlyHealedAgentID && agent.agentTypeID != Constants.priorityInfectedAgentID)  // Moved priorityInfectedAgentID here
        {
            if (!isInfectedWithoutSymptoms && !arePathDestinationsSet)
            {
                bool pathPending = true;
                // bool notPriorityNorRecentlyHealed = agent.agentTypeID != priorityInfectedAgentID && agent.agentTypeID != recentlyHealedAgentID;  // Put recentlyHealed check in main if-condition
                if (agent.remainingDistance != Mathf.Infinity && IsOnHospitalTile(farInfectedHospitalBorderWidth))  // When path is a straight line to hospital and close enough
                {
                    if (agent.agentTypeID == Constants.farInfectedAgentID)
                    {
                        agent.agentTypeID = Constants.infectedAgentID;
                        agent.SetDestination(hospitalTilePos);
                    }
                    else
                    {
                        agent.agentTypeID = Constants.infectedAgentID;  // Fixed v0.7 Alpha 1 Bug #1?
                    }
                }
                if (infectedSetPathTimer < 0)  // Changed NavMeshAgent acceleration to counteract path recalculation navigation pauses
                {
                    if (agent.agentTypeID == Constants.farInfectedAgentID)
                    {
                        agent.SetDestination(hospitalTilePos);
                    }
                    else
                    {
                        Vector3 shiftedOrigin = new Vector3(transform.position.x, wallYPos, transform.position.z);
                        Vector3 hospitalDirection = hospitalTilePos - transform.position;
                        float hospitalDistance = Vector3.Distance(transform.position, hospitalTilePos);
                        int layerMask = 1 << 9;  // Ray cast only against colliders in Layer 9: Walls
                        bool hasLineOfSightWithHospital = !Physics.Raycast(shiftedOrigin, hospitalDirection, hospitalDistance, layerMask);
                        Debug.DrawRay(shiftedOrigin, hospitalDirection, UnityEngine.Color.red, 2f);
                        // Debug.Log("Line of sight with Hospital: " + hasLineOfSightWithHospital);
                        if (CalculateNewPath(hospitalTilePos, true) && !hasLineOfSightWithHospital)  // This if-block seems to never be run, but should when path to hospital found and agent type is infectedSpheroid
                        {
                            agent.agentTypeID = Constants.farInfectedAgentID;
                            // agent.SetDestination(hospitalTilePos);  // Enable?
                        }
                        else
                        {
                            float minPathLength = float.MaxValue;
                            Vector3 minPathLocation = hospitalTilePos;  // Defaults here if no complete paths
                            foreach (Vector3 location in pathModeDestinations[hospitalPathMode])
                            {
                                float pathLength = GetPathLength(location, true);
                                // Debug.Log(location + ": " + pathLength);
                                if (pathLength < minPathLength)
                                {
                                    minPathLength = pathLength;
                                    minPathLocation = location;
                                }
                            }
                            pathPending = agent.SetDestination(minPathLocation);
                        }
                    }
                    infectedSetPathTimer = infectedSetPathTime;
                }
                infectedSetPathTimer -= Time.deltaTime;
                // Debug.Log("GOING THERE: " + pathPending);
                // Debug.Log("REMAINING: " + agent.remainingDistance);
                hospitalTileDistance = agent.remainingDistance;
            }
            if (isRecentlyInfected)
            {
                gameObject.tag = "Infected";  // Used only for finding truly infected people after watching ad
                if (GameManager.instance.areSymptomsDelayed)
                {
                    Timing.RunCoroutine(DelaySymptoms(), "DelaySymptoms " + GetInstanceID());
                }
                else
                {
                    Timing.RunCoroutine(InfectionProcess(), "InfectionProcess " + GetInstanceID());
                }
                Timing.RunCoroutine(infectionCylinderScript.SinusoidalRadius(), "SinusoidalRadius " + GetInstanceID());
                
                // GameManager.instance.infectedPathDistances[gameObject] = hospitalTileDistance;
                isRecentlyInfected = false;
                infectedPeopleTotal++;
                if (infectedPeopleTotal >= infectedPeopleBackgroundThreshold)
                {
                    mainCamera.backgroundColor = dangerBackgroundColor;
                }
            }
        }
        else if (agent.agentTypeID == Constants.priorityHealthyAgentID || agent.agentTypeID == Constants.priorityInfectedAgentID)
        {
            priorityHospitalReachTimer -= Time.deltaTime;
            if (priorityHospitalReachTimer < 0)
            {
                if (agent.agentTypeID == Constants.priorityHealthyAgentID)
                {
                    agent.agentTypeID = Constants.healthyUnboundAgentID;
                }
                else
                {
                    agent.agentTypeID = Constants.infectedAgentID;
                }
                HospitalTile.isOccupied = false;
                Debug.Log("UNOCCUPIED 2 BY " + gameObject.name);
                priorityHospitalReachTimer = priorityHospitalReachTime;
            }
        }
    }
    bool CalculateNewPath(Vector3 targetPos, bool usePriorityInfectedAgent)
    {
        if (usePriorityInfectedAgent)
        {
            Vector3 warpPosition = new Vector3(transform.position.x, tempAgentYPos, transform.position.z);
            tempAgent.Warp(warpPosition);  // If decided to not warp, tempAgent will stay at Hospital at all times
            // Debug.Log("Temp Location: " + tempAgent.transform.position);
            Vector3 newTargetPos = new Vector3(targetPos.x, tempAgentYPos, targetPos.z);
            tempAgent.CalculatePath(newTargetPos, navMeshPath);
        }
        else
        {
            agent.CalculatePath(targetPos, navMeshPath);
        }
        // Debug.Log("New path calculated");
        if (navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        return true;
    }

    public float GetPathLength(Vector3 targetPos, bool usePriorityInfectedAgent)
    {
        float pathLength = 0;
        if (CalculateNewPath(targetPos, usePriorityInfectedAgent))  // Resulting path stored in navMeshPath
        {
            for (int i = 1; i < navMeshPath.corners.Length; i++)
            {
                pathLength += Vector3.Distance(navMeshPath.corners[i - 1], navMeshPath.corners[i]);
            }
        }
        else
        {
            return float.MaxValue;
        }
        return pathLength;
    }

    public bool IsOnHospitalTile(float extraBorderWidth)
    {
        float halfTileSize = tileSize / 2;
        halfTileSize += extraBorderWidth;
        bool isInXRange = transform.position.x > -halfTileSize && transform.position.x < halfTileSize;
        bool isInZRange = transform.position.z > -halfTileSize && transform.position.z < halfTileSize;
        if (isInXRange && isInZRange)
        {
            return true;
        }
        return false;
    }

    void Heal()
    {
        if (isInfected)  // agent.agentTypeID == Constants.priorityInfectedAgentID || agent.agentTypeID == Constants.infectedAgentID
        {
            infectedPeopleTotal--;
            if (infectedPeopleTotal < infectedPeopleBackgroundThreshold)
            {
                mainCamera.backgroundColor = defaultBackgroundColor;
            }
            gameObject.tag = "Untagged";
            
            agent.acceleration = healthyAcceleration;
            int infectionProcessCoroutinesKilled = Timing.KillCoroutines("InfectionProcess " + GetInstanceID());
            Debug.Log("Killed on heal: " + infectionProcessCoroutinesKilled);
            Timing.KillCoroutines("SinusoidalRadius " + GetInstanceID());
            if (GameManager.instance.areSymptomsDelayed)
            {
                Timing.KillCoroutines("DelaySymptoms " + GetInstanceID());
            }
            infectionCylinderScript.gameObject.SetActive(false);
            meshRenderer.material = healthyMaterial;  // OUTSIDE (0.8.2)
            AudioManager.instance.SFX_Source.PlayOneShot(healSound, healSoundVolume);
            if (GameManager.instance.areParticlesOn)
            {
                Timing.KillCoroutines("PlayInfectionParticles " + GetInstanceID());
                infectionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                healParticles.Play();
            }
        }
        agent.agentTypeID = Constants.recentlyHealedAgentID;  // INSIDE?
        agent.SetDestination(hospitalTilePos);
    }

    IEnumerator<float> CheckIfOffHospitalTile()
    {
        while (IsOnHospitalTile(navMeshSurfaceAgentRadius))
        {
            yield return Timing.WaitForOneFrame;
        }
        agent.agentTypeID = Constants.healthyAgentID;
        if (GameManager.instance.canHealthyHeal)
        {
            mustStayHealthyBoundAgent = true;
            Timing.RunCoroutine(ChangeBackToHealthyUnboundAgent());
        }
        if (GameManager.instance.areSymptomsDelayed)  // Place in HealingProcess()?
        {
            meshRenderer.material = unknownMaterial;
        }
        HospitalTile.isOccupied = false;
        Debug.Log("UNOCCUPIED " + gameObject.name);
    }

    public IEnumerator<float> InfectionProcess()
    {
        gameObject.tag = "Infected";
        if (isNearHospital || arePathDestinationsSet)
        {
            agent.agentTypeID = Constants.infectedAgentID;
        }
        else
        {
            agent.agentTypeID = Constants.farInfectedAgentID;
        }
        agent.acceleration = infectedAcceleration;
        if (GameManager.instance.areParticlesOn)
        {
            Timing.RunCoroutine(PlayInfectionParticles(), "PlayInfectionParticles " + GetInstanceID());
        }

        float individualStageTime = infectionDeathDuration / infectedMaterials.Length;
        for (int i = 0; i < infectedMaterials.Length; i++)
        {
            meshRenderer.material = infectedMaterials[i];
            yield return Timing.WaitForSeconds(individualStageTime);
        }
        // Timing.StopCoroutine(SetDestinationAsHospitalContinuously());
        meshRenderer.material = deadMaterial;
        // Timing.KillCoroutines("SinusoidalRadius " + GetInstanceID());
        // infectionCylinderScript.gameObject.SetActive(false);
        agent.isStopped = true;
        Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
    }

    IEnumerator<float> PlayInfectionParticles()
    {
        yield return Timing.WaitForSeconds(infectionParticleStartTime);
        infectionParticles.Play();
    }

    IEnumerator<float> HealingProcess()
    {
        yield return Timing.WaitForSeconds(healTime);
        isInfected = false;
        isHealing = false;
        if (GameManager.instance.areParticlesOn)
        {
            healParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    IEnumerator<float> ChangeBackToHealthyUnboundAgent()
    {
        yield return Timing.WaitForSeconds(changeBackToHealthyUnboundAgentTime);
        mustStayHealthyBoundAgent = false;
        // agent.agentTypeID = Constants.healthyUnboundAgentID;
        hasRecentlyHealed = false;
    }

    public IEnumerator<float> DelaySymptoms()
    {
        yield return Timing.WaitForSeconds(GameManager.instance.symptomDelayTime);
        Timing.RunCoroutine(InfectionProcess(), "InfectionProcess " + GetInstanceID());
        infectionCylinderScript.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    IEnumerator<float> BackgroundFlash()
    {
        mainCamera.backgroundColor = dangerBackgroundColor;
        yield return Timing.WaitForSeconds(backgroundFlashTime);
        mainCamera.backgroundColor = defaultBackgroundColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInfected && other.CompareTag("InfectionCylinder") && other.transform.parent.gameObject != gameObject && !IsOnHospitalTile(0))  // May remove Hospital immunity
        {
            isInfected = true;
            isRecentlyInfected = true;
            if (!GameManager.instance.areSymptomsDelayed)
            {
                AudioManager.instance.SFX_Source.PlayOneShot(newInfectionSound, newInfectionSoundVolume);
            }
            // Timing.RunCoroutine(BackgroundFlash());
            // Debug.Log("PERSON TO PERSON");
        }
        else if (other.CompareTag("AvoidanceCylinder") && other.transform.parent.gameObject != gameObject && !isInfected)
        {
            agent.avoidancePriority = Random.Range(highestAvoidancePriority, lowestAvoidancePriority);
        }
        else if (other.CompareTag("NearHospital"))  // isNearHospital and "NearHospital" tag eems to be unused now
        {
            isNearHospital = true;
            if (agent.agentTypeID == Constants.farInfectedAgentID)
            {
                agent.agentTypeID = Constants.infectedAgentID;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NearHospital"))
        {
            isNearHospital = false;
        }
        else if (other.CompareTag("AvoidanceCylinder"))
        {
            agent.avoidancePriority = defaultAvoidancePriority;
        }
    }

    void GeneratePathModeDestinations(bool isSubtractingHospitalWalls)
    {
        float val1;
        if (isSubtractingHospitalWalls)
        {
            val1 = 6.17f;  // Distance from center of Hospital to InfectedSpheroid's NavMesh without wall (minus 0.03 to collide with Hospital barriers)
        }
        else
        {
            val1 = 6.5f;  // Distance from center of tile and through a wall of adjacent tile
        }
        float val2 = 3.5f;  // Distance from center of tile and touching wall on same tile
        pathModeDestinations[0] = new Vector3[]{};
        pathModeDestinations[1] = new Vector3[4];
        pathModeDestinations[2] = new Vector3[12];
        for (int i = 0; i < 4; i++)
        {
            float newVal1 = val1;
            if (i % 2 == 1)
            {
                newVal1 *= -1;
            }
            if (i < 2)
            {
                pathModeDestinations[1][i] = new Vector3(newVal1, yPos, 0);
                pathModeDestinations[2][i] = new Vector3(newVal1, yPos, 0);
            }
            else
            {
                pathModeDestinations[1][i] = new Vector3(0, yPos, newVal1);
                pathModeDestinations[2][i] = new Vector3(0, yPos, newVal1);
            }
        }
        for (int i = 0; i < 8; i++)
        {
            float newVal1 = val1;
            float newVal2 = val2;
            if (i % 2 == 1)
            {
                newVal1 *= -1;
            }
            if (i % 4 >= 2)
            {
                newVal2 *= -1;
            }
            if (i < 4)
            {
                pathModeDestinations[2][i + 4] = new Vector3(newVal1, yPos, newVal2);
            }
            else
            {
                pathModeDestinations[2][i + 4] = new Vector3(newVal2, yPos, newVal1);
            }
        }
    }
}