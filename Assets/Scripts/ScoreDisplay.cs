using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public int points;
    TextMeshPro textField;


    public void Start()
    {
        textField = GetComponent<TextMeshPro>();
        textField.text = "Score: 0";
    }

    public void SetScore(int score)
    {
        textField.text = "Score: " + score.ToString();
    }
}
