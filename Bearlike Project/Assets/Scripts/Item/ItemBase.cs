using Scripts.State.GameStatus;
using UnityEngine;
using UnityEngine.UI;

namespace Item
{
    public class ItemBase : MonoBehaviour
    {
        public Sprite icon;
        
        public StatusValue<int> amount;

        public virtual void GetItem<T>(T target)
        {
            
        }
    }
}

