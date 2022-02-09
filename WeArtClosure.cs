/**
*	WEART - Clousure Force
*	https://www.weart.it/
*/

using System;

namespace WeArt.Core
{
    /// <summary>
    /// The actuation point closure amount.
    /// It is minimum when the actuation point is open, maximum when closed.
    /// </summary>
    [Serializable]
    public struct Closure
    {
        /// <summary>
        /// The default closure is zero (max openness)
        /// </summary>
        public static Closure Default = new Closure
        {
            Value = WeArtConstants.defaultClosure
        };


        internal float _value;

        /// <summary>
        /// The closure amount, normalized between 0 (max openness) and 1 (max closure)
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = value;
        }
    }
}