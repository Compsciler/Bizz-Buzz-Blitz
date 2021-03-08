using System.Collections;
using UnityEngine;

// Source: https://www.youtube.com/watch?v=_nRzoTzeyxU&t

public class DialogueTrigger : MonoBehaviour
{
	public Dialogue dialogue;
	public GameObject dialogueManager;
	public float startDelay;

	/*
	public void TriggerDialogue()
	{
		FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
	}
	*/

	public IEnumerator TriggerDialogue()
    {
		yield return new WaitForSeconds(startDelay);
		dialogueManager.GetComponent<DialogueManager>().StartDialogue(dialogue);
    }
}