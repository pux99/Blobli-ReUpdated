using System.Collections.Generic;
using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Enemies.LightBug
{
    public class LightBug : Enemy
    {
        //Path vars
        private readonly Transform _transform;
        private readonly List<Vector3Int> _path;
        private readonly int _speed;
        private int _currentPathIndex = 0;
    
        //Animation
        private int _currentAnimFrame = 0;
    
        //Grid vars
        private HashSet<Vector3Int> _lightCells = new();
        private readonly int _intensity;
    
        //SO
        private readonly LightBugSO _config;
    
        public LightBug(GameObject gameObject, LightBugSO config, GridManager gridManager, int intensity, int speed)
        {
            _transform = gameObject.transform;
            SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            GridManager = gridManager;
            _config = config;
            _intensity = intensity;
            _speed = speed;
            //_path = path;
            LightMap = GridManager.TileMaps.light;

            CellPos = GridManager.PositionInGrid(_transform.position);
        
            UpdateState();
        }

        public void OnStepTaken()
        {
            //Move it to TICK
            Animation();

            // 2. Move along path
            Move();
        
            UpdateState();
        }

        private void Animation() //Move it to Tick
        {
            _currentAnimFrame = (_currentAnimFrame + 1) % _config.animationFrames.Length;
            SpriteRenderer.sprite = _config.animationFrames[_currentAnimFrame];
        }

        private void Move()
        {
            var dir = new Vector3(-1,0,0);
            if (dir == Vector3.zero) return;

            if (!GridManager.CanEnemyMove(_transform.position,dir, _speed)) return;
            _transform.position += dir;
            CellPos = GridManager.PositionInGrid(_transform.position);
        }

        private void UpdateState()
        {
            foreach (var cell in _lightCells)
            {
                LightMap.SetTile(cell, null);
            }
            _lightCells.Clear();
        
            foreach (var offset in GridManager.GetCellsInRadius(CellPos,_intensity))
            {
                Vector3Int cell = CellPos + offset;
                if (!GridManager.IsShadowTile(cell)) continue;
                TileBase tile = _config.tileVariants[Random.Range(0, _config.tileVariants.Length)];
                LightMap.SetTile(cell, tile);
                _lightCells.Add(cell);
            }
        }
    }
}
