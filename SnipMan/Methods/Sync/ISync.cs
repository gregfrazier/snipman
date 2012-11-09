using System.Collections.Generic;

namespace SnipMan.Methods.Sync
{
    interface ISync
    {
        void SaveSettings();
        void GetSettings();
        Dictionary<string, string> Settings();
        void WriteSettings(Dictionary<string, string> sets);
    }
}
