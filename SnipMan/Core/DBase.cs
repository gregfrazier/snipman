using SnipMan.Methods;

namespace SnipMan.Core
{
    /// <summary>
    /// Database Singleton
    /// </summary>
    public sealed class DBase
    {        
        private readonly string _dbConn;

        public static DBase Instance { get; } = new DBase("data.s3db");

        private DBase(string db){
            // Get Database Connection from Registry
            _dbConn = "Data Source=" + db;
            DbFilename = db;
            // Create DataProvider
            Db = new SqLiteProvider(_dbConn);
        }

        public IDataProvider Db { get; }

        public string DbFilename { get; }
    }
}
