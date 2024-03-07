using System;
using UnityEngine;

namespace Util
{
    public interface IInteract
    {
        public bool IsInteract { get; set; }
        public Action<GameObject> Action { get; set; }
    }
}