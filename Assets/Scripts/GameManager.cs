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

    private void Start()
    {
        StartLevel();
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

    void StartLevel()
    {
        _state = GameState.LevelStart;

        _gameTimer = _levelDuration;

        _state = GameState.Playing;
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
        _state = GameState.LevelComplete;

        _currentLevel++;

        if (_currentLevel >= _maxLevel)
        {
            _state = GameState.GameComplete;
        }
        else
        {
            StartLevel();
        }
    }
}
