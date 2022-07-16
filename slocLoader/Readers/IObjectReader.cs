using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public interface IObjectReader {

        slocGameObject Read(BinaryReader stream);

    }

}
