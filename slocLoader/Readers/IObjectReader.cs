using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public interface IObjectReader {

        slocHeader ReadHeader(BinaryReader stream);

        slocGameObject Read(BinaryReader stream, slocHeader header);

    }

}
