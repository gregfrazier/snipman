namespace SnipMan.JavaScript.Interfaces
{
    public interface IDbObject
    {
        string ExecuteScalar(string sql);
        IDataListObj ExecuteQuery(string sql);
    }
}
