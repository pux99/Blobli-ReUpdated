using System;
using System.Collections;
using GemScripts;
using Potions;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class UiInventory : MonoBehaviour
    {
        private GemManager _gemManager;
        [SerializeField]private Slot[] crating = new Slot[2];
        [SerializeField]private Slot[] stored = new Slot[4];
        [SerializeField]private Slot[] keys = new Slot[2];
        [SerializeField]private Slot potion;
        
        private void Start()
        {
            StartCoroutine(Delay());
        }

        private IEnumerator Delay()
        {
            yield return new WaitForEndOfFrame();
            _gemManager = ServiceLocator.Instance.GetService<GemManager>();
            _gemManager.PlayerInventory.UpdateCrafting += UpdateCrafting;
            _gemManager.PlayerInventory.UpdateStored += UpdateStorage;
            _gemManager.PlayerInventory.UpdatePotion += UpdatePotion;
            _gemManager.PlayerInventory.UpdateKeys += UpdateKeys;
            SetUpSlots();
        }

        private void SetUpSlots()
        {
            foreach (var slot in crating) slot.Setup();
            foreach (var slot in stored) slot.Setup();
            foreach (var slot in keys) slot.Setup();
            potion.Setup();
        }
        private void UpdatePotion( SO_ShadowShape newPotion)
        {
            if (newPotion == default)
                potion.Update(false,null);
            else
                potion.Update(true,newPotion.sprite);
        }
        private void UpdateCrafting(Gem[] gems) => UpdateSlotArray(crating, gems);
        private void UpdateStorage(Gem[] gems) => UpdateSlotArray(stored, gems);
        private void UpdateKeys(Gem[] gems) => UpdateSlotArray(keys, gems);
        private void UpdateSlotArray(Slot[] slot, Gem[] gems)
        {
            for (int i = 0; i < slot.Length; i++)
            {
                if (gems[i] == null)
                    slot[i].Update(false,null);
                else
                    slot[i].Update(true,gems[i].Renderer.sprite);
            }
        }
        
        public void RemoveFromCraft(int slot) => _gemManager.PlayerInventory.RemoveFromCraft(slot);
        public void ToCrafting(int slot) => _gemManager.PlayerInventory.ToCrafting(slot);
        public void UsePotion() => _gemManager.PlayerInventory.ThrowPotion();
    }

    [Serializable]
    internal class Slot
    {
        public GameObject gameObject;
        public Image sptite;
        public GameObject sptiteGameObject;
        public Button button;

        public void Update(bool state,Sprite art)
        {
            sptiteGameObject.SetActive(state);
            sptite.sprite = art;
        }
        public void Setup()
        {
            sptite=gameObject.transform.GetChild(0).GetComponent<Image>();
            button = gameObject.GetComponent<Button>();
            sptiteGameObject = sptite.gameObject;
            sptiteGameObject.SetActive(false);
        }
    }
}
