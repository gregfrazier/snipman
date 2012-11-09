using System.Collections.Generic;

namespace SnipMan.Methods.Sync
{
    class Ftp : ISync
    {
        private Dictionary<string, string> _settingDict;
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Directory { get; set; }

        public Ftp()
        {
            _settingDict = new Dictionary<string, string>();
            GetSettings();
        }

        public override string ToString()
        {
            return "ftp";
            //return base.ToString();
        }

        public void SaveSettings()
        {
            OptionsDb.SaveSetting(new KeyValuePair<string, string>("ftp_host", Host));
            OptionsDb.SaveSetting(new KeyValuePair<string, string>("ftp_port", Port));
            OptionsDb.SaveSetting(new KeyValuePair<string, string>("ftp_user", Username));
            OptionsDb.SaveSetting(new KeyValuePair<string, string>("ftp_pwd", Password));
            OptionsDb.SaveSetting(new KeyValuePair<string, string>("ftp_dir", Directory));
            return;
        }

        public void GetSettings()
        {
            Host = OptionsDb.GetSettingStr("ftp_host");
            Port = OptionsDb.GetSettingStr("ftp_port");
            Username = OptionsDb.GetSettingStr("ftp_user");
            Password = OptionsDb.GetSettingStr("ftp_pwd");
            Directory = OptionsDb.GetSettingStr("ftp_dir");
            return;
        }

        private void PopSettingDict()
        {
            _settingDict.Add("ftp_host", Host);
            _settingDict.Add("ftp_port", Port);
            _settingDict.Add("ftp_user", Username);
            _settingDict.Add("ftp_pwd", Password);
            _settingDict.Add("ftp_dir", Directory);
            return;
        }


        public Dictionary<string, string> Settings()
        {
            if (_settingDict.Count < 1)
                PopSettingDict();
            return _settingDict;
        }


        public void WriteSettings(Dictionary<string, string> sets)
        {
            if (sets.ContainsKey("ftp_host"))
                Host = sets["ftp_host"];
            if (sets.ContainsKey("ftp_port"))
                Port = sets["ftp_port"];
            if (sets.ContainsKey("ftp_user"))
                Username = sets["ftp_user"];
            if (sets.ContainsKey("ftp_pwd"))
                Password = sets["ftp_pwd"];
            if (sets.ContainsKey("ftp_dir"))
                Directory = sets["ftp_dir"];
            return;
        }
    }
}
