using System.Collections.Generic;

namespace claims.src.database
{
    public class QuerryInfo
    {
        public string targetTable;
        public QuerryType action;
        public Dictionary<string, object> parameters;

        public QuerryInfo(string targetTable, QuerryType toDelete, Dictionary<string, object> parameters)
        {
            this.targetTable = targetTable;
            this.action = toDelete;
            this.parameters = parameters;
        }
    }
}
