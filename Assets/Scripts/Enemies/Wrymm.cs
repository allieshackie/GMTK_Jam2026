using System.Collections.Generic;
using UnityEngine;

public class Wrymm : MonoBehaviour
{
    [SerializeField] private FlockManager _flockManager;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _retreatSpeed = 3f;

    private List<Sheep> _allSheep;
    private Sheep _targetSheep;

    private bool _isRetreating;

    private void Start()
    {
        _allSheep = _flockManager.GetCurrentFlock();

        _targetSheep = GetClosestSheep();
    }

    private void Update()
    {
        if (_targetSheep == null)
            return;

        if (_isRetreating)
        {
            Retreat();
        }
        else
        {
            MoveTowardsSheep();
        }
    }

    private void MoveTowardsSheep()
    {
        Vector3 direction =
            (_targetSheep.transform.position - transform.position).normalized;

        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    private void Retreat()
    {
        // Move backwards relative to the Wrymm's forward direction
        transform.position -= transform.forward * _retreatSpeed * Time.deltaTime;
    }

    private Sheep GetClosestSheep()
    {
        Sheep closestSheep = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Sheep sheep in _allSheep)
        {
            if (sheep == null)
                continue;

            float distanceSqr =
                (sheep.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestSheep = sheep;
            }
        }

        return closestSheep;
    }

    private void OnTriggerEnter(Collider other)
    {
        Sheep sheep = other.GetComponent<Sheep>();

        if (sheep == null)
            return;

        if (sheep == _targetSheep)
        {
            _isRetreating = true;

            // Parent the sheep to the Wrymm
            sheep.transform.SetParent(transform);
        }
    }
}
