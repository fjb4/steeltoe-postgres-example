using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            IEnumerable<InformationSchemaTable> tables;

            using (var conn = CreatePostgresConnection())
            {
                // interact with the database to prove we're able to connect
                tables = await conn.QueryAsync<InformationSchemaTable>("SELECT * FROM INFORMATION_SCHEMA.TABLES");
            }

            return tables?.ToList();
        }

        private static NpgsqlConnection CreatePostgresConnection()
        {
            var vcapServicesEnv = Environment.GetEnvironmentVariable("VCAP_SERVICES");
            var vcapServicesJson = JObject.Parse(vcapServicesEnv);

            // this assumes the Postgres service is the first one in VCAP_SERVICES...
            var credentialsJson = vcapServicesJson.SelectToken("$..credentials");

            var connString = BuildPostgresConnectionString(credentialsJson);
            Console.WriteLine($"Postgres Connection String: {connString}");

            var conn = new NpgsqlConnection(connString);

            conn.ProvideClientCertificatesCallback += certs =>
            {
                var certPem = (string) credentialsJson.SelectToken("ClientCert");

                var certBytes = ConvertPemToBytes(certPem, "CERTIFICATE");
                certs.Add(new X509Certificate2(certBytes));
            };

            return conn;
        }

        private static string BuildPostgresConnectionString(JToken credentialsJson)
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = (string) credentialsJson.SelectToken("host"),
                Database = (string) credentialsJson.SelectToken("database_name"),
                Username = (string) credentialsJson.SelectToken("Username"),
                //Password = (string) credentialsJson.SelectToken("Password"),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return connStringBuilder.ConnectionString;
        }

        private static byte[] ConvertPemToBytes(string certData, string type)
        {
            var header = $"-----BEGIN {type}-----";
            var footer = $"-----END {type}-----";

            var start = certData.IndexOf(header, StringComparison.Ordinal) + header.Length;
            var end = certData.IndexOf(footer, start, StringComparison.Ordinal);
            var base64 = certData.Substring(start, end - start);

            return Convert.FromBase64String(base64);
        }
    }
}