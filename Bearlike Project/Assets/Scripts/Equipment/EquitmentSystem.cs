using Unity.VisualScripting;
using UnityEngine;

namespace Inho.Scripts.Equipment
{
    public class EquitmentSystem
    {
        private Equitment mEquitment;

        public void Init()
        {
            mEquitment = new Magnum();
            mEquitment.Init();
        }

        public Equitment GetEquitment() { return mEquitment; }
    }
}