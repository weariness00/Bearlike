using GamePlay.UI;
using Item;
using Manager;
using Photon;
using Skill;
using UI;
using UI.Skill;
using UI.Status;
using UI.Weapon.Gun;
using UnityEngine;
using User;

namespace Player
{
    public class PlayerUIController : NetworkBehaviourEx
    {
        private PlayerController playerController;
        
        [Header("UI")]
        public GunUI gunUI;
        public PlayerHP hpUI;
        public ItemInventory itemInventory;
        public SkillInventory skillInventory;
        public SkillSelectUI skillSelectUI;
        public SkillCanvas skillCanvas;
        public PlayerEXP levelCanvas;
        public BuffCanvas buffCanvas;
        public GoodsCanvas goodsCanvas;
        public GameProgressCanvas progressCanvas;
        public StageSelectUI stageSelectUI;

        private void Awake()
        {
            playerController = GetComponentInChildren<PlayerController>();
            
            gunUI = GetComponentInChildren<GunUI>(true);
            hpUI = GetComponentInChildren<PlayerHP>(true);
            itemInventory = GetComponentInChildren<ItemInventory>(true);
            skillInventory = GetComponentInChildren<SkillInventory>(true);
            skillSelectUI = GetComponentInChildren<SkillSelectUI>(true);
            skillCanvas = GetComponentInChildren<SkillCanvas>(true);
            levelCanvas = GetComponentInChildren<PlayerEXP>(true);
            buffCanvas = GetComponentInChildren<BuffCanvas>(true);
            goodsCanvas = GetComponentInChildren<GoodsCanvas>(true);
            progressCanvas = GetComponentInChildren<GameProgressCanvas>(true);
            stageSelectUI = FindObjectOfType<StageSelectUI>(true);

            bool activeValue = true;
            itemInventory.gameObject.SetActive(activeValue);
            skillInventory.gameObject.SetActive(activeValue);
        }

        private void Start()
        {
            playerController.status.LevelUpAction += () =>
            {
                if (HasInputAuthority)
                {
                    if (skillSelectUI.GetSelectCount() <= 0)
                        skillSelectUI.SpawnRandomSkillBlocks(3);
                    skillSelectUI.AddSelectCount();
                }
            };
        }

        public override void Spawned()
        {
            base.Spawned();
            progressCanvas = FindObjectOfType<GameProgressCanvas>();

            if (HasInputAuthority)
            {
                goodsCanvas.CottonCoinUpdate(UserInformation.Instance.cottonInfo.GetCoin());
                
                CanvasActive(true);
            }
            else
            {
                CanvasActive(false);
            }
            
            bool activeValue = false;
            itemInventory.gameObject.SetActive(activeValue);
            skillInventory.gameObject.SetActive(activeValue);
        }

        public void Update()
        {
            if (HasInputAuthority)
            {
                UISetting();
            }
        }

        private void CanvasActive(bool value)
        {
            gunUI.gameObject.SetActive(value);
            hpUI.gameObject.SetActive(value);
            levelCanvas.gameObject.SetActive(value);
            buffCanvas.gameObject.SetActive(value);
            goodsCanvas.gameObject.SetActive(value);
            skillCanvas.gameObject.SetActive(value);
        }

        private void UISetting()
        {
            void UIActive(GameObject uiObj)
            {
                var isActive = uiObj.activeSelf;
                UIManager.ActiveUIAllDisable();
                if (!isActive)
                {
                    uiObj.SetActive(true);
                    UIManager.AddActiveUI(uiObj);
                }
            }

            if (KeyManager.InputActionDown(KeyToAction.StageSelect))
                UIActive(stageSelectUI.gameObject);
            else if (KeyManager.InputActionDown(KeyToAction.ItemInventory))
                UIActive(itemInventory.canvas.gameObject);
            else if (KeyManager.InputActionDown(KeyToAction.SkillInventory))
                UIActive(skillInventory.canvas.gameObject);
            else if (KeyManager.InputActionDown(KeyToAction.SkillSelect))
                UIActive(skillSelectUI.canvas.gameObject);

            if (KeyManager.InputActionDown(KeyToAction.GameProgress)) progressCanvas.gameObject.SetActive(true);
            else if(KeyManager.InputActionUp(KeyToAction.GameProgress)) progressCanvas.gameObject.SetActive(false);
        }
    }
}

