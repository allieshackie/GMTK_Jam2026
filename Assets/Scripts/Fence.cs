using UnityEngine;
using FMODUnity;

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
        //RuntimeManager.PlayOneShotAttached("event:/Wrymm/fence_hit", gameObject);
        if (_currentHitPoints <= 0)
        {
            // Destroy Fence
            // Play Fence Destruction Animation
            RuntimeManager.PlayOneShotAttached("event:/Wyrms/fence_break", gameObject);
            gameObject.SetActive(false);
        }
    }

    public void RepairFence() 
    {
        _currentHitPoints = _hitPoints;
            gameObject.SetActive(false);
    }
}
