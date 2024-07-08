using System;

namespace Status
{
    public interface IAfterApplyDamage
    {
        public Action<int> AfterApplyDamageAction { get; set; }
    }
}