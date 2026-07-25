using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private bool _immortal = false;

    private bool _dying = false;

    public event Action OnHit;
    public event Action OnDeath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TakeHit()
    {
        if (_dying)
        {
            return; 
        }

        if (!_immortal)
        {
            _health--;
        }

        Debug.Log("Take hit");

        OnHit?.Invoke();

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _dying = true;
        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}
