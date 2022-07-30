using System.IO;
using UnityEngine;

namespace slocLoader.Objects {

    public class LightObject : slocGameObject {

        public LightObject(int instanceId) : base(instanceId) => Type = ObjectType.Light;

        public Color LightColor = Color.white;

        public bool Shadows = true;

        public float Range = 5;

        public float Intensity = 1;

        public override void WriteTo(BinaryWriter writer) {
            base.WriteTo(writer);
            writer.Write(LightColor.r);
            writer.Write(LightColor.g);
            writer.Write(LightColor.b);
            writer.Write(LightColor.a);
            writer.Write(Shadows);
            writer.Write(Range);
            writer.Write(Intensity);
        }

    }

}
