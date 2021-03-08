using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    public float xTileRange = 40f;
    public float zTileRange = 20f;
    public float tileSize = 10f;

    [Range(0, 100)]
    public int wallTotal = 40;  // 40 + 64 - 4 = 100 is maximum wall total
    public GameObject wallPrefab;
    public Transform wallsGO_Transform;

    private bool canSpawnOnEdgeMidpoints = true;
    private bool canSpawnOnCorners = false;

    public GameObject[] tileTypeGroups;
    private bool[] movableTileGroupsEnables;

    public GameObject tutorialWalls;

    void Start()
    {
        switch (gameObject.GetComponent<SpawnPeople>().spawnPosListIndex)
        {
            case 0:
                break;
            case 1:
                canSpawnOnEdgeMidpoints = false;
                canSpawnOnCorners = true;
                break;
            case 2:
                canSpawnOnEdgeMidpoints = true;
                canSpawnOnCorners = true;
                break;
        }
        movableTileGroupsEnables = new bool[] {canSpawnOnEdgeMidpoints, canSpawnOnCorners};

        for (int i = 0; i < movableTileGroupsEnables.Length; i++)
        {
            if (movableTileGroupsEnables[i])
            {
                tileTypeGroups[i * 2].SetActive(true);
                tileTypeGroups[i * 2 + 1].SetActive(false);
            }
            else
            {
                tileTypeGroups[i * 2 + 1].SetActive(true);
                tileTypeGroups[i * 2].SetActive(false);
            }
        }
    }

    public void BeginGeneration()
    {
        if (GameManager.instance.isTutorial)
        {
            tutorialWalls.SetActive(true);
        }
        else
        {
            List<Vector3> tilePosList = new List<Vector3>();

            for (float x = -xTileRange; x <= xTileRange; x += tileSize)
            {
                for (float z = -zTileRange; z <= zTileRange; z += tileSize)
                {
                    bool isOnEdgeMidpoint = ((Mathf.Abs(x) == 0 && Mathf.Abs(z) == zTileRange) || (Mathf.Abs(x) == xTileRange && Mathf.Abs(z) == 0));
                    bool isOnCorner = (Mathf.Abs(x) == xTileRange && Mathf.Abs(z) == zTileRange);
                    if ((!canSpawnOnEdgeMidpoints || !isOnEdgeMidpoint) && (!canSpawnOnCorners || !isOnCorner))
                    {
                        tilePosList.Add(new Vector3(x, wallPrefab.transform.position.y, z));
                    }
                }
            }

            List<GameObject> walls = new List<GameObject>();
            for (int i = 0; i < wallTotal; i++)
            {
                Vector3 tilePos = tilePosList[Random.Range(0, tilePosList.Count)];
                GameObject wall = Instantiate(wallPrefab, tilePos, Quaternion.identity, wallsGO_Transform);

                // Rotate wall to one of four orientations
                wall.transform.Translate(Vector3.forward * tileSize / 2);
                wall.transform.RotateAround(tilePos, Vector3.up, Random.Range(0, 4) * 90);

                foreach (GameObject wall2 in walls)
                {
                    if (wall2.transform.position == wall.transform.position)  // wall2.transform.position.Equals(wall.transform.position
                    {
                        goto REPEAT;  // Essentially continues outer loop if position matches
                    }
                }
                wall.GetComponent<BoxCollider>().enabled = true;
                walls.Add(wall);
                continue;

            REPEAT:
                Destroy(wall);
                i--;
            }
        }
    }
}