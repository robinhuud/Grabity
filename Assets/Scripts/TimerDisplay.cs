using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public float time;
    TextMeshPro textField;

    public void Start()
    {
        textField = GetComponent<TextMeshPro>();
        textField.text = "Time: " + time.ToString();
    }

    public void SetTime(float t)
    {
        time = t;
        textField.text = "Time: " + time.ToString();
    }
}
