using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeControls : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Sprite _activeSprite;

    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _ffButton;

    public void SetTimeMultiplierPause()
    {
        // Set the sprites
        _pauseButton.GetComponent<Image>().sprite = _activeSprite;
        _playButton.GetComponent<Image>().sprite = _sprite;
        _ffButton.GetComponent<Image>().sprite = _sprite;
        // Set the time mulitplier
        GameManager.Instance.TimeMultiplier = 0;
    }
    public void SetTimeMultiplierPlay()
    {
        // Set the sprites
        _pauseButton.GetComponent<Image>().sprite = _sprite;
        _playButton.GetComponent<Image>().sprite = _activeSprite;
        _ffButton.GetComponent<Image>().sprite = _sprite;
        // Set the time mulitplier
        GameManager.Instance.TimeMultiplier = 1;
    }
    public void SetTimeMultiplierFF()
    {        
        // Set the sprites
        _pauseButton.GetComponent<Image>().sprite = _sprite;
        _playButton.GetComponent<Image>().sprite = _sprite;
        _ffButton.GetComponent<Image>().sprite = _activeSprite;
        // Set the time mulitplier
        GameManager.Instance.TimeMultiplier = 2;
    }
}
