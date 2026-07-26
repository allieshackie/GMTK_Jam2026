using System;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    [SerializeField] private Anchor[] _anchors;

    public event Action NewAnchorUnlocked;

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
        NewAnchorUnlocked?.Invoke();
    }

    public Vector3 GetAnchorPosition()
    {
        if (_activeAnchor == null)
        {
            return Vector3.zero;
        }
        return _activeAnchor.transform.position;
    }

    void HandleAnchorComplete()
    {
        _activeAnchor.OnTimerComplete -= HandleAnchorComplete;
        _activeIndex++;
        SetActiveAnchor();
    }
}
