using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicManager : MonoBehaviour
{
    [SerializeField]
    ScoreDisplay scoreDisplay;
    [SerializeField]
    TimerDisplay timerDisplay;
    [SerializeField]
    BallsDisplay ballsDisplay;

    private int balls = 9;
    private float timer = 30f;
    private int currentScore = 0;
    private bool timerRunning = false;

    public static GameLogicManager Instance;

    public void Awake()
    {
        Instance = this;
        if(scoreDisplay is null)
        {
            scoreDisplay = GetComponent<ScoreDisplay>();
            Debug.Assert(scoreDisplay != null, "No Score Display Object!");
        }
        if(timerDisplay is null)
        {
            timerDisplay = GetComponent<TimerDisplay>();
            Debug.Assert(timerDisplay != null, "No Timer Display Object!");
        }
        if (ballsDisplay is null)
        {
            ballsDisplay = GetComponent<BallsDisplay>();
            Debug.Assert(ballsDisplay != null, "No Balls Display Object!");
        }
    }

    public void AddScore(int score)
    {
        currentScore += score;
        scoreDisplay.SetScore(currentScore);
    }

    public void AddBalls(int count)
    {
        balls += count;
        ballsDisplay.SetBalls(balls);
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public bool CanSpawn()
    {
        return (balls > 0);
    }

    public void TimerExpired()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        timerDisplay.SetTime(timer);
        scoreDisplay.SetScore(currentScore);
        ballsDisplay.SetBalls(balls);
    }

    // Update is called once per frame
    void Update()
    {
        if(timerRunning)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                TimerExpired();
            }
            else
            {
                timerDisplay.SetTime(timer);
            }
        }
    }
}
