using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeArt.Core;

namespace WeArt.Components
{
    /// <summary>
    /// Internal class used to create the haptic effet on collision.
    /// </summary>
    class TouchEffect : IWeArtEffect
    {
        /// <summary>
        /// Defines the OnUpdate.
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// Gets the Temperature.
        /// </summary>
        public Temperature Temperature { get; private set; } = Temperature.Default;

        /// <summary>
        /// Gets the Force.
        /// </summary>
        public Force Force { get; private set; } = Force.Default;

        /// <summary>
        /// Gets the Texture.
        /// </summary>
        public Texture Texture { get; private set; } = Texture.Default;

        /// <summary>
        /// The Set.
        /// </summary>
        /// <param name="temperature">The temperature<see cref="Temperature"/>.</param>
        /// <param name="force">The force<see cref="Force"/>.</param>
        /// <param name="texture">The texture<see cref="Texture"/>.</param>
        public void Set(Temperature temperature, Force force, Texture texture)
        {
            // Need to clone these, or the internal arrays will point to the same data
            force = (Force)force.Clone();
            texture = (Texture)texture.Clone();


            bool changed = false;

            // Temperature
            changed |= !Temperature.Equals(temperature);
            Temperature = temperature;

            // Force
            changed |= !Force.Equals(force);
            Force = force;

            // Texture
            texture.Velocity = 0.5f;

            changed |= !Texture.Equals(texture);
            Texture = texture;

            if (changed)
                OnUpdate?.Invoke();
        }

    }
}