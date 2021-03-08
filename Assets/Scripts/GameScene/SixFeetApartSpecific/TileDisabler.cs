using UnityEngine;
using UnityEngine.AI;

public class TileDisabler : MonoBehaviour
{
    public GameObject[] tileSets;
    public bool[] tileSetEnables = {true, true};

    void Start()
    {
        for (int i = 0; i < tileSets.Length; i++)
        {
            if (!tileSetEnables[i])
            {
                foreach (GameObject tile in ExtensionMethods.GetChildren(tileSets[i]))
                {
                    tile.GetComponent<MeshRenderer>().enabled = false;
                    tile.GetComponent<BoxCollider>().enabled = false;
                    tile.GetComponent<RotateTile>().enabled = false;
                    tile.GetComponent<NavMeshObstacle>().enabled = true;
                }
            }
        }
    }
}