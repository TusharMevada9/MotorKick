using UnityEngine;
using System.Collections;

public class demo : MonoBehaviour
{
    public CarBehavior[] cars; // Assign all car objects in the Inspector

    void Start()
    {
        StartCoroutine(GameStartRoutine());
    }

    IEnumerator GameStartRoutine()
    {
        // Disable all cars before countdown
        foreach (var car in cars)
            if (car != null) car.enabled = false;

        yield return StartCoroutine(UIManager.Instance.ShowCountdown());    

        // Enable all cars after countdown
        foreach (var car in cars)
            if (car != null) car.enabled = true;
    }
}


