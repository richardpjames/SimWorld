using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseButton : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    // Start is called before the first frame update
    public void OnClick()
    {
        _panel.SetActive(false);
    }
}
