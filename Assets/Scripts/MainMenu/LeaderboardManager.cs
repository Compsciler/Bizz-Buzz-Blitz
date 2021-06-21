using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Source: https://youtu.be/KZuqEyxYZCc

public class LeaderboardManager : MonoBehaviour
{
	internal static LeaderboardManager instance;

	private string[] privateCodes = {
										"EreiRCzf-kKiYdA-tfFY2QU_FPvg2qNU6ffwXTTb_fcw",  // Overall Target
										"osJH-jT3L0KFteYMq5sGAAO3SWeXHpBkWUdSn_sUmgAQ",
										"Q_0hnklFE0imewaOHnXbvQC3hItJYoiEC1lnEevTo55w",
										"tNodvh5Evkqf5cg18SlgUgMUSqq-jCkEG6J4vw90oicw",
										"NHAJYDUGoECPUWGa0iYEyAzsuzJp9zvUeYwlcTcUzmGA",  // Overall Endless
										"P7SdI5jLqk65ZmEkpUS1BgQCLUEf_Sx0u4AH3OSZ63wQ",
										"94koCMYWQUeFjw0YITqAjQe63KFp5jHkSzCuL6VQ8i6w",
										"iWIG2Rxba0m6KP9rn6atwgXnRcrvFuQEqTe8xnu8R-Bg"
									};
	private string[] publicCodes =	{
										"60c7b45b8f40bb114c3ef898",
										"60c7b45e8f40bb114c3ef89b",
										"60c7b4608f40bb114c3ef8a0",
										"60c7b45f8f40bb114c3ef89e",
										"60c7b4628f40bb114c3ef8a4",
										"60c7b4638f40bb114c3ef8aa",
										"60c7b4638f40bb114c3ef8a7",
										"60c7b4638f40bb114c3ef8a8"
									};
	const string webURL = "http://dreamlo.com/lb/";

	internal static string username;
	internal static bool isPlayingAsGuest = false;

	private HighScore[][] allOnlineHighScores;

	[SerializeField] GameObject leaderboardsHolder;
	private GameObject[] leaderboards;

	[SerializeField] TMP_Text titleText;
	[SerializeField] TMP_Text messageText;
	[SerializeField] GameObject scrollView;
	[SerializeField] Button uploadScoresButtonComponent;
	[SerializeField] TMP_Text myUsernameText;
	[SerializeField] TMP_Text myLocalScoreText;

	private int maxScores = 100;
	private int finishedLeaderboardUpdates = 0;
	private int[] myLocalHighScores;

	private bool isFinishedDisplayingLeaderboards = false;
	private int finishedLeaderboardDownloads = 0;
	private int currentlyDisplayedLeaderboard = 0;
	private string[] leaderboardStrings =	{
												"Target Overall", "Target Set <color=green>A</color>", "Target Set <color=red>B</color>", "Target Set <color=blue>C</color>",
												"Endless Overall", "Endless Set <color=#00FFFF>A</color>", "Endless Set <color=purple>B</color>", "Endless Set <color=yellow>C</color>"
											};

