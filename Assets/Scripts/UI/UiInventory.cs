using System;
using System.Collections;
using GemScripts;
using Potions;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace UI
{
    [Serializable]
    public class UiInventory 
    {
        private GemManager _gemManager;
        [SerializeField]private Slot[] crating = new Slot[2];
        [SerializeField]private Slot[] stored = new Slot[4];
        [SerializeField]private Slot[] keys = new Slot[2];
        [SerializeField]private Slot potion;
        
        public void Start(MonoBehaviour mono)
        {
            mono.StartCoroutine(Delay());
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
            for (var i = 0; i < crating.Length; i++)
            {
                var slot = crating[i];
                slot.Setup();
                slot.button.onClick.AddListener(()=>RemoveFromCraft(slot));
            }

            for (var i = 0; i < stored.Length; i++)
            {
                var slot = stored[i];
                slot.Setup();
                slot.button.onClick.AddListener(() => ToCrafting(slot));
            }

            foreach (var slot in keys) slot.Setup();
            potion.Setup();
            potion.button.onClick.AddListener(UsePotion);
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
        
        public void RemoveFromCraft(Slot slot) => _gemManager.PlayerInventory.RemoveFromCraft(slot.SlotNumbter);
        public void ToCrafting(Slot slot) => _gemManager.PlayerInventory.ToCrafting(slot.SlotNumbter);
        public void UsePotion() => _gemManager.PlayerInventory.ThrowPotion();
    }

    [Serializable]
    public class Slot
    {
        public GameObject gameObject;
        [HideInInspector]public Image sptite;
        [HideInInspector]public GameObject sptiteGameObject;
        [HideInInspector]public Button button;
        public int SlotNumbter;

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
