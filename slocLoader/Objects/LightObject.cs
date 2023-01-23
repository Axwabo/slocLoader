using System.IO;
using slocLoader.Readers;
using UnityEngine;

namespace slocLoader.Objects {

    public sealed class LightObject : slocGameObject {

        public LightObject() : this(0) {
        }

        public LightObject(int instanceId) : base(instanceId) => Type = ObjectType.Light;

        public Color LightColor = Color.white;

        public bool Shadows = true;

        public float Range = 5;

        public float Intensity = 1;

        protected override void WriteData(BinaryWriter writer, slocHeader header) {
            if (header.HasAttribute(slocAttributes.LossyColors))
                writer.Write(LightColor.ToLossyColor());
            else
                writer.WriteColor(LightColor);

            writer.Write(Shadows);
            writer.Write(Range);
            writer.Write(Intensity);
        }

    }

}
