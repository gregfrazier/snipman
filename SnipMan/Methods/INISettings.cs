using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;
using SnipMan.Core;

namespace SnipMan.Methods
{
    class IniSettings : IMySettings
    {
        private Hashtable _hashedIni;
        private string _filePath;
        private string _defaultGroup = "[Default]";

        private string _myFont;
        
        private bool _myOnTop;        
        private bool _myConfirmDelete;
        private bool _myVertOrient;
        private bool _myShowMenu;
        private bool _myAutoSave;
        private bool _myMinimizeTray;

        private float _myFontSize;

        private int _mySplitterSize;

        public IniSettings(string file)
        {
            _filePath = file;
            // Set defaults:
            _myFont = "";
            _myOnTop = false;
            _myFontSize = 9.0f;
            _myConfirmDelete = true;
            _myVertOrient = false;
            _mySplitterSize = 125;
            _myShowMenu = true;
            _myAutoSave = true;
            _myMinimizeTray = true;
            try
            {
                if (System.IO.File.Exists(_filePath))
                {
                    Tokenize();
                    LoadSettings();
                    //SaveSettings();
                }
                else
                {
                    _hashedIni = new Hashtable {{_defaultGroup, new Hashtable()}};
                }
            }
            catch (Exception e)
            {
                new ErrorHandler(e.Message, e.StackTrace);
            }
        }

        private void Tokenize()
        {
            string regexGroupName = @"^\[[^\]]+\]$",
                regexGroupMember = @"^[^#=]+=",
                regexMemberName = @"^[^=]+",
                currGroupName = _defaultGroup;

            if (_hashedIni != null){
                _hashedIni.Clear();
            }else{
                _hashedIni = new Hashtable();
            }
            using(System.IO.StreamReader objReader = new System.IO.StreamReader(_filePath)){
                while (objReader.Peek() >= 0)
                {
                    var strLine = objReader.ReadLine();
                    if(strLine != null)
                        if (strLine != String.Empty && Regex.IsMatch(strLine.Trim(), regexGroupName))
                        {
                            currGroupName = Regex.Match(strLine.Trim(), regexGroupName).ToString();
                            _hashedIni.Add(currGroupName, new Hashtable());
                        }
                        else if (Regex.IsMatch(strLine.Trim(), regexGroupMember))
                        {
                            ((Hashtable)_hashedIni[currGroupName]).Add(Regex.Match(strLine.Trim(), regexMemberName).ToString().Trim(), Regex.Replace(strLine.Trim(), regexMemberName, "").Trim('=').Trim());
                        }
                }
                objReader.Close();
            }
            return;
        }

        private string CheckSetting(string group, string key)
        {
            return ((Hashtable)_hashedIni[group]).ContainsKey(key) == true ? ((Hashtable)_hashedIni[group])[key].ToString() : String.Empty;
        }

        private void LoadSettings()
        {
            _myOnTop = CheckSetting("[Default]", "AlwaysOnTop") == "1" ? true : false;
            _myShowMenu = CheckSetting("[Default]", "ShowMenu") == "1" ? true : false;
            _myVertOrient = CheckSetting("[Default]", "VerticalOrient") == "1" ? true : false;
            _myConfirmDelete = CheckSetting("[Default]", "ConfirmDelete") == "1" ? true : false;
            _myFont = CheckSetting("[Default]", "DefaultFont");
            _myFontSize = float.Parse("0" + CheckSetting("[Default]", "DefaultFontSize"));
            DefaultFontSize = _myFontSize <= 0 ? 10 : _myFontSize;
            _mySplitterSize = Int32.Parse("0" + CheckSetting("[Default]", "SplitterSize"));
            _myAutoSave = CheckSetting("[Default]", "AutoSave") == "1" ? true : false;
            _myMinimizeTray = CheckSetting("[Default]", "MinimizeTray") == "1" ? true : false;
        }

