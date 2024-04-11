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
                pc.skillSelectUI.SpawnSkillBlocks(_skillBlockSpawnCount);
                Destroy(gameObject);   
            }
        }

        #region Interact Interface
        
        public void InteractInit()
        {
            IsInteract = true;
            InteractEnterAction += GetItem;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
        
        #endregion

        #region Json Data Interface

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
            // _skillBlockSpawnCount = json.GetInt("Skill Block Spawn Count");
        }

        #endregion
    }
}

