using Fusion;
using UnityEngine;

namespace Inho_Test_.Physics
{
    public class TestBasicSpawner
    {
        private bool _mouseButton0;
        private bool _mouseButton1;

        private void Update()
        {
            _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
            _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new TestNetworkInputData();
            
            data.buttons.Set(TestNetworkInputData.MOUSEBUTTON0, _mouseButton0);
            _mouseButton0 = false;
            data.buttons.Set(TestNetworkInputData.MOUSEBUTTON1, _mouseButton1);
            _mouseButton1 = false;

            // input.Set(data);
        }
    }
}