/**
*	WEART - Temperature component 
*	https://www.weart.it/
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace WeArt.Core
{
    /// <summary>
    /// The temperature applied to the thermal actuator on the actuation point.
    /// The minimum value indicates the coldest temperature, the maximum indicates the hottest.
    /// </summary>
    [Serializable]
    public struct Temperature : ICloneable
    {
        /// <summary>
        /// The default temperature is the ambient one, with the actuator off
        /// </summary>
        public static Temperature Default = new Temperature
        {
            Value = WeArtConstants.defaultTemperature,
            Active = false
        };


        internal float _value;
        
        internal bool _active;


        /// <summary>
        /// The temperature value, normalized between 0 (cold) and 1 (hot)
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = Math.Clamp(value, WeArtConstants.minTemperature, WeArtConstants.maxTemperature);
        }

        /// <summary>
        /// Indicates whether the temperature feeling is applied or not
        /// </summary>
        public bool Active
        {
            get => _active;
            set => _active = value;
        }


        /// <summary>
        /// True if the object is a <see cref="Temperature"/> instance with the same activation status and value
        /// </summary>
        /// <param name="obj">The object to check equality with</param>
        /// <returns>The equality check result</returns>
        public override bool Equals(object obj)
        {
            return obj is Temperature temperature &&
                   ApproximateFloatComparer.Instance.Equals(Value, temperature.Value) &&
                   Active == temperature.Active;
        }

        /// <summary>Basic <see cref="GetHashCode"/> implementation</summary>
        /// <returns>The hashcode of this object</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>Clones this object</summary>
        /// <returns>A clone of this object</returns>
        public object Clone() => this;


        /// <summary>
        /// Calculates the mean of multiple temperatures
        /// </summary>
        /// <param name="temperatures">A collection or set of temperatures</param>
        /// <returns>The mean temperature</returns>
        public static Temperature Mean(IEnumerable<Temperature> temperatures)
        {
            var actives = temperatures.Where(t => t.Active);
            var total = actives.Sum(t => t.Value);

            return new Temperature
            {
                Active = actives.Count() > 0,
                Value = total / actives.Count()
            };
        }
    }
}