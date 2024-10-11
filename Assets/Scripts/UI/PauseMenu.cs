using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    private int _previousTimeMultiplier;

    private void OnEnable()
    {
        _previousTimeMultiplier = GameManager.Instance.TimeMultiplier;
        // Set the time multiplier to pause the game
        GameManager.Instance.TimeMultiplier = 0;
    }

    // On disabling, reset the time multiplier for the game
    private void OnDisable()
    {
        GameManager.Instance.TimeMultiplier = _previousTimeMultiplier;
    }

    // For the quit button we simply call the game manager to go to the main menu
    public void Quit()
    {
        GameManager.Instance.MainMenu();
    }

    // For the resume button we disable (which will reset the time multiplier)
    public void Resume()
    {
        _pauseMenu.SetActive(false);
    }
}
