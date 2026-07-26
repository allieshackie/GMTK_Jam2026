using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        LevelStart,
        Playing,
        LevelComplete,
        GameWon,
        GameLost
    }

    public static GameManager Instance;

    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private float _levelDuration = 120f;
    [SerializeField] private int _maxLevel = 3;

    private GameState _state;

    private float _gameTimer;

    private int _currentLevel = 0;

    private FlockManager _flockManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
        _flockManager = FindAnyObjectByType<FlockManager>();
        _flockManager.LastSheepKilled += GameLost;
    }

    private void GameLost()
    {
        SetState(GameState.GameLost);
        LevelCleanup();
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

    void LevelCleanup()
    {
        // Kill all enemies on level end
        Enemy[] allEnemies = FindObjectsByType<Enemy>();
        foreach(Enemy enemy in allEnemies)
        {
            Destroy(enemy);
        }
    }

    void EndLevel()
    {
        SetState(GameState.LevelComplete);

        LevelCleanup();
        _currentLevel++;

        if (_currentLevel >= _maxLevel)
        {
            SetState(GameState.GameWon);
        }
        else
        {
            StartLevel();
        }
    }
}
