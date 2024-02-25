using System;
using System.Collections.Generic;
using BehaviorTree.Base;
using BehaviorTree.Component;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Allocator = Unity.Collections.Allocator;
using Fusion;

namespace BehaviorTree
{
    public class BTManager : MonoBehaviour
    {
        enum UnitType
        {
            PiggyBank,
        }
        
        struct UnitData
        {
            public UnitType Type;
            public PiggyBankInfo.State CurrentState;

            public INode RootNode;
        }

        struct ExcuteBTJob : IJobParallelFor
        {
            public NativeArray<UnitData> UnitDatas;
            
            public void Execute(int index)
            {
                // UnitDatas[0].
            }
        }

        private void Update()
        {
            NativeArray<UnitData> unitData = new NativeArray<UnitData>(1, Allocator.TempJob);
            
            
            ExcuteBTJob btJob = new ExcuteBTJob {
                UnitDatas = unitData
            };

            JobHandle handle = btJob.Schedule(1, 64);
            handle.Complete();

            unitData.Dispose();
        }
    }
}