        public void SaveSettings()
        {
            try
            {
                if (!System.IO.File.Exists(_filePath))
                {
                    System.IO.File.Create(_filePath).Close();
                }

                if (System.IO.File.Exists(_filePath))
                {
                    using (System.IO.StreamWriter objWriter = new System.IO.StreamWriter(_filePath))
                    {
                        foreach (DictionaryEntry grp in _hashedIni)
                        {
                            objWriter.WriteLine(grp.Key.ToString());
                            foreach (DictionaryEntry v in (Hashtable)grp.Value)
                            {
                                objWriter.WriteLine(v.Key.ToString() + "=" + v.Value.ToString());
                            }
                        }
                        objWriter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                new ErrorHandler(e.Message, e.StackTrace);
            }
        }

        public bool VerticalOrient
        {
            get
            {
                return _myVertOrient;
            }
            set
            {
                _myVertOrient = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("VerticalOrient"))
                    ((Hashtable)_hashedIni["[Default]"])["VerticalOrient"] = _myVertOrient ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("VerticalOrient", _myVertOrient ? "1" : "0");
            }
        }

        public bool ShowMenu
        {
            get
            {
                return _myShowMenu;
            }
            set
            {
                _myShowMenu = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("ShowMenu"))
                    ((Hashtable)_hashedIni["[Default]"])["ShowMenu"] = _myShowMenu ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("ShowMenu", _myShowMenu ? "1" : "0");
            }
        }

        public bool MinimizeTray
        {
            get
            {
                return _myMinimizeTray;
            }
            set
            {
                _myMinimizeTray = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("MinimizeTray"))
                    ((Hashtable)_hashedIni["[Default]"])["MinimizeTray"] = _myMinimizeTray ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("MinimizeTray", _myMinimizeTray ? "1" : "0");
            }
        }

        public bool AlwaysOnTop
        {
            get
            {
                return _myOnTop;
            }
            set
            {
                _myOnTop = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("AlwaysOnTop"))
                    ((Hashtable)_hashedIni["[Default]"])["AlwaysOnTop"] = _myOnTop ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("AlwaysOnTop", _myOnTop ? "1" : "0");
            }
        }

        public bool AutoSave
        {
            get
            {
                return _myAutoSave;
            }
            set
            {
                _myAutoSave = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("AutoSave"))
                    ((Hashtable)_hashedIni["[Default]"])["AutoSave"] = _myAutoSave ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("AutoSave", _myAutoSave ? "1" : "0");
            }
        }

        public float DefaultFontSize
        {
            get
            {
                return _myFontSize;
            }
            set
            {
                _myFontSize = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("DefaultFontSize"))
                    ((Hashtable)_hashedIni["[Default]"])["DefaultFontSize"] = _myFontSize.ToString(CultureInfo.InvariantCulture);
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("DefaultFontSize", _myFontSize.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool ConfirmDelete
        {
            get
            {
                return _myConfirmDelete;
            }
            set
            {
                _myConfirmDelete = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("ConfirmDelete"))
                    ((Hashtable)_hashedIni["[Default]"])["ConfirmDelete"] = _myConfirmDelete ? "1" : "0";
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("ConfirmDelete", _myConfirmDelete ? "1" : "0");
            }
        }

        public string DefaultFont
        {
            get
            {
                return _myFont;
            }
            set
            {
                _myFont = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("DefaultFont"))
                    ((Hashtable)_hashedIni["[Default]"])["DefaultFont"] = _myFont;
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("DefaultFont", _myFont);
            }
        }


        public int SplitterDistance
        {
            get
            {
                return _mySplitterSize;
            }
            set
            {
                _mySplitterSize = value;
                if (((Hashtable)_hashedIni["[Default]"]).ContainsKey("SplitterSize"))
                    ((Hashtable)_hashedIni["[Default]"])["SplitterSize"] = _mySplitterSize.ToString();
                else
                    ((Hashtable)_hashedIni["[Default]"]).Add("SplitterSize", _mySplitterSize.ToString());
            }
        }
    }
}
