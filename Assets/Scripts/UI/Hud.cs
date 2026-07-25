using TMPro;
using UnityEngine;

public class Hud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _countdownText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetHudState;
        gameObject.SetActive(false);
    }

    void SetHudState(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelStart)
        {
            gameObject.SetActive(true);
        }
        else if (state == GameManager.GameState.LevelComplete)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _countdownText.text = GameManager.Instance.GetCountdown().ToString("0");
    }
}
