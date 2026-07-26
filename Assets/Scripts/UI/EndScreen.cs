using System.Collections;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _endText;

    private FlockManager _flockManager;

    private float charactersPerSecond = 6f;
    void Start()
    {
        GameManager.Instance.OnGameStateChanged += SetEndScreenState;
        gameObject.SetActive(false);
       _flockManager = FindAnyObjectByType<FlockManager>();
    }

    void SetEndScreenState(GameManager.GameState state)
    {
        gameObject.SetActive(state == GameManager.GameState.GameWon || state == GameManager.GameState.GameLost);
        if (state == GameManager.GameState.GameWon)
        {
            _endText.text = $"You Survived\n {_flockManager.GetSheepCount()} Sheep Saved";
            _endText.maxVisibleCharacters = 0;
            StartCoroutine(TypeRoutine());
        }
        else if (state == GameManager.GameState.GameLost)
        {
            _endText.text = $"You Have Lost";
            _endText.maxVisibleCharacters = 0;
            StartCoroutine(TypeRoutine());
        }
    }

    IEnumerator TypeRoutine()
    {
        while (_endText.maxVisibleCharacters < _endText.text.Length)
        {
            _endText.maxVisibleCharacters++;
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
