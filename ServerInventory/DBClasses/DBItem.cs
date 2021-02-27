using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory.DBClasses
{
    [Serializable]
    public class DBItem
    {
        [BsonId(true)]
        public int DbItemId;
        public String PlayerId;
        public String ItemName;
        public int StackCount;
        public int Quality;
        public int Variant;
    }
}
