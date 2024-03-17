using System;
using UnityEngine;

namespace Util
{
    public interface IInteract
    {
        public void InteractInit();
        
        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
    }
}