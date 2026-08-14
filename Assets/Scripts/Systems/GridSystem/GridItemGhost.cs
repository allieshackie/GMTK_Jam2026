using UnityEngine;

///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=dulosHPl82A&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=8
/// 
/// "Making a grid system, and how to implement it"
///

public class GridItemGhost : MonoBehaviour
{
    private GameObject _currentVisual;

    void Start()
    {
        RefreshVisual();

        GridBuildingSystem.Instance.OnSelectedGridItemChanged += OnSelectedChanged;
    }

    private void OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_currentVisual != null)
        {
            Destroy(_currentVisual);
            _currentVisual = null;
        }
        
        GridItemData data = GridBuildingSystem.Instance.GetGridItemDataType();
        if (data != null)
        {
            _currentVisual = Instantiate(data.Obj, Vector3.zero, Quaternion.identity);
            _currentVisual.transform.parent = transform;
            _currentVisual.transform.localPosition = Vector3.zero;
            _currentVisual.transform.localEulerAngles = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition();
        targetPosition.y= 1f;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem.Instance.GetPlacedItemRotation(), Time.deltaTime * 15f);
    }
}
