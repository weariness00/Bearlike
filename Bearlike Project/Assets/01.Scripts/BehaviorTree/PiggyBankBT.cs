using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using BehaviorTree.Component.PiggyBank;
using Fusion;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

namespace BehaviorTree
{
    public class PiggyBankBT : MonoBehaviour
    {
        private PiggyBankActionExcutorComponent _action;
        private PiggyBankInfo _info;

        private struct BTJob : IJob
        {
            // public NativeArray<>
            
            public void Execute()
            {
                
            }
        }

        private BTJob btJob;
        
        private void Awake()
        {
            _action = GetComponent<PiggyBankActionExcutorComponent>();
            _info = GetComponent<PiggyBankInfo>();
        }

        private void Start()
        {
            _info.root = SettingBT();
        }

        INode SettingBT()
        {
            return new SequenceNode(
                new List<INode>()
                {
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(_action.CheckAttackAction),
                            new ActionNode(_action.StartAttack),
                        }
                    ),
                    new ActionNode(_action.WalkAround),
                }
            );
        }
    }
}