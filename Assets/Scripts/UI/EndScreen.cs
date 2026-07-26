using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _endText;
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetEndScreenState;
        gameObject.SetActive(false);
    }

    void SetEndScreenState(GameManager.GameState state)
    {
        gameObject.SetActive(state == GameManager.GameState.GameWon || state == GameManager.GameState.GameLost);
        if (state == GameManager.GameState.GameWon)
        {
            _endText.text = "You Have Won";
        }
        else if (state == GameManager.GameState.GameLost)
        {
            _endText.text = "You Have Lost";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
