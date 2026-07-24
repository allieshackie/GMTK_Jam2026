using UnityEngine;

public class Lure : MonoBehaviour
{
    public float Radius;  
    public float LureStrength;

    private FlockManager _flockManager;

    private void Awake()
    {
        _flockManager = FindAnyObjectByType<FlockManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize(float radius, float strength)
    {
        Radius = radius;
        LureStrength = strength;

        _flockManager.AddLure(this);
    }

    private void OnDisable()
    {
        _flockManager.RemoveLure(this);
    }
}
