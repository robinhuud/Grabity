using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTarget : MonoBehaviour
{

    public int points;
    TextMeshPro textField;

    private Color previousColor;

    public void Start()
    {
        textField = GetComponent<TextMeshPro>();
        textField.text = "";
    }

    public void activate(int score)
    {
        previousColor = textField.faceColor;
        textField.faceColor = new Color(1f, 0f, 0f, 1f);
        textField.text = score.ToString();
        StartCoroutine(deactivate());
    }

    IEnumerator deactivate()
    {
        yield return new WaitForSeconds(.2f);
        textField.faceColor = previousColor;
        textField.text = "";
        yield return null;
    }
}
