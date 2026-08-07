using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _inventoryUI;
    [SerializeField] private CanvasGroup _canvasGroup;

    private List<InventoryUIItem> _uiSlots = new List<InventoryUIItem>();
    private Player_Controls _playerControls;
    private bool _showing;

    private void Awake()
    {
        _playerControls = new Player_Controls();
    }

    private void OnEnable()
    {
        _playerControls.UI.ToggleInventory.performed += OnToggleMenu;
        _playerControls.UI.Enable();
    }

    private void ODisable()
    {
        _playerControls.UI.ToggleInventory.performed -= OnToggleMenu;
        _playerControls.UI.Disable();
    }

    void Start()
    {
        Show(false);
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        if (_canvasGroup != null)
        {
            Show(!_showing);
        }
    }

    void Show(bool show)
    {
        _showing = show;
        if (show)
        {
            _canvasGroup.alpha = 1f; 
            _canvasGroup.interactable = true; 
            _canvasGroup.blocksRaycasts = true; 
        }
        else
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void UpdateGrid(List<InventoryItem> itemList) 
    {
        // Instantiate slots if they don't exist yet
        while (_uiSlots.Count < itemList.Count) 
        {
            GameObject newSlot = Instantiate(_inventoryUI, _content);
            _uiSlots.Add(newSlot.GetComponent<InventoryUIItem>());
        }

        // Bind data state directly onto UI elements
        for (int i = 0; i < itemList.Count; i++) 
        {
            _uiSlots[i].InitSlot(itemList[i]);
        }
    }
}
