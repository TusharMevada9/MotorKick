using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int leftScore = 0;
    public int rightScore = 0;

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
    }
} 