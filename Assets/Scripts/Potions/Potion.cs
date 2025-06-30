using System;
using System.Collections.Generic;
using System.Linq;
using Grid;
using Potions;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.MonoManager;

    [Serializable]
    public class Potion : IStartable
    {
        [Header("Drawing Shape")]
        [Tooltip("Temporary")]
        [SerializeField] private SO_ShadowShape shape; //This should not be here, it should be set by the Inventory
        private CustomMonoManager _customMonoManager;
        public Action PotionThrown;
        public bool ShadowIndicator { get; private set; } = false;
        #region Private variables
    
        private List<Vector3Int> _markedPositions = new(); //List that shows where the shadow would be placed
        private GridManager _gridManager;
        private Tilemap _shadowMap; //Map in which it will paint
        private Vector3 _playerDir;

        #endregion

        public void Awake()
        {
            _customMonoManager = ServiceLocator.Instance.GetService<CustomMonoManager>();
            _customMonoManager.RegisterOnStart(this);
        }


        public void Beginning()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
            _shadowMap = _gridManager.TileMaps.Shadow;
        }

        public void UsePotion(Vector3 dir)
        {
            _playerDir = dir;
            if (ShadowIndicator)
            {
                SetShadow();
                ShadowIndicator = false;
                PotionThrown?.Invoke();
            }
            else
            {
                ShowIndicator();
                ShadowIndicator = true;
            }
        }
        public void HideIndicator()
        {
            ShadowIndicator = false;
            ClearIndicator();
        }
        public void UpdateRotation(Vector3 dir)
        {
            _playerDir = dir;
            ClearIndicator();
            Vector3Int direction = Vector3Int.RoundToInt(_playerDir);
            _markedPositions = CalculatePositions(direction);
            DrawIndicator(_markedPositions);
        }
        public void SetShape(SO_ShadowShape newShape) //The inventory should set the shape of the potion
        {
            shape = newShape;
        }

        private void ShowIndicator() //Shows where the shadow would be placed
        {
            ClearIndicator();
            Vector3Int direction = Vector3Int.RoundToInt(_playerDir);
            _markedPositions = CalculatePositions(direction);
            DrawIndicator(_markedPositions);
        }
        private void SetShadow() //Sets shadow on the marker positions
        {
            foreach (var pos in _markedPositions.Where(pos => _gridManager.CanPlaceShadow(pos)))
            {
                _gridManager.TileMaps.Light.SetTile(pos, _gridManager.ShadowTile);
            }

            ClearIndicator();
        }
        private void ClearIndicator() //Resets the marked positions
        {
            foreach (var pos in _markedPositions)
            {
                _shadowMap.SetTile(pos, null);
            }
            
            _markedPositions.Clear();
        }
        private Vector2Int RotateDirection(Vector2Int pos, Vector3Int dir)
        {
            if (dir == Vector3Int.up) return pos; // Up (0, 1) → 0°
            if (dir == Vector3Int.down) return new Vector2Int(-pos.x, -pos.y); // Down (0, -1) → 180°
            if (dir == Vector3Int.right) return new Vector2Int(pos.y, -pos.x); // Right (1, 0) → 90°
            if (dir == Vector3Int.left) return new Vector2Int(-pos.y, pos.x); // Left (-1, 0) → -90°
            return pos; // No rotation
        }
        private List<Vector3Int> CalculatePositions(Vector3Int direction)
        {
            var playerPos = _gridManager.PlayerTile();
            var forward = playerPos + direction;

            return shape.relativePositions.Select(rel => RotateDirection(rel, direction)).Select(rotated => forward + new Vector3Int(rotated.x, rotated.y, 0)).ToList();
        }
        private void DrawIndicator(List<Vector3Int> positions)
        {
            var canPlaceAll = positions.All(pos => _gridManager.CanPlaceShadow(pos));
            var indicatorColor = canPlaceAll ? new Color(0, 0, 0, 200f / 255f) : new Color(1, 0, 0, 200f / 255f);
            _shadowMap.color = indicatorColor;

            foreach (var pos in positions)
            {
                _shadowMap.SetTile(pos, _gridManager.ShadowTile);
            }
        }

    }

