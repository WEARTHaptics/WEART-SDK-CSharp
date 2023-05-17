/**
*	WEART - Haptic Object component 
*	https://www.weart.it/
*/

using System;
using System.Collections.Generic;
using WeArt.Core;
using WeArt.Messages;
using WeArt.Utils;
using Texture = WeArt.Core.Texture;

namespace WeArt.Components
{
    /// <summary>
    /// This component controls the haptic actuators of one or more hardware thimbles.
    /// The haptic control can be issued:
    /// 1) Manually from the Unity inspector
    /// 2) When a <see cref="WeArtTouchableObject"/> collides with this object
    /// 3) On custom haptic effects added or removed
    /// 4) On direct value set, through the public properties
    /// </summary>
    public class WeArtHapticObject
    {
        internal HandSideFlags _handSides = HandSideFlags.None;

        internal ActuationPointFlags _actuationPoints = ActuationPointFlags.None;

        internal Temperature _temperature = Temperature.Default;

        internal Force _force = Force.Default;

        internal Texture _texture = Texture.Default;

        internal IWeArtEffect _activeEffect;


        private WeArtClient _client;


        /// <summary>
        /// Called when the resultant haptic effect changes because of the influence
        /// caused by the currently active effects
        /// </summary>
        public event Action OnActiveEffectsUpdate;


        /// <summary>
        /// The hand sides to control with this component
        /// </summary>
        public HandSideFlags HandSides
        {
            get => _handSides;
            set
            {
                if (value != _handSides)
                {
                    var sidesToStop = _handSides ^ value & _handSides;
                    _handSides = sidesToStop;
                    StopControl();

                    _handSides = value;
                    StartControl();
                }
            }
        }

        /// <summary>
        /// The thimbles to control with this component
        /// </summary>
        public ActuationPointFlags ActuationPoints
        {
            get => _actuationPoints;
            set
            {
                if (value != _actuationPoints)
                {
                    var pointsToStop = _actuationPoints ^ value & _actuationPoints;
                    _actuationPoints = pointsToStop;
                    StopControl();

                    _actuationPoints = value;
                    StartControl();
                }
            }
        }

        /// <summary>
        /// The current temperature of the specified thimbles
        /// </summary>
        public Temperature Temperature
        {
            get => _temperature;
            set
            {
                if (!_temperature.Equals(value))
                {
                    _temperature = value;

                    if (value.Active)
                        SendSetTemperature();
                    else
                        SendStopTemperature();
                }
            }
        }

        /// <summary>
        /// The current pressing force of the specified thimbles
        /// </summary>
        public Force Force
        {
            get => _force;
            set
            {
                if (!_force.Equals(value))
                {
                    _force = value;

                    if (value.Active)
                        SendSetForce();
                    else
                        SendStopForce();
                }
            }
        }

        /// <summary>
        /// The current texture feeling applied on the specified thimbles
        /// </summary>
        public Texture Texture
        {
            get => _texture;
            set
            {
                if (!_texture.Equals(value))
                {
                    _texture = value;

                    if (value.Active)
                        SendSetTexture();
                    else
                        SendStopTexture();
                }
            }
        }

        /// <summary>
        /// The currently active effects on this object
        /// </summary>
        public IWeArtEffect ActiveEffect => _activeEffect;

        public WeArtHapticObject(WeArtClient client)
        {
            _client = client;
            _client.OnConnectionStatusChanged -= OnConnectionChanged;
            _client.OnConnectionStatusChanged += OnConnectionChanged;
        }


        /// <summary>
        /// Adds a haptic effect to this object. This effect will have an influence
        /// as long as it is not removed or the haptic properties are programmatically
        /// forced to have a specified value.
        /// </summary>
        /// <remarks>When called, this methods replaces the latest effect applied with the new one</remarks>
        /// <param name="effect">The haptic effect to add to this object</param>
        public void AddEffect(IWeArtEffect effect)
        {
            _activeEffect = effect;
            UpdateEffects();
            effect.OnUpdate += UpdateEffects;
        }

        /// <summary>
        /// Removes a haptic effect from the set of influencing effects
        /// </summary>
        /// <remarks>When called, this methods removes the (only) active effect, leaving the
        /// haptic object without applied effects</remarks>
        /// <param name="effect">The haptic effect to remove</param>
        public void RemoveEffect(IWeArtEffect effect)
        {
            _activeEffect = null;
            UpdateEffects();
            effect.OnUpdate -= UpdateEffects;
        }

        /// <summary>
        /// Internally updates the resultant haptic effect caused by the set of active effects.
        /// </summary>
        private void UpdateEffects()
        {
            var lastTemperature = Temperature.Default;
            if (_activeEffect != null)
            {
                lastTemperature = _activeEffect.Temperature;
            }

            Temperature = lastTemperature;

            var lastForce = Force.Default;
            if (_activeEffect != null)
            {
                lastForce = _activeEffect.Force;
            }
            Force = lastForce;

            var lastTexture = Texture.Default;
            if (_activeEffect != null)
            {
                lastTexture = _activeEffect.Texture;
            }
            Texture = lastTexture;
        }

        internal void OnConnectionChanged(bool connected)
        {
            if (connected)
                StartControl();
        }

        internal void StartControl()
        {
            if (Temperature.Active)
                SendSetTemperature();

            if (Force.Active)
                SendSetForce();

            if (Texture.Active)
                SendSetTexture();
        }

        internal void StopControl()
        {
            if (Temperature.Active)
                SendStopTemperature();

            if (Force.Active)
                SendStopForce();

            if (Texture.Active)
                SendStopTexture();
        }


        private void SendSetTemperature() => SendMessage((handSide, actuationPoint) => new SetTemperatureMessage()
        {
            Temperature = _temperature.Value,
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopTemperature() => SendMessage((handSide, actuationPoint) => new StopTemperatureMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendSetForce() => SendMessage((handSide, actuationPoint) => new SetForceMessage()
        {
            Force = new float[] { _force.Value, _force.Value, _force.Value },
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopForce() => SendMessage((handSide, actuationPoint) => new StopForceMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendSetTexture() => SendMessage((handSide, actuationPoint) => new SetTextureMessage()
        {
            TextureIndex = (int)_texture.TextureType,
            TextureVelocity = new float[] { 0.5f, 0.0f, _texture.Velocity },
            TextureVolume = _texture.Volume,
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopTexture() => SendMessage((handSide, actuationPoint) => new StopTextureMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendMessage(Func<HandSide, ActuationPoint, IWeArtMessage> createMessage)
        {
            if (_client == null)
                return;

            foreach (var handSide in WeArtConstants.HandSides)
                if (HandSides.HasFlag((HandSideFlags)(1 << (int)handSide)))
                    foreach (var actuationPoint in WeArtConstants.ActuationPoints)
                        if (ActuationPoints.HasFlag((ActuationPointFlags)(1 << (int)actuationPoint)))
                            _client.SendMessage(createMessage(handSide, actuationPoint));
        }
    }
}