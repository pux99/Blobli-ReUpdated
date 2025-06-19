using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Enemies.Fungi
{
    public class Fungi : Enemy
    {
        //Sprite variables
        private readonly TileBase _lightTile;
        private readonly Sprite _offSprite;
        private readonly Sprite _onSprite;
    
        //Timing variables
        private readonly int _onToOff;
        private readonly int _offToOn;
        private bool _isOn = false;
        private int _stepsSinceLastChange = 0;
    
        public Fungi(GameObject fungiGo,GridManager gridManager,TileBase tile, Sprite offSprite, Sprite onSprite, int onToOff, int offToOn)
        {
            SpriteRenderer = fungiGo.GetComponent<SpriteRenderer>();
            _lightTile = tile;
            _offSprite = offSprite;
            _onSprite = onSprite;
            _onToOff = onToOff;
            _offToOn = offToOn;

            GridManager = gridManager;
            CellPos = gridManager.PositionInGrid(fungiGo.transform.position); 
        
            LightMap = gridManager.TileMaps.light;

            UpdateState();
        }
    
        public void OnStepTaken()
        {
            _stepsSinceLastChange++;

            switch (_isOn)
            {
                case false when _stepsSinceLastChange >= _offToOn:
                    _isOn = true;
                    _stepsSinceLastChange = 0;
                    UpdateState();
                    break;
                case true when _stepsSinceLastChange >= _onToOff:
                    _isOn = false;
                    _stepsSinceLastChange = 0;
                    UpdateState();
                    break;
            }
        }
        private void UpdateState()
        {
            if (!GridManager.IsShadowTile(CellPos))
            {
                SpriteRenderer.sprite = _offSprite;
                return;
            }

            if (_isOn)
            {
                SpriteRenderer.sprite = _onSprite;
                LightMap.SetTile(CellPos, _lightTile);
            }
            else
            {
                SpriteRenderer.sprite = _offSprite;
                LightMap.SetTile(CellPos, null);
            }
        }
    }
}
