/**
*	WEART - Abduction Value
*	https://www.weart.it/
*/

using System;

namespace WeArt.Core
{
    /// <summary>
    /// The actuation point abduction amount.
    /// It is minimum when the actuation point is far from the hand, maximum when close.
    /// </summary>
    [Serializable]
    public struct Abduction
    {
        /// <summary>
        /// The default abduction is zero
        /// </summary>
        public static Abduction Default = new Abduction
        {
            Value = WeArtConstants.defaultAbduction
        };


        internal float _value;

        /// <summary>
        /// The abduction amount, normalized between 0 (max) and 1 (max)
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = Math.Max(WeArtConstants.minAbduction, Math.Min(value, WeArtConstants.maxAbduction));
        }
    }
}