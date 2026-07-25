using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        LevelStart,
        Playing,
        LevelComplete,
        GameComplete
    }

    public static GameManager Instance;

    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private float _levelDuration = 120f;
    [SerializeField] private int _maxLevel = 3;

    private GameState _state;

    private float _gameTimer;

    private int _currentLevel = 0;


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        switch (_state)
        {
            case GameState.Playing:
                UpdatePlaying();
                break;
            default:
                break;
        }
    }

    private void SetState(GameState newState)
    {
        _state = newState;
        OnGameStateChanged?.Invoke(_state);
    }

    public void StartLevel()
    {
        SetState(GameState.LevelStart);

        _gameTimer = _levelDuration;

        SetState(GameState.Playing);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public float GetCountdown()
    {
        return _gameTimer;
    }

    void UpdatePlaying()
    {
        _gameTimer -= Time.deltaTime;
        if (_gameTimer <= 0)
        {
            EndLevel();
        }
    }

    void EndLevel()
    {
        SetState(GameState.LevelComplete);

        _currentLevel++;

        if (_currentLevel >= _maxLevel)
        {
            SetState(GameState.GameComplete);
        }
        else
        {
            StartLevel();
        }
    }
}
