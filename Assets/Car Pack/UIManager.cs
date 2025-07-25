using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score")] public TextMeshProUGUI leftScoreText;
    public TextMeshProUGUI rightScoreText;

    [Header("Timer")] public TextMeshProUGUI timerText;

    [Header("Boost Bar")] public Image boostBarFill;

    [Header("Goal Replay")] public GameObject goalReplayPanel;
    public TextMeshProUGUI goalReplayText;

    [Header("Win/Lose")] public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;

    [Header("Pause")] public GameObject pausePanel;

    [Header("Countdown")] public TextMeshProUGUI countdownText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetScore(int left, int right)
    {
        if (leftScoreText) leftScoreText.text = left.ToString();
        if (rightScoreText) rightScoreText.text = right.ToString();
    }

    public void SetTimer(float time)
    {
        if (timerText) timerText.text = Mathf.FloorToInt(time).ToString("00");
    }

    public void SetBoost(float percent)
    {
        if (boostBarFill) boostBarFill.fillAmount = percent;
    }

    public void ShowGoalReplay(string message)
    {
        if (goalReplayPanel) goalReplayPanel.SetActive(true);
        if (goalReplayText) goalReplayText.text = message;
    }
    public void HideGoalReplay()
    {
        if (goalReplayPanel) goalReplayPanel.SetActive(false);
    }

    public void ShowEndGame(string message)
    {
        if (endGamePanel) endGamePanel.SetActive(true);
        if (endGameText) endGameText.text = message;
    }
    public void HideEndGame()
    {
        if (endGamePanel) endGamePanel.SetActive(false);
    }

    public void ShowPause()
    {
        if (pausePanel) pausePanel.SetActive(true);
    }
    public void HidePause()
    {
        if (pausePanel) pausePanel.SetActive(false);
    }

    public IEnumerator ShowCountdown(float delay = 1f)
    {
        if (countdownText == null) yield break;
        countdownText.gameObject.SetActive(true);
        string[] sequence = { "3", "2", "1", "GO!" };
        foreach (var s in sequence)
        {
            countdownText.text = s;
            yield return new WaitForSeconds(delay);
        }
        countdownText.gameObject.SetActive(false);
    }
} 