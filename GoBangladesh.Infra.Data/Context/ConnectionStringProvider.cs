using GoBangladesh.Domain.Enums;
using GoBangladesh.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Infra.Data.Context
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _configuration;

        public ConnectionStringProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString(Enum.GetName(typeof(DbConnection), DbConnection.GoBangladeshConnection_Local));
        }

    }
}
