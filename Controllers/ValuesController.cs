using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;
using PostgresExample.Models;

namespace PostgresExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InformationSchemaTable>>> Get()
        {
            IEnumerable<InformationSchemaTable> tables = null;

            var connString = GetPostgresConnectionString();
            Console.WriteLine($"Postgres Connection String: {connString}");

            using (var conn = new NpgsqlConnection(connString))
            {
                // interact with the database to prove we're able to connect
                tables = await conn.QueryAsync<InformationSchemaTable>("SELECT * FROM INFORMATION_SCHEMA.TABLES");
            }

            return tables?.ToList();
        }

        private static string GetPostgresConnectionString()
        {
            var vcapServicesJson = Environment.GetEnvironmentVariable("VCAP_SERVICES");

            var parsedJson = JObject.Parse(vcapServicesJson);

            // this assumes the Postgres service is the first one in VCAP_SERVICES...
            return (string) parsedJson.SelectToken("$..credentials.uri");
        }
    }
}