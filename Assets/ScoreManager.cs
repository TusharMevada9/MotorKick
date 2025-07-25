using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int leftScore = 0;
    public int rightScore = 0;
     public AdvancedCarAI carAI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoalScored(GoalTrigger.GoalSide side)
    {
        Debug.Log("GoalScored called");
        if (carAI == null)
        {
            Debug.LogWarning("carAI reference is not set in ScoreManager!");
        }
        if (side == GoalTrigger.GoalSide.Left)
        {
            rightScore++;
        }
        else
        {
            leftScore++;
        }
        Debug.Log($"Score - Left: {leftScore} | Right: {rightScore}");
        // TODO: Update UI here
        if (UIManager.Instance != null)
            UIManager.Instance.SetScore(leftScore, rightScore);
         if (carAI != null)
            carAI.ResetPositions();
    }
} 