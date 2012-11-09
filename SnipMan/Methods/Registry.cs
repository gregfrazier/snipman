using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Globalization;
using SnipMan.Core;

namespace SnipMan.Methods
{
    class MyRegistry : IMySettings
    {
        private const string SubKeyLocale = "SOFTWARE\\SNIPMAN";
        private string _myFont;
        private bool _myOnTop;
        private float _myFontSize;
        private bool _myConfirmDelete;

        public MyRegistry(){
            try{
                RegistryKey snipKey = Registry.LocalMachine.OpenSubKey(SubKeyLocale, true);
                if(snipKey == null){
                    _myFont = "Courier New";
                    snipKey = Registry.LocalMachine.CreateSubKey(SubKeyLocale);
                    snipKey?.SetValue("DefaultFont", _myFont, RegistryValueKind.String);
                }
                if (snipKey != null)
                {
                    _myFont = (string) snipKey.GetValue("DefaultFont", _myFont);
                    _myOnTop = ((string) (snipKey.GetValue("AlwaysOnTop", "false")) == "true" ? true : false);
                    _myFontSize = float.Parse(((string) snipKey.GetValue("FontSize", "9.0")));
                    _myConfirmDelete = ((string) snipKey.GetValue("ConfirmDelete", "true") == "true" ? true : false);
                    snipKey.Close();
                }
            }catch (System.Security.SecurityException y){
                MessageBox.Show(y.Message + "\n\nRun in Administrator mode or select .INI in the Settings Menu");
                _myFontSize = 9.0f;
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message + "\n\nRun in Administrator mode or select .INI in the Settings Menu");
                _myFontSize = 9.0f;
            }
            catch (Exception c)
            {
                MessageBox.Show(c.Message);
                _myFontSize = 9.0f;
            }
        }

        public void SaveSettings()
        {
            // No need to implement, since registry settings save automatically
            return;
        }

        public bool AlwaysOnTop{
            get{ return _myOnTop; }
            set{
                _myOnTop = value;
                RegistryKey snipKey = Registry.LocalMachine.OpenSubKey(SubKeyLocale, true);
                if(snipKey == null){ return; }
                snipKey.SetValue("AlwaysOnTop", (value ? "true" : "false"));                
                snipKey.Close();
            }
        }

        public float DefaultFontSize{
            get{ return _myFontSize; }
            set{
                _myFontSize = value;
                RegistryKey snipKey = Registry.LocalMachine.OpenSubKey(SubKeyLocale, true);
                if(snipKey == null){ return; }
                snipKey.SetValue("FontSize", value.ToString(CultureInfo.InvariantCulture));                
                snipKey.Close();
            }
        }

        public bool ConfirmDelete{
            get{ return _myConfirmDelete; }
            set{
                _myConfirmDelete = value;
                RegistryKey snipKey = Registry.LocalMachine.OpenSubKey(SubKeyLocale, true);
                if(snipKey == null){ return; }
                snipKey.SetValue("ConfirmDelete", value.ToString());                
                snipKey.Close();
            }
        }

        public string DefaultFont{
            get { return _myFont; }
            set { 
                _myFont = value.ToString();
                RegistryKey snipKey = Registry.LocalMachine.OpenSubKey(SubKeyLocale, true);
                if(snipKey == null){ return; }
                snipKey.SetValue("DefaultFont", value.ToString());                
                snipKey.Close();
            }        
        }


        public bool VerticalOrient
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public int SplitterDistance
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool ShowMenu
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AutoSave
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool MinimizeTray
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
