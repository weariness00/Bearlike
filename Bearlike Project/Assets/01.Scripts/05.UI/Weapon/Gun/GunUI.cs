using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Weapon;
using Weapon.Gun;
using DebugManager = Manager.DebugManager;

namespace UI.Weapon.Gun
{
    public class GunUI : MonoBehaviour
    {
        [SerializeField]private PlayerController playerController;
        [SerializeField]private Animation ShotAnimation;
        
        public TMP_Text bulletCount;
        public TMP_Text ammoCount;

        private WeaponSystem _weaponSystem;
        
        private int _magazineCount;
        private int _ammoCount;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            _weaponSystem = playerController.weaponSystem;
            SetUI();
        }
        
        private void Update()
        {
            DebugManager.ToDo("Gun만다루는 UI가 아닌 Weapon 전체를 다루는 UI로 꾸바기");
            
            DebugManager.Log($"{bulletCount.rectTransform.position}");
            GunUpdate();
        }

        private void SetUI()
        {
            if (_weaponSystem.equipment.IsGun)
            {
                var gun = _weaponSystem.equipment as GunBase;
                
                DebugManager.ToDo("ammo의 종류는 많으니 추후에 수정 필요");
                _ammoCount = GunBase.ammo.Current;
                
                DebugManager.ToDo("총의 종류에 따라 Ammo를 받아오는 방식으로 변경해야함");
                ammoCount.text = "/ "+ GunBase.ammo.Current;
            
                bulletCount.text = gun.magazine.Current.ToString();
                
                if (gun.magazine.Current >= 10)
                    bulletCount.rectTransform.position = new Vector3(1782, 230, 0);
                else
                    bulletCount.rectTransform.position = new Vector3(1820, 230, 0);
            }
        }

        private void GunUpdate()
        {
            if (_weaponSystem.equipment.IsGun)
            {
                var gun = _weaponSystem.equipment as GunBase;
                if (_magazineCount != gun.magazine.Current || _ammoCount != GunBase.ammo.Current)
                {
                    DebugManager.Log($"{ShotAnimation.clip.name}");
                    // ShotAnimation.Play("ShotAnim");
                    ShotAnimation.Play();
                    
                    bulletCount.text = gun.magazine.Current.ToString();
                    _magazineCount = gun.magazine.Current;
                    
                    ammoCount.text = "/ "+ GunBase.ammo.Current;
                    _ammoCount = GunBase.ammo.Current;
                    
                    if (gun.magazine.Current >= 10)
                        bulletCount.rectTransform.position = new Vector3(1782, 230, 0);
                    else
                        bulletCount.rectTransform.position = new Vector3(1820, 230, 0);
                }
            }
        }
    }
}
