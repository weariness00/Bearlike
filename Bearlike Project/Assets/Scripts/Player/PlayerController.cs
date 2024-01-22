using Fusion;
using Fusion.Addons.SimpleKCC;
using Script.Manager;
using Script.Photon;
using Script.Util;
using Script.Weapon.Gun;
using Scripts.State.GameStatus;
using UnityEngine;

namespace Script.Player
{
    public class PlayerController : NetworkBehaviour
    {
        public Status status;

        public IEquipment equipment;
        public StatusValue<int> ammo = new StatusValue<int>();

        private SimpleKCC _simpleKCC;
        
        private void Awake()
        {
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            equipment = GetComponentInChildren<IEquipment>();
            status = ObjectUtil.GetORAddComponet<Status>(gameObject);
        }
        
        public override void Spawned()
        {
            _simpleKCC = ObjectUtil.GetORAddComponet<SimpleKCC>(gameObject);
            if (HasInputAuthority)
            {
                name = "Local Player";

                Runner.SetPlayerObject(Runner.LocalPlayer, Object);
                
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
                // 임시 처방 SetPlayerObject가 안되는 이유 알아내야함
                if (Runner.GetPlayerObject(Runner.LocalPlayer) == null)
                    GetComponent<PlayerCameraController>().SetPlayerCamera(Object);
            }
            else
                name = "Remote Player";
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData data))
            {       
                MouseRotate(data.MouseAxis);
                MoveControl(data);

                if (data.Attack && equipment != null)
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
            Vector3 dir = Vector3.zero;
            if (data.MoveFront)
                dir += transform.forward;
            if (data.MoveBack)
                dir += -transform.forward;
            if (data.MoveLeft)
                dir += -transform.right;
            if (data.MoveRight)
                dir += transform.right;

            dir *= Runner.DeltaTime * 100f;
            _simpleKCC.Move(dir);
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

            _simpleKCC.SetLookRotation(angle);
        }
    }
}