	private int[][] leaderboardHighScores =	{
												new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8},
												new int[] {0, 1, 2, 4},
												new int[] {5, 6, 7, 8},
												new int[] {3},
												new int[] {50, 51, 52, 53, 54, 55, 56, 57, 58},
												new int[] {50, 51, 52, 54},
												new int[] {55, 56, 57, 58},
												new int[] {53}
											};

    void Awake()
    {
		if (instance == null)
		{
			instance = this;
		}

		leaderboards = leaderboardsHolder.GetChildren();
	}

    void Start()
    {
		allOnlineHighScores = new HighScore[publicCodes.Length][];
		myLocalHighScores = HighScoreLogger.instance.GetLeaderboardHighScores(leaderboardHighScores);
		myUsernameText.text = "Username: " + username;
		DisplayLocalHighScore();
	}

    void OnEnable()
    {
        if (!isFinishedDisplayingLeaderboards)
        {
			finishedLeaderboardDownloads = 0;
			messageText.text = "";
			StartCoroutine(DownloadAllHighScores(maxScores));
		}
    }

    IEnumerator UploadAllHighScores()  // Does not update equal high scores
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
					string[] entryInfo = requestText.Split(new char[] {'|'});
					myOnlineHighScores[i] = int.Parse(entryInfo[1]);
				}
			}
			else
			{
				StopCoroutine(UploadAllHighScores());
			}
		}

		for (int i = 0; i < myOnlineHighScores.Length; i++)
        {
			if (HighScoreLogger.IsImprovedScore(IsLeaderboardEndlessMode(i), myLocalHighScores[i], myOnlineHighScores[i]))
            {
				StartCoroutine(UploadGameModeHighScore(i, myLocalHighScores[i]));
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

	// Leaderboard mode instead of game mode
	public void UploadNewHighScores(int gameMode, int gameModeHighScore, int overallHighScore)  // Unused
	{
		StartCoroutine(UploadGameModeHighScore(gameMode, gameModeHighScore));
	}

	// Leaderboard mode instead of game mode
	IEnumerator UploadGameModeHighScore(int gameMode, int score)
    {
		UnityWebRequest request = UnityWebRequest.Get(webURL + privateCodes[gameMode] + "/delete/" + UnityWebRequest.EscapeURL(username));
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();
		request = UnityWebRequest.Get(webURL + privateCodes[gameMode] + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();

		string gameModeHighScoreString = leaderboardStrings[gameMode];
		if (string.IsNullOrEmpty(request.error))
		{
			finishedLeaderboardUpdates++;
			// Debug.Log("Upload Successful with " + gameModeHighScoreString);
		}
		else
		{
			messageText.text = "<color=#FF4040>Check your internet connection and try again.</color>";
			StopCoroutine(UploadAllHighScores());
			uploadScoresButtonComponent.interactable = true;
			Debug.Log("Error uploading " + gameModeHighScoreString + ": " + request.error);
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

	IEnumerator DownloadHighScores(int leaderboardNum, int maxScores)  // maxScores is unused
	{
		UnityWebRequest request = UnityWebRequest.Get(webURL + publicCodes[leaderboardNum] + "/pipe/");
		// UnityWebRequest request = UnityWebRequest.Get(webURL + publicCodes[leaderboardNum] + "/pipe/" + maxScores);
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
			Debug.Log("Error downloading high scores: " + request.error);
		}
	}

	public void DownloadHighScoresStartCoroutine(int leaderboardNum, int maxScores)  // Unused
	{
		StartCoroutine(DownloadHighScores(leaderboardNum, maxScores));
	}

	void FormatHighScores(int leaderboardNum, string textStream)
	{
		string[] entries = textStream.Split(new char[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
		HighScore[] currentOnlineHighScores = new HighScore[entries.Length];

		bool isAscending = !IsLeaderboardEndlessMode(leaderboardNum);
		for (int i = 0; i < entries.Length; i++)
		{
			string[] entryInfo = entries[i].Split(new char[] {'|'});
			string username = entryInfo[0];
			int score = int.Parse(entryInfo[1]);
			int highScoreIndex;
			if (isAscending)
            {
				highScoreIndex = entries.Length - i - 1;
			}
            else
            {
				highScoreIndex = i;
			}
			currentOnlineHighScores[highScoreIndex] = new HighScore(username, score);
			// Debug.Log(leaderboardNum + " | " + currentOnlineHighScores[i].username + ": " + currentOnlineHighScores[i].score);
		}
		allOnlineHighScores[leaderboardNum] = currentOnlineHighScores;
	}

	void DisplayHighScores()
    {
		for (int i = 0; i < publicCodes.Length; i++)
        {
			TMP_Text mainText = leaderboards[i].GetComponent<TMP_Text>();
			TMP_Text scoreText = mainText.transform.GetChild(0).GetComponent<TMP_Text>();  // Score Text should be Child 0
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

	public void DisplayLocalHighScore()
    {
		myLocalScoreText.text = "Score: " + myLocalHighScores[currentlyDisplayedLeaderboard];
    }

	public bool IsLeaderboardEndlessMode(int leaderboardNum)
	{
		return HighScoreLogger.IsEndlessGameMode(leaderboardHighScores[leaderboardNum][0]);
	}
}

public struct HighScore
{
	public string username;
	public int score;

	public HighScore(string username, int score)
	{
		this.username = username;
		this.score = score;
	}
}