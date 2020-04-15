using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTarget : MonoBehaviour
{
    public int points;
    TextMeshPro textField;
    public Color flashColor = new Color(1f,0f,0f,1f);

    private Color previousColor;

    public void Start()
    {
        textField = GetComponent<TextMeshPro>();
        textField.text = "";
    }

    public void activate(int score, float delay = 0.2f)
    {
        previousColor = textField.faceColor;
        textField.faceColor = flashColor;
        textField.text = score.ToString();
        StartCoroutine(deactivate(delay));
    }

    IEnumerator deactivate(float time)
    {
        yield return new WaitForSeconds(time);
        textField.faceColor = previousColor;
        textField.text = "";
        yield return null;
    }
}
