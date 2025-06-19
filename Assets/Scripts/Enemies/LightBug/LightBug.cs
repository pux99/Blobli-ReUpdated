using System.Collections.Generic;
using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.MonoManager;

namespace Enemies.LightBug
{
    public class LightBug : Enemy, IUpdatable
    {
        private readonly List<Vector2Int> _path;
        private readonly int _speed;
        private readonly int _lightIntensity;
        private readonly GameObject _go;
        private readonly TileBase[] _tileVariants;
        private readonly Sprite[] _animationFrames;
        private int _currentSegmentIndex = 0;
        private int _progressOnSegment = 0;
        private bool _goingForward = true;
        private int _frameIndex = 0;
        
        public LightBug(GameObject go,GridManager grid, List<Vector2Int> path, int speed, int lightIntensity, LightBugLevelSO so,Vector3Int startingCell)
        {
            _go = go;
            GridManager = grid;
            _path = path;
            _speed = speed;
            _lightIntensity = lightIntensity;
            _tileVariants = so.tileVariants;
            _animationFrames = so.animationFrames;
            CellPos = startingCell;
            
            LightMap = grid.TileMaps.light;
            SpriteRenderer = _go.GetComponent<SpriteRenderer>();

            UpdateState();
        }

        public void OnStepTaken()
        {
            Move();
            UpdateState();
        }

        private void Animation()
        {
            if (_animationFrames == null || _animationFrames.Length == 0 || SpriteRenderer == null) return;

            _frameIndex = (_frameIndex + 1) % _animationFrames.Length;
            SpriteRenderer.sprite = _animationFrames[_frameIndex];
        }

        private void Move()
        {
            //It uses the Path list past to it by the constructor and goes through it.
            //The path is a list of vectors, so whenever it enters the first vector, lets imagine its (3,0,0) for each step taken it should move 1*speed
            //towards that direction, lets say speed = 2. In the first step taken it move 2 units up in the X axis. In the next step taken, it moves 1
            //in the X axis and then checks for the next vector on the list, lets say its (2,2,0) it moves 1 more on the X axis, after another step
            //it should move 1 on the X axis and 1 on the Y axis. So it first allways goes through the X axis and then checks the Y axix.
            //when there are no more vectors on the list, it goes back the way it came from, like a patrol.
            //Before moving, it should check GridManager.CanEnemyMove(currentPos,destination)
            if (_path == null || _path.Count == 0) return;

            Vector2Int currentTarget = _path[_currentSegmentIndex];
            Vector3 currentPos = _go.transform.position;
            Vector3 targetPos = (Vector3Int)currentTarget;

            Vector3 direction = targetPos - currentPos;

            Vector3Int step = Vector3Int.zero;
            int stepsRemaining = _speed;

            while (stepsRemaining > 0)
            {
                if (currentTarget.x != 0)
                {
                    int moveX = Mathf.Clamp(currentTarget.x, -1, 1);
                    step += new Vector3Int(moveX, 0, 0);
                    currentTarget.x -= moveX;
                }
                else if (currentTarget.y != 0)
                {
                    int moveY = Mathf.Clamp(currentTarget.y, -1, 1);
                    step += new Vector3Int(0, moveY, 0);
                    currentTarget.y -= moveY;
                }
                else
                {
                    break;
                }

                stepsRemaining--;
            }

            Vector3Int currentCell = GridManager.PositionInGrid(_go.transform.position);
            Vector3Int nextCell = currentCell + step;

            if (GridManager.CanEnemyMove(nextCell))
            {
                _go.transform.position = GridManager.grid.CellToWorld(nextCell) + new Vector3(0.5f, 0.5f); // center of cell
                CellPos = nextCell;
            }

            if (direction == Vector3.zero)
            {
                // Move to next segment
                if (_goingForward)
                {
                    _currentSegmentIndex++;
                    if (_currentSegmentIndex >= _path.Count)
                    {
                        _currentSegmentIndex = _path.Count - 1;
                        _goingForward = false;
                    }
                }
                else
                {
                    _currentSegmentIndex--;
                    if (_currentSegmentIndex < 0)
                    {
                        _currentSegmentIndex = 0;
                        _goingForward = true;
                    }
                }
            }
        }
        

        private void UpdateState()
        {
            // Clear previous light
            foreach (var pos in GridManager.GetCellsInRadius(CellPos, _lightIntensity))
            {
                if (GridManager.CanPlaceShadow(pos))
                    LightMap.SetTile(pos, null);
            }

            // Place new light
            foreach (var pos in GridManager.GetCellsInRadius(CellPos, _lightIntensity))
            {
                if (GridManager.CanPlaceShadow(pos))
                {
                    TileBase tile = _tileVariants[Random.Range(0, _tileVariants.Length)];
                    LightMap.SetTile(pos, tile);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            //Acts like an Update
            Animation();
        }
    }
}
