using System.Collections.Generic;
using BehaviorTree.Base;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviorTree
{
    [RequireComponent(typeof(Animator))]
    public class CardSoldierAI : NetworkBehaviour
    {
        [Header("Range")] 
        [SerializeField] private float detectRange = 10.0f;
        [SerializeField] private float meleeAttackRange = 3.0f;
        
        [Header("Movement")] 
        [SerializeField] private float movementSpeed = 5.0f;

        #region Property

        private Vector3 _originPos = default;
        private BehaviorTreeRunner _BTRunner;
        private Transform _detectedPlayer = null;
        private Animator _animator = null;

        #endregion
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _BTRunner = new BehaviorTreeRunner(SettingBT());
            _originPos = transform.position;
        }

        INode SettingBT()
        {
            return new SelectorNode(
                new List<INode>()
                {
                }
            );
        }

        #region Patrol

        

        #endregion
        
        #region Attack

        

        #endregion
    }
}