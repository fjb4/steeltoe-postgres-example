using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Pivotal.SteeltoeProgresExample.Models;

namespace Pivotal.SteeltoeProgresExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly NpgsqlConnection _dbConnection;

        public ValuesController([FromServices] NpgsqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InformationSchemaTable>>> Get()
        {
            IEnumerable<InformationSchemaTable> tables = null;

            _dbConnection.Open();

            try
            {
                // interact with the database to prove we're able to connect
                tables = await _dbConnection.QueryAsync<InformationSchemaTable>(
                    "SELECT * FROM INFORMATION_SCHEMA.TABLES");
            }
            finally
            {
                _dbConnection.Close();
            }

            return tables?.ToList();
        }
    }
}