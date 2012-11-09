namespace SnipMan.JavaScript.Interfaces
{
    /// <summary>
    /// DataList Object for JavaScript
    /// </summary>
    /// <remarks>
    /// Do not give access to this as an object, give access to an implementation as a type.
    /// </remarks>
    public interface IDataListObj
    {
        string[] Row(int yIndex);
        string ColumnName(int xIndex);
        int Count();
    }
}
