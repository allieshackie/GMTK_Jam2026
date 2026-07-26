using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _countdownText;
    [SerializeField] TextMeshProUGUI _sheepCountText;

    [SerializeField] RectTransform  _indicator;

    [SerializeField] private float _indicatorTime = 3f;

    private FlockManager _flockManager;

    private AnchorManager _anchorManager;

    private Canvas _canvas;

    private float _indicatorTimer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetHudState;
        gameObject.SetActive(false);

        _flockManager = FindAnyObjectByType<FlockManager>();
        _flockManager.OnSheepKilled += UpdateSheepCounter;
        _indicator.gameObject.SetActive(false);

        _anchorManager = FindAnyObjectByType<AnchorManager>();
        _anchorManager.NewAnchorUnlocked += TrySetIndicator;

        _canvas = GetComponent<Canvas>();
    }

    void UpdateSheepCounter()
    {
        int sheepCount = _flockManager.GetSheepCount();
        int spawnCount = _flockManager.GetSpawnCount();

        _sheepCountText.text = $"Sheep: {sheepCount}/{spawnCount}";
    }

    void TrySetIndicator()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player)
        {
            Vector3 anchorPos = _anchorManager.GetAnchorPosition();
            float distance = Vector3.Distance(player.transform.position, _anchorManager.GetAnchorPosition());
            if(distance > 12f)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(anchorPos);
                RectTransform canvasRect = _canvas.transform as RectTransform;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _canvas.worldCamera, out Vector2 localPoint);
                // Keep the indicator inside the canvas
                float padding = 120f;
                float halfWidth = canvasRect.rect.width * 0.5f;
                float halfHeight = canvasRect.rect.height * 0.5f;

                localPoint.x = Mathf.Clamp(localPoint.x, -halfWidth + padding, halfWidth - padding);
                localPoint.y = Mathf.Clamp(localPoint.y, -halfHeight + padding, halfHeight - padding);

                _indicator.localPosition = localPoint;
                _indicator.gameObject.SetActive(true);

                _indicatorTimer = _indicatorTime;
            }
        }
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

        if (_indicatorTimer > 0)
        {
            _indicatorTimer -= Time.deltaTime;
            if (_indicatorTimer <= 0)
            {
                _indicator.gameObject.SetActive(false);
            }
        }
    }
}
