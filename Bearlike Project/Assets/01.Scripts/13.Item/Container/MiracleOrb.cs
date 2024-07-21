using System;
using Player;
using Status;
using UnityEngine;
using Util;

namespace Item.Container
{
    /// <summary>
    /// 통칭 : 매직 오브
    /// 캐릭터의 스킬 포인트를 올려준다
    /// </summary>
    public class MiracleOrb : ItemBase, IInteract
    {
        private int _skillBlockSpawnCount = 3;
        private OrbType _orbType;
        
        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();

            InteractInit();
        }

        #endregion
        
        /// <summary>
        /// 습득시 바로 사용되기에 기존의 Base.GetItem은 사용하지 않는다.
        /// </summary>
        /// <param name="targetObject"></param>
        public override void GetItem(GameObject targetObject)
        {
            PlayerController pc;
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                switch (_orbType)
                {
                    case OrbType.Random:
                        if(pc.skillSelectUI.GetSelectCount() <= 0)
                            pc.skillSelectUI.SpawnRandomSkillBlocks(_skillBlockSpawnCount);
                        pc.skillSelectUI.AddSelectCount();
                        break;
                    case OrbType.HasRandom:
                        pc.skillSelectUI.SpawnHasRandomSkillBlock(_skillBlockSpawnCount);
                        break;
                    case OrbType.Has:
                        break;
                }
                Destroy(gameObject);   
            }
        }

        #region Interact Interface
        
        public void InteractInit()
        {
            IsInteract = true;
            InteractKeyDownAction += GetItem;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
        public Action<GameObject> InteractKeyDownAction { get; set; }
        public Action<GameObject> InteractKeyUpAction { get; set; }
        
        #endregion

        #region Json Data Interface

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
            _skillBlockSpawnCount = json.GetInt("Skill Block Spawn Count");
            _orbType = (OrbType)json.GetInt("Orb Type");
        }

        #endregion
        
        private enum OrbType
        {
            Random, // 만렙이 아닌 스킬들 중에서 랜덤 선택
            HasRandom, // 가지고 있는 것들 중에 랜덤
            Has, // 가지고 있는 것들 중에 선택
        }
    }
}

