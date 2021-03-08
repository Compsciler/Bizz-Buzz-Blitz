using TMPro;
using UnityEngine;

public class TestingButton : MonoBehaviour
{
    public TMP_InputField inputField;

    public void TestingFunction()
    {
        // Debug.Log("Hospital occupied: " + HospitalTile.isOccupied);
        // Debug.Log(Time.timeScale);
        // GameObject.Find("Pause Button").GetComponent<Button>().interactable = false;
        // Application.OpenURL("https://www.cdc.gov/coronavirus/2019-ncov/prevent-getting-sick/social-distancing.html");

        // GameObject person = GameObject.Find("Person " + inputField.text);
        // Debug.Log("Speed: " + person.GetComponent<NavMeshAgent>().velocity.magnitude);
        // person.GetComponent<NavMeshAgent>().avoidancePriority = 45;
        // Debug.Log("Remaining Distance: " + person.GetComponent<NavMeshAgent>().remainingDistance);

        // Debug.Log(PersonController.infectedPeopleTotal);
        // GameManager.instance.ResetStaticVariables();

        // Debug.Log(Screen.width + "x" + Screen.height);
        if (Time.timeScale > 1)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 10;
        }
    }
}