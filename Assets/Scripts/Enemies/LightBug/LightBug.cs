using System.Collections.Generic;
using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.MonoManager;

namespace Enemies.LightBug
{
    public class LightBug : Enemy, IUpdatable
    {
        private readonly List<Vector3Int> _path;
        private readonly int _speed;
        private readonly GameObject _go;
        private readonly Sprite[] _animationFrames;
        private int _currentSegmentIndex = 0;
        private int _progressOnSegment = 0;
        private bool _goingForward = true;
        
        
        //Animation
        private int _frameIndex = 0;
        private float _timer;
        [SerializeField] private float _frameRate = 0.75f;
        
        //Lighting
        private readonly List<Vector3Int> lightCells = new List<Vector3Int>();
        private readonly TileBase[] _tileVariants;
        private readonly int _lightIntensity;
        
        public LightBug(GameObject go,GridManager grid, List<Vector3Int> path, int speed, int lightIntensity, LightBugLevelSO so,Vector3Int position)
        {
            _go = go;
            GridManager = grid;
            _path = path;
            _speed = speed;
            _lightIntensity = lightIntensity;
            _tileVariants = so.tileVariants;
            _animationFrames = so.animationFrames;
            CellPos = GridManager.PositionInGrid(position);
            
            LightMap = grid.TileMaps.light;
            SpriteRenderer = _go.GetComponent<SpriteRenderer>();

            ServiceLocator.Instance.GetService<CustomMonoManager>().RegisterOnUpdate(this);
            UpdateLight();
        }

        public void OnStepTaken()
        {
            Move();
            UpdateLight();
        }

        private void Animation()
        {
            _frameIndex++;
            if (_frameIndex == _animationFrames.Length)
            {
                _frameIndex = 0;
            }
            SpriteRenderer.sprite = _animationFrames[_frameIndex];
        }

        private void Move()
        {
            
        }
        
        private void UpdateLight()
        {
            foreach (var tile in lightCells)
            {
                LightMap.SetTile(tile, null);
            }
            
            lightCells.Clear();
            
            foreach (var pos in GridManager.GetCellsInRadius(CellPos, _lightIntensity))
            {
                if (!GridManager.CanPlaceTile(pos)) continue;
                TileBase tile = _tileVariants[Random.Range(0, _tileVariants.Length)];
                LightMap.SetTile(pos, tile);
                lightCells.Add(pos);

            }
        }

        public void Tick(float deltaTime)
        {
            _timer += Time.deltaTime;

            if (_timer >= _frameRate)
            {
                _timer = 0f;
                Animation();
            }
        }
    }
}
