using System;
using Fusion;
using Script.GameStatus;
using Script.Manager;
using Script.Photon;
using Script.Weapon.Gun;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Player
{
    public class PlayerController : NetworkBehaviour
    {
        public IEquipment equipment;
        public StatusValue ammo = new StatusValue();

        private void Start()
        {
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            equipment = GetComponentInChildren<IEquipment>();  
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData data))
            {       
                MouseRotate(data.MouseAxis);
                MoveControl(data);

                if (data.Attack)
                {
                    equipment.AttackAction?.Invoke();
                }

                if (data.ReLoad && equipment.IsGun)
                {
                    var gun = equipment as GunBase;
                    gun.ammo = ammo;
                    gun.ReLoadBullet();
                }
            }
        }

        private void MoveControl(PlayerInputData data)
        {
            if (data.MoveFront)
                transform.position += transform.forward * Runner.DeltaTime;
            if (data.MoveBack)
                transform.position += -transform.forward * Runner.DeltaTime;
            if (data.MoveLeft)
                transform.position += -transform.right * Runner.DeltaTime;
            if (data.MoveRight)
                transform.position += transform.right * Runner.DeltaTime;
        }
        
        public float rotateSpeed = 500.0f;
        float xRotate, yRotate, xRotateMove, yRotateMove;
        public void MouseRotate(Vector2 mouseAxis)
        {
            xRotateMove = mouseAxis.y * Runner.DeltaTime * rotateSpeed;
            yRotateMove = mouseAxis.x * Runner.DeltaTime * rotateSpeed;

            yRotate = transform.eulerAngles.y + yRotateMove;
            xRotate = xRotate + xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
            var angle = new Vector3(xRotate, yRotate, 0);

            transform.eulerAngles = angle;
        }
    }
}