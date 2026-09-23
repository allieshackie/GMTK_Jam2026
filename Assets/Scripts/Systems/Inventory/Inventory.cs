using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryUI;
    [SerializeField] private GameObject _pendingUI;
    [SerializeField] private Grid2D _inventoryGrid;
    [SerializeField] private Grid2D _pendingGrid;
    private Dictionary<GridItemData, int> _items = new();

    private Player_Controls _playerControls;
    private GridDragController _dragController;

    private Chest _activeChest;

    private bool _isInventoryOpen = false;

    private void Awake()
    {
        _playerControls = new Player_Controls();
        _playerControls.UI.ToggleInventory.performed += ToggleInventoryButton;
        _playerControls.UI.CloseInventory.performed += CloseInventoryButton;
        _playerControls.UI.Enable();

        _dragController = _inventoryUI.GetComponentInChildren<GridDragController>(true);
    }

    private void OnEnable()
    {
        _inventoryGrid.ItemAddedToGrid += HandleItemAdded;
        _inventoryGrid.ItemRemovedFromGrid += HandleItemRemoved;
    }

    private void OnDisable()
    {
        _inventoryGrid.ItemAddedToGrid -= HandleItemAdded;
        _inventoryGrid.ItemRemovedFromGrid -= HandleItemRemoved;
    }

    private void Start()
    {
        Canvas inventoryCanvas = _inventoryUI.GetComponent<Canvas>();
        Player player = FindAnyObjectByType<Player>();

        inventoryCanvas.transform.position = player.transform.position + Vector3.up * 10f;
        inventoryCanvas.transform.localScale = Vector3.one * 0.01f;

        Vector3 direction = inventoryCanvas.transform.position - Camera.main.transform.position;
        inventoryCanvas.transform.rotation = Quaternion.LookRotation(direction);
        
        _inventoryUI.SetActive(_isInventoryOpen);
        _pendingUI.SetActive(false);
    }

    private void ToggleInventoryButton(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    private void CloseInventoryButton(InputAction.CallbackContext context)
    {
        if (_isInventoryOpen)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        _isInventoryOpen = !_isInventoryOpen;

        if (!_isInventoryOpen)
        {
            // Note: Need to cancel before inventoryUI is closed, can't change item parenting 
            // while canvas is inactive
            _dragController?.CancelDrag();
            if (_pendingUI.activeSelf)
            {
                ClosePendingGrid();
            }
        }

        _inventoryUI.SetActive(_isInventoryOpen);
    }

    private void HandleItemAdded(InventoryItem item)
    {
        Add(item.Data, 1);
    }

    private void HandleItemRemoved(InventoryItem item)
    {
        Remove(item.Data, 1);
    }

    public void Add(GridItemData item, int amount)
    {
        if (!_items.ContainsKey(item))
        {   
            _items[item] = 0;
        }

        _items[item] += amount;
    }

    public void Remove(GridItemData ingredient, int amount)
    {
        if (_items.ContainsKey(ingredient))
        {   
            _items[ingredient] -= amount;
        }
    }

    public bool Has(GridItemData ingredient, int amount)
    {
        if (_items.ContainsKey(ingredient))
        {
            return _items[ingredient] >= amount;
        }

        return false;
    }

    public void OpenPendingGrid(Chest chest)
    {
        if (!_isInventoryOpen)
        {
            ToggleInventory();
        }
        _activeChest = chest;
        _pendingUI.SetActive(true);
        _pendingGrid.InitWithItems(chest.GetPendingItems());
    }

    private void ClosePendingGrid()
    {
        _dragController?.CancelDrag();

        _activeChest.SavePendingItems(_pendingGrid.GetCurrentPendingItems());
        _pendingGrid.ClearGrid();

        _activeChest = null;
        _pendingUI.SetActive(false);
    }
}
