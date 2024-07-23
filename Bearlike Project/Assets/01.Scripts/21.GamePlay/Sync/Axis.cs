using System.ComponentModel;

namespace GamePlay.Sync
{
    public enum Axis
    {
        /// <summary>Positive X Axis (1, 0, 0)</summary>
        X,
        /// <summary>Negative X Axis (-1, 0, 0)</summary>
        [Description("-X")] X_NEG,
        /// <summary>Positive Y Axis (0, 1, 0)</summary>
        Y,  
        /// <summary>Negative Y Axis (0, -1, 0)</summary>
        [Description("-Y")] Y_NEG, 
        /// <summary>Positive Z Axis (0, 0, 1)</summary>
        Z,
        /// <summary>Negative Z Axis (0, 0, -1)</summary>
        [Description("-Z")] Z_NEG 
    }
}