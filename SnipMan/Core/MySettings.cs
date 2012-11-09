namespace SnipMan.Core
{
    /// <summary>
    /// Settings Defined in the INI file
    /// </summary>
    interface IMySettings
    {
        bool AlwaysOnTop {get; set;}
        float DefaultFontSize {get; set;}
        bool ConfirmDelete {get; set;}
        string DefaultFont {get; set;}
        bool VerticalOrient { get; set; }
        int SplitterDistance { get; set; }
        bool ShowMenu { get; set; }
        bool AutoSave { get; set; }
        bool MinimizeTray { get; set; }
        void SaveSettings();
    }
}
