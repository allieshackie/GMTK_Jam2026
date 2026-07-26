using FMODUnity;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public void OnFootstepApply()
    {
        RuntimeManager.PlayOneShotAttached("event:/Player/player_footstep", gameObject);
    }
}
