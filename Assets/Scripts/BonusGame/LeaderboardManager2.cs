using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardManager2 : MonoBehaviour
{
	internal static LeaderboardManager2 instance;

	private string[] privateCodes = {
										"p3mTxSnc80yA20Z6u31SCwTXuvNiF2qUyKVllm74xGGg"
									};
	private string[] publicCodes =  {
										"5f4c06eaeb371809c4001b89"
									};
	const string webURL = "http://dreamlo.com/lb/";

	internal static string username;

	private HighScore2[][] allOnlineHighScores;  // Change to 2D array to hold a struct for each game mode
	public GameObject[] leaderboards;
	public TMP_Text titleText;
	public TMP_Text messageText;
	public GameObject scrollView;
	public Button uploadScoresButtonComponent;
	public TMP_Text myUsernameText;
	public TMP_Text myLocalScoreText;

	private int maxScores = 100;
	private int finishedLeaderboardUpdates = 0;
	private int[] myLocalHighScores;

	private bool isFinishedDisplayingLeaderboards = false;
	private int finishedLeaderboardDownloads = 0;
	private int currentlyDisplayedLeaderboard = 0;
	private string[] leaderboardStrings = {"Leaderboard"};

	void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

	void Start()
	{
		allOnlineHighScores = new HighScore2[publicCodes.Length][];
		username = LeaderboardManager.username;
		myUsernameText.text = "Username: " + username;
		// myLocalHighScores = new int[] {PlayerPrefs.GetInt("BonusGameHighScore")};
		// DisplayLocalHighScore();
	}

	void OnEnable()
	{
		if (!isFinishedDisplayingLeaderboards)
		{
			finishedLeaderboardDownloads = 0;
			messageText.text = "";
			StartCoroutine(DownloadAllHighScores(maxScores));
		}
		myLocalHighScores = new int[] {PlayerPrefs.GetInt("BonusGameHighScore")};
		DisplayLocalHighScore();
	}

	IEnumerator UploadAllHighScores()  // Doesn't update equal high scores
	{
		messageText.text = "Uploading high scores to database...";
		uploadScoresButtonComponent.interactable = false;

		finishedLeaderboardUpdates = 0;

		int[] myOnlineHighScores = new int[publicCodes.Length];  // Includes overall high score
		for (int i = 0; i < myOnlineHighScores.Length; i++)
		{
			UnityWebRequest request = UnityWebRequest.Get(webURL + publicCodes[i] + "/pipe-get/" + username);
			request.timeout = Constants.connectionTimeoutTime;
			yield return request.SendWebRequest();

			if (string.IsNullOrEmpty(request.error))
			{
				string requestText = request.downloadHandler.text;
				if (string.IsNullOrEmpty(requestText))
				{
					myOnlineHighScores[i] = 0;
				}
				else
				{
					string[] entryInfo = requestText.Split(new char[] { '|' });
					myOnlineHighScores[i] = int.Parse(entryInfo[1]);
				}
			}
			else
			{
				// errorText.gameObject.SetActive(true);
				// errorText.text = "Error uploading username! Restart app and try again.";
				StopCoroutine(UploadAllHighScores());
			}
		}

		for (int i = 0; i < myOnlineHighScores.Length; i++)
		{
			if (myLocalHighScores[i] > myOnlineHighScores[i])
			{
				StartCoroutine(UploadHighScore(i, myLocalHighScores[i]));
			}
			else
			{
				finishedLeaderboardUpdates++;
			}
		}
		yield return new WaitUntil(() => finishedLeaderboardUpdates == myOnlineHighScores.Length);
		messageText.text = "Upload successful!";
		isFinishedDisplayingLeaderboards = false;
		OnEnable();
	}

	public void UploadAllHighScoresStartCoroutine()
	{
		StartCoroutine(UploadAllHighScores());
	}

	/*
	public void UploadNewHighScores(int gameModeHighScore)
	{
		StartCoroutine(UploadGameModeHighScore(gameModeHighScore));
		StartCoroutine(UploadOverallHighScore());
	}
	*/

	IEnumerator UploadHighScore(int gameMode, int score)
	{
		UnityWebRequest request = UnityWebRequest.Get(webURL + privateCodes[gameMode] + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();

		// string gameModeHighScoreString = HighScoreLogger.instance.highScoreStrings[gameMode];
		if (string.IsNullOrEmpty(request.error))
		{
			finishedLeaderboardUpdates++;
			Debug.Log("Upload Successful with " + leaderboardStrings[gameMode]);
		}
		else
		{
			messageText.text = "<color=#FF4040>Check your internet connection and try again.</color>";
			StopCoroutine(UploadAllHighScores());
			uploadScoresButtonComponent.interactable = true;
			Debug.Log("Error uploading " + leaderboardStrings[gameMode] + ": " + request.error);
		}
	}

	IEnumerator DownloadAllHighScores(int maxScores)
	{
		messageText.text = "Retrieving scores from database...";

		for (int i = 0; i < publicCodes.Length; i++)
		{
			StartCoroutine(DownloadHighScores(i, maxScores));
			yield return null;
		}
		yield return new WaitUntil(() => finishedLeaderboardDownloads == publicCodes.Length);
		DisplayHighScores();
		isFinishedDisplayingLeaderboards = true;
		messageText.text = "Request successful!";
	}

	IEnumerator DownloadHighScores(int leaderboardNum, int maxScores)
	{
		UnityWebRequest request = UnityWebRequest.Get(webURL + publicCodes[leaderboardNum] + "/pipe/" + maxScores);
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();

		if (string.IsNullOrEmpty(request.error))
		{
			finishedLeaderboardDownloads++;
			FormatHighScores(leaderboardNum, request.downloadHandler.text);
		}
		else
		{
			messageText.text = "<color=#FF4040>Check your internet connection and re-enter the menu.</color>";
			StopCoroutine(DownloadAllHighScores(maxScores));
			Debug.Log("Error Downloading: " + request.error);
		}
	}

	/*
	public void DownloadHighScoresStartCoroutine(int leaderboardNum, int maxScores)
	{
		StartCoroutine(DownloadHighScores(leaderboardNum, maxScores));
	}
	*/

	void FormatHighScores(int leaderboardNum, string textStream)
	{
		string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		HighScore2[] currentOnlineHighScores = new HighScore2[entries.Length];

		for (int i = 0; i < entries.Length; i++)
		{
			string[] entryInfo = entries[i].Split(new char[] { '|' });
			string username = entryInfo[0];
			int score = int.Parse(entryInfo[1]);
			currentOnlineHighScores[i] = new HighScore2(username, score);
			Debug.Log(leaderboardNum + " | " + currentOnlineHighScores[i].username + ": " + currentOnlineHighScores[i].score);
		}
		allOnlineHighScores[leaderboardNum] = currentOnlineHighScores;
	}

	void DisplayHighScores()
	{
		for (int i = 0; i < publicCodes.Length; i++)
		{
			TMP_Text mainText = leaderboards[i].GetComponent<TMP_Text>();
			TMP_Text scoreText = mainText.transform.Find("Score Text").GetComponent<TMP_Text>();
			mainText.text = "";
			scoreText.text = "";
			for (int j = 0; j < allOnlineHighScores[i].Length; j++)
			{
				mainText.text += "\n" + (j + 1) + ")\t" + allOnlineHighScores[i][j].username;
				scoreText.text += "\n" + allOnlineHighScores[i][j].score;
			}
		}
	}

	public void ChangeLeaderboard(bool isChangingForward)
	{
		leaderboards[currentlyDisplayedLeaderboard].SetActive(false);
		currentlyDisplayedLeaderboard += (isChangingForward) ? 1 : -1;
		currentlyDisplayedLeaderboard = (currentlyDisplayedLeaderboard + leaderboards.Length) % leaderboards.Length;
		leaderboards[currentlyDisplayedLeaderboard].SetActive(true);

		titleText.text = leaderboardStrings[currentlyDisplayedLeaderboard];
		scrollView.GetComponent<ScrollRect>().content = leaderboards[currentlyDisplayedLeaderboard].GetComponent<RectTransform>();
		DisplayLocalHighScore();
	}

	void DisplayLocalHighScore()
	{
		myLocalScoreText.text = "Score: " + myLocalHighScores[currentlyDisplayedLeaderboard];
	}
}

public struct HighScore2
{
	public string username;
	public int score;

	public HighScore2(string username, int score)
	{
		this.username = username;
		this.score = score;
	}
}