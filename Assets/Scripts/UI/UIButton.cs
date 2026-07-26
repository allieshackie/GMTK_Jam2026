using UnityEngine;
using FMODUnity;

public class UIButton : MonoBehaviour
{
    [SerializeField] private EventReference _buttonSound;
    [SerializeField] private EventReference _hoverSound;

    public void PlayClickSound()
    {
        RuntimeManager.PlayOneShot(_buttonSound);
    }

    public void PlayHoverSound()
    {
        RuntimeManager.PlayOneShot(_hoverSound);
    }
}
