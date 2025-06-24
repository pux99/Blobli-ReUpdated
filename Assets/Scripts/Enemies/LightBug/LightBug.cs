using System.Collections.Generic;
using System.Net;
using Grid;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.MonoManager;

namespace Enemies.LightBug
{
    public class LightBug : Enemy, IUpdatable
    {
        private readonly GameObject _go;
        
        //Movement
        private readonly List<Vector3Int> _directions;
        private readonly int _speed;
        private Vector3Int _targetPos;
        private int _currentDirectionIndex = 0;
        private Vector3Int _remainingSteps;
        
        //Animation
        private readonly Sprite[] _animationFrames;
        private int _frameIndex = 0;
        private float _timer;
        private float _frameRate = 0.75f;
        
        //Lighting
        private readonly List<Vector3Int> lightCells = new List<Vector3Int>();
        private readonly TileBase[] _tileVariants;
        private readonly int _lightIntensity;
        
        public LightBug(GameObject go, LightBugStats stats, Sprite[] animation)
        {
            var sl = ServiceLocator.Instance;
            
            _go = go;
            
            //Light & Grid
            GridManager = sl.GetService<GridManager>();
            _lightIntensity = stats.lightIntensity;
            _tileVariants = GridManager.LightTileVariants;
            CellPos = GridManager.WorldToCell(stats.initialPosition);
            LightMap = GridManager.TileMaps.Light;
            UpdateLight();

            //Movement
            _directions = stats.directions;
            _speed = stats.speed;
            _currentDirectionIndex = 0;
            _remainingSteps = _directions[_currentDirectionIndex];
            
            //Animation
            _animationFrames = animation;
            SpriteRenderer = _go.GetComponent<SpriteRenderer>();
            sl.GetService<CustomMonoManager>().RegisterOnUpdate(this);
        }

        public void Tick(float deltaTime) //Acts like an UPDATE(), without MONO-BEHAVIOUR
        {
            _timer += Time.deltaTime;

            if (_timer >= _frameRate)
            {
                _timer = 0f;
                Animation();
            }
        }
        
        public void OnStepTaken() //Player event
        {
            Move();
            UpdateLight();
        }

        private void Animation() //Cycles through the sprite-sheet
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
            if (_directions.Count == 0 || _speed <= 0) return;
            
            int stepsLeft = _speed;

            while (stepsLeft > 0)
            {
                //When an element of the list is completed, jumps to the next or loops.
                if (_remainingSteps == Vector3Int.zero)
                {
                    _currentDirectionIndex = (_currentDirectionIndex + 1) % _directions.Count;
                    _remainingSteps = _directions[_currentDirectionIndex];
                }
                
                Vector3 movement = Vector3.zero;
                
                //Checks for remaining value on X
                if (_remainingSteps.x != 0)
                {
                    int step = Mathf.Clamp(_remainingSteps.x, -1, 1);
                    movement.x = step;
                    _remainingSteps.x -= step;
                }
                //Checks for remaining value on Y
                else if (_remainingSteps.y != 0)
                {
                    int step = Mathf.Clamp(_remainingSteps.y, -1, 1);
                    movement.y = step;
                    _remainingSteps.y -= step;
                }
                else
                {
                    continue;
                }
                
                stepsLeft--;
                
                //Moves the object
                _go.transform.position += movement;
                
            }
            CellPos = GridManager.WorldToCell(_go.transform.position);
        }
        
        private void UpdateLight()
        {
            //Deletes all the tiles from the previous spot
            foreach (var tile in lightCells)
            {
                LightMap.SetTile(tile, null);
            }
            lightCells.Clear();

            var cells = GridManager.GetCellsInRadius(CellPos, _lightIntensity);
            
            foreach (var pos in cells)
            {
                if (!GridManager.CanPlaceTile(pos)) continue;
                TileBase tile = _tileVariants[Random.Range(0, _tileVariants.Length)];
                LightMap.SetTile(pos, tile);
                lightCells.Add(pos);
            }
        }
        
        public override void OnSceneChange()
        {
            Object.Destroy(_go);
        }
    }
}
