using TMPro;
using UnityEngine;
using System;

public class Hud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _countdownText;
    [SerializeField] TextMeshProUGUI _sheepCountText;

    private FlockManager _flockManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetHudState;
        gameObject.SetActive(false);

        _flockManager = FindAnyObjectByType<FlockManager>();
        _flockManager.OnSheepKilled += UpdateSheepCounter;
    }

    void UpdateSheepCounter()
    {
        int sheepCount = _flockManager.GetSheepCount();
        int spawnCount = _flockManager.GetSpawnCount();

        _sheepCountText.text = $"Sheep: {sheepCount}/{spawnCount}";
    }

    void SetHudState(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelStart)
        {
            UpdateSheepCounter();
            gameObject.SetActive(true);
        }
        else if (state == GameManager.GameState.GameLost || state == GameManager.GameState.GameWon)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan time = TimeSpan.FromSeconds(GameManager.Instance.GetCountdown());
        _countdownText.text = time.ToString(@"m\:ss");
    }
}
