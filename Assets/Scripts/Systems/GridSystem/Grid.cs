using System;
using Unity.Mathematics;
using UnityEngine;
///
/// Credits:
/// 
/// CodeMonkey - https://www.youtube.com/watch?v=waEsGu--9P8&list=PLzDRvYVwl53uhO8yhqxcyjDImRjO9W722&index=1
/// 
/// "Making a grid system, and how to implement it"
///


namespace Manaflow.Systems
{
    public class Grid<TGridObject>
    {
        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int _width;
        private int _height;
        private float _cellSize;
        private Vector3 _originPosition;

        private TGridObject[,] _gridArray;
        private TextMesh[,] _debugTextArray;

        /// <summary>
        /// Creates a data grid that is relevant in 3D Space.
        /// </summary>
        /// <param name="width"> How long the grid is on the X axis </param>
        /// <param name="height"> How long the grid is on the Y axis </param>
        /// <param name="cellSize"> How big each cells are </param>
        /// <param name="originPosition"> Where start the grid in world space </param>
        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createTGridObject)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;
            _gridArray = new TGridObject[width, height];
            _debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x,y] = createTGridObject(this, x, y);
                }
            }

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _debugTextArray[x, y] = DebugUtils.CreateWorldText(_gridArray[x, y]?.ToString(), GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 15, Color.white);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x , y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                _debugTextArray[eventArgs.x, eventArgs.y].text = _gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };

        }

        // convert x / y keys into world coordinates.
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _originPosition;
        }

        public float GetCellSize()
        {
            return _cellSize;
        }

        // Convert world coordinate into x / y keys from the grid array.
        public void GetXY(Vector3 worldPosition, out int x, out int y) 
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        // setting the grid coordinate's value via direct x,y coordinates.
        public void SetValue(int x, int y, TGridObject value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _debugTextArray[x, y].text = value.ToString();
                _gridArray[x, y] = value;
                TriggerGridObjectChanged(x, y);
            }
        }

        // Setting the grid coordinate's value via vector 3 world position.
        public void SetValue(Vector3 worldPosition, TGridObject value)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            SetValue(x, y, value);
        }

        // Reports back a value from given coordinates.
        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }
            else
            {
                // What do we return if the player goes out of bounds? 
                return default;
            }
        }

        // world position wrapper for the get value.
        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetGridObject(x, y);
        }
    }
}
