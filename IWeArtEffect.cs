/**
*	WEART - Effect interface
*	https://www.weart.it/
*/

using System;

namespace WeArt.Core
{
    /// <summary>
    /// An interface for objects that combines temperature, force and texture haptic feelings.
    /// </summary>
    public interface IWeArtEffect
    {
        /// <summary>
        /// The temperature applied by this haptic effect
        /// </summary>
        Temperature Temperature { get; }

        /// <summary>
        /// The force applied by this haptic effect
        /// </summary>
        Force Force { get; }

        /// <summary>
        /// The texture applied by this haptic effect
        /// </summary>
        Texture Texture { get; }

        /// <summary>
        /// This event should be called whenever any of the haptic values changes over time
        /// </summary>
        event Action OnUpdate;
    }
}