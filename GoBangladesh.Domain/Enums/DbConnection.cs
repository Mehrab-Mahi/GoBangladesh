using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Domain.Enums
{
    public enum DbConnection
    {
        GoBangladeshConnection_Local,
        GoBangladeshConnection_Live
    }

    public enum PaymentType
    {
        One_Off,
        Bonus,
        Royality
    }
}
