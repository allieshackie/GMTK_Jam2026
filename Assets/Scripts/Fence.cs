using UnityEngine;

public class Fence : MonoBehaviour
{
    [SerializeField] private int _hitPoints = 2;
    [SerializeField] private GameObject _fenceModel;

    private int _currentHitPoints;

    private void Awake()
    {
        _currentHitPoints = _hitPoints;
    }

    public void Damage() 
    {
        _currentHitPoints -= 1;
        if (_currentHitPoints <= 0)
        {
            // Destroy Fence
            // Play Fence Destruction Animation

            _fenceModel.SetActive(false);
        }
    }

    public void RepairFence() 
    {
        _currentHitPoints = _hitPoints;
        _fenceModel.SetActive(true);
    }
}
