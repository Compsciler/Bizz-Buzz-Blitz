using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MEC;

// Source: https://www.youtube.com/watch?v=_nRzoTzeyxU&t
public class DialogueManager : MonoBehaviour
{
	internal static DialogueManager instance;

	// public Text nameText;
	public TMP_Text dialogueText;

	public Animator animator;

	public GameObject tileHighlightCanvas;
	public GameObject pauseButton;

	private Queue<string> sentences = new Queue<string>();
	private Queue<float> sentenceTimes = new Queue<float>();
	private Queue<GameObject> tilesToClick = new Queue<GameObject>();
	internal GameObject currentTileToClick;

	private float rotationSpeed = -75f;

	// Use this for initialization
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
	}

	public void StartDialogue(Dialogue dialogue)
	{
		animator.SetBool("IsOpen", true);

		// nameText.text = dialogue.name;

		sentences.Clear();

		foreach (string sentence in dialogue.sentences)
		{
			sentences.Enqueue(sentence);
		}
		foreach (float sentenceTime in dialogue.sentenceTimes)
        {
			sentenceTimes.Enqueue(sentenceTime);
		}
		foreach (GameObject tile in dialogue.tilesToClick)
        {
			tilesToClick.Enqueue(tile);
        }

		DisplayNextSentence();
	}

	public void DisplayNextSentence()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");  // Tag not necessary
			return;
		}

		string sentence = sentences.Dequeue();
		float sentenceTime = sentenceTimes.Dequeue();
		StopAllCoroutines();
		StartCoroutine(TypeSentence(sentence));
		StartCoroutine(TriggerNextSentence(sentenceTime));
	}

	IEnumerator TriggerNextSentence(float sentenceTime)
	{
		if (sentenceTime < 0)
        {
			currentTileToClick = tilesToClick.Dequeue();
			GameObject currentTileHighlightCanvas = Instantiate(tileHighlightCanvas, currentTileToClick.transform);
			currentTileHighlightCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
			GameObject currentTileHighlight = currentTileHighlightCanvas.transform.GetChild(0).gameObject;
			StartCoroutine(RotateImageContinuously(currentTileHighlight));
			RotateTile.tileClicksRemainingToTriggerDialogue = (int)-sentenceTime;
			Time.timeScale = 0;

			yield return new WaitUntil(() => RotateTile.tileClicksRemainingToTriggerDialogue <= 0);
			Time.timeScale = 1;
			currentTileToClick = null;
			StopCoroutine(RotateImageContinuously(currentTileHighlight));
			Destroy(currentTileHighlightCanvas);
        }
		else if (sentenceTime > 1000)
        {
			Time.timeScale = 0;
			pauseButton.GetComponent<Button>().interactable = false;
			yield return new WaitForSecondsRealtime(sentenceTime - 1000);
			Time.timeScale = 1;
			pauseButton.GetComponent<Button>().interactable = true;
		}
        else
        {
			yield return new WaitForSeconds(sentenceTime);
        }
		DisplayNextSentence();
	}

	IEnumerator TypeSentence(string sentence)
	{
		dialogueText.text = "";
		foreach (char letter in sentence.ToCharArray())
		{
			dialogueText.text += letter;
			yield return null;
		}
	}

	void EndDialogue()
	{
		animator.SetBool("IsOpen", false);
	}

	IEnumerator RotateImageContinuously(GameObject go)
    {
		while (true)
        {
			go.GetComponent<RectTransform>().Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);
			yield return null;
        }
    }
}