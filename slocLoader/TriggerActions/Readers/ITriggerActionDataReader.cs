using System.IO;
using slocLoader.TriggerActions.Data;

namespace slocLoader.TriggerActions.Readers {

    public interface ITriggerActionDataReader {

        BaseTriggerActionData Read(BinaryReader reader);

    }

}
