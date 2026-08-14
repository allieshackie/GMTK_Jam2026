using UnityEngine;
using UnityEngine.InputSystem;

///
/// Credits :
/// Code Monkey - "Debugging Utilities"
/// 

namespace Manaflow.Systems
{
    public static class DebugUtils
    {

        public static TextMesh CreateWorldText(string text, Vector3 worldPosition = default(Vector3), int fontSize = 1, Color color = default(Color))
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            gameObject.transform.position = worldPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = fontSize;
            return textMesh;
        }

        public static Vector3 MouseInWorldSpace(Camera cam)
        {
            return new Vector3(cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x, cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()).y);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        // 3D test setup
        public static Vector3 GetMouseWorldPosition3D()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 999f))
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

    }
}
