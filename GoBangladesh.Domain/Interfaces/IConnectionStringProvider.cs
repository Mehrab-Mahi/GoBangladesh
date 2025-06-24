using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Domain.Interfaces
{
    public interface IConnectionStringProvider
    {
        string GetConnectionString();
    }
}
