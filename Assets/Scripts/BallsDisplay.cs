using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BallsDisplay : MonoBehaviour
{
    public int balls;
    TextMeshPro textField;

    public void Start()
    {
        textField = GetComponent<TextMeshPro>();
        textField.text = "Balls: " + balls.ToString();
    }

    public void SetBalls(int b)
    {
        balls = b;
        textField.text = "Balls: " + balls.ToString();
    }
}
