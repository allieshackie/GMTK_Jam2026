using System;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    [SerializeField] private Anchor[] _anchors;

    private int _activeIndex;
    private Anchor _activeAnchor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChanged;
    }

    void GameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelStart)
        {
            SetActiveAnchor();        
        }
    }

    void SetActiveAnchor()
    {
        if (_activeIndex >= _anchors.Length)
        {
            return;
        }
        _activeAnchor = _anchors[_activeIndex];
        _activeAnchor.OnTimerComplete += HandleAnchorComplete;
        _activeAnchor.SetActiveState(true);
    }

    void HandleAnchorComplete()
    {
        _activeAnchor.OnTimerComplete -= HandleAnchorComplete;
        _activeIndex++;
        SetActiveAnchor();
    }
}
