using System.Linq;
using System;
using WeArt.Utils;

namespace WeArt.Core
{
    /// <summary>
    /// The texture applied as haptic feeling on the actuation point.
    /// It contains an index identifying a specific texture and a 3D velocity vector.
    /// </summary>
    [Serializable]
    public struct Texture : ICloneable
    {
        /// <summary>
        /// The default texture is the first one, with no velocity
        /// </summary>
        public static Texture Default = new Texture
        {
            TextureType = (TextureType)WeArtConstants.defaultTextureIndex,
            VelocityX = WeArtConstants.defaultTextureVelocity_X,
            VelocityY = WeArtConstants.defaultTextureVelocity_Y,
            VelocityZ = WeArtConstants.defaultTextureVelocity_Z,
            Volume = WeArtConstants.defaultVolumeTexture,
            Active = false
        };


        internal TextureType _textureType;

        internal bool _active;

        private float _vx, _vy, _vz;

        private float _volume;


        /// <summary>
        /// The texture type
        /// </summary>
        public TextureType TextureType
        {
            get => _textureType;
            set {
                if((int)value > WeArtConstants.maxTextureIndex || (int)value < WeArtConstants.minTextureIndex)
                {
                    _textureType = (TextureType)WeArtConstants.nullTextureIndex;
                }
                else
                {
                    _textureType = (TextureType)value;
                }
            }
        }

        /// <summary>
        /// The horizontal component of the 3D velocity, normalized between 0 (min velocity) and 1 (max velocity)
        /// </summary>
        public float VelocityX
        {
            get => _vx;
            set => _vx = Math.Clamp(value, WeArtConstants.minTextureVelocity, WeArtConstants.maxTextureVelocity);
        }

        /// <summary>
        /// The vertical component of the 3D velocity, normalized between 0 (min velocity) and 1 (max velocity)
        /// </summary>
        public float VelocityY
        {
            get => _vy;
            set => _vy = Math.Clamp(value, WeArtConstants.minTextureVelocity, WeArtConstants.maxTextureVelocity);
        }

        /// <summary>
        /// The forward component of the 3D velocity, normalized between 0 (min velocity) and 1 (max velocity)
        /// </summary>
        public float VelocityZ
        {
            get => _vz;
            set => _vz = Math.Clamp(value, WeArtConstants.minTextureVelocity, WeArtConstants.maxTextureVelocity);
        }

        public float Volume
        {
            get => _volume;
            set => _volume = value;
        }

        /// <summary>
        /// Indicates whether the texture feeling is applied or not
        /// </summary>
        public bool Active
        {
            get => _active;
            set => _active = value;
        }


        /// <summary>
        /// True if the object is a <see cref="Texture"/> instance with the same activation status, index and velocity
        /// </summary>
        /// <param name="obj">The object to check equality with</param>
        /// <returns>The equality check result</returns>
        public override bool Equals(object obj)
        {
            return obj is Texture texture &&
                TextureType == texture.TextureType &&
                ApproximateFloatComparer.Instance.Equals(VelocityX, texture.VelocityX) &&
                ApproximateFloatComparer.Instance.Equals(VelocityY, texture.VelocityY) &&
                ApproximateFloatComparer.Instance.Equals(VelocityZ, texture.VelocityZ) &&
                Volume == texture.Volume &&
                Active == texture.Active;
        }

        /// <summary>Basic <see cref="GetHashCode"/> implementation</summary>
        /// <returns>The hashcode of this object</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>Clones this object</summary>
        /// <returns>A clone of this object</returns>
        public object Clone()
        {
            return this;
        }
    }
}