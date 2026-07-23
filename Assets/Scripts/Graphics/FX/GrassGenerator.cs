using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    private ParticleSystem _grassSystem;

    private void Start()
    {
        _grassSystem = GetComponent<ParticleSystem>();
        _grassSystem.Pause();
    }
}
