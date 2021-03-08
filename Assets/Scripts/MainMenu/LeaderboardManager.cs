using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Source: https://youtu.be/KZuqEyxYZCc

public class LeaderboardManager : MonoBehaviour
{
	internal static LeaderboardManager instance;

	//{Dreamlo private and public code pair for each leaderboard used}
	private string[] privateCodes = {
										"XH-C7irxHU69Du5NSODUIAPcassrlFlkKAaSWvS4Kbqw",
										"twf8XCfbREq9JkpLmy_X_gPoo9nbmWbk-xExOodKpWYw",
										"QExs4kDVL0etrqFUNqMa6QwjbqTqUIMk-kTdJ83wK-Vw",
										"VngGAYUZf0CM2oJfL23Ehwvji82GEQQ0aGe3xfmBs-fw",
										"DPiaLrq-GUWxthSDy7B9Ogen3SNSVYlUq2CeTIoXXNQQ",
										"2WhenWoV9UmvZHPrAIWlUwAYp5xC2kpkenzcI-HXKqag",
										"Wlby2p9aO0mcZtNC7r28tQX7CK07qqk0mQ6RichtwVZw",
										"cYYLuViMakeo0uF5reXVsQjc6s-uHd3Eun9iSt3okMtg",
										"wmnuebpHbUe8eXXsTh5BEAST8X3mGzKEuY6Uc-iq_IKQ"  // Overall leaderboard
									};
	private string[] publicCodes =	{
										
										"5f2b94d3eb371809c4afbd01",
										"5f2b9be1eb371809c4afce15",
										"5f2b9c2beb371809c4afcecb",
										"5f2b9d05eb371809c4afd0fa",
										"5f2b9d1aeb371809c4afd132",
										"5f2b9e24eb371809c4afd3f4",
										"5f2b9f33eb371809c4afd6a9",
										"5f2ba0bbeb371809c4afdaaf",
										"5f2b9566eb371809c4afbe68"
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
	private int currentlyDisplayedLeaderboard = 8;
	private string[] leaderboardStrings = {"Game Mode 0", "Game Mode 1", "Game Mode 2", "Game Mode 3", "Game Mode 4", "Game Mode 5", "Game Mode 6", "Game Mode 7", "Overall"};  //{Rename}

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
		myLocalHighScores = HighScoreLogger.instance.GetHighScores(true);
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

		for (int i = 0; i < myOnlineHighScores.Length - 1; i++)
        {
			if (myLocalHighScores[i] > myOnlineHighScores[i])
            {
				StartCoroutine(UploadGameModeHighScore(i, myLocalHighScores[i]));
            }
            else
            {
				finishedLeaderboardUpdates++;
            }
        }
		int overallHighScoreIndex = myOnlineHighScores.Length - 1;
		if (myLocalHighScores[overallHighScoreIndex] > myOnlineHighScores[overallHighScoreIndex])
        {
			StartCoroutine(UploadOverallHighScore(myLocalHighScores[overallHighScoreIndex]));
        }
        else
        {
			finishedLeaderboardUpdates++;
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

	public void UploadNewHighScores(int gameMode, int gameModeHighScore, int overallHighScore)  // Unused
	{
		StartCoroutine(UploadGameModeHighScore(gameMode, gameModeHighScore));
		StartCoroutine(UploadOverallHighScore(overallHighScore));
	}

	IEnumerator UploadGameModeHighScore(int gameMode, int score)
    {
		UnityWebRequest request = UnityWebRequest.Get(webURL + privateCodes[gameMode] + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();

		string gameModeHighScoreString = HighScoreLogger.instance.highScoreStrings[gameMode];
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

	IEnumerator UploadOverallHighScore(int score)
    {
		UnityWebRequest request = UnityWebRequest.Get(webURL + privateCodes[privateCodes.Length - 1] + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
		request.timeout = Constants.connectionTimeoutTime;
		yield return request.SendWebRequest();

		if (string.IsNullOrEmpty(request.error))
		{
			finishedLeaderboardUpdates++;
			// Debug.Log("Upload Successful with OverallHighScore");
		}
		else
		{
			messageText.text = "<color=#FF4040>Check your internet connection and try again.</color>";
			StopCoroutine(UploadAllHighScores());
			uploadScoresButtonComponent.interactable = true;
			Debug.Log("Error uploading OverallHighScore" + ": " + request.error);
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

		for (int i = 0; i < entries.Length; i++)
		{
			string[] entryInfo = entries[i].Split(new char[] {'|'});
			string username = entryInfo[0];
			int score = int.Parse(entryInfo[1]);
			currentOnlineHighScores[i] = new HighScore(username, score);
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

	void DisplayLocalHighScore()
    {
		myLocalScoreText.text = "Score: " + myLocalHighScores[currentlyDisplayedLeaderboard];
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