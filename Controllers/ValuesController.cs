using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
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
                var cert = GetClientCertificate(credentialsJson);
                certs.Add(cert);
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
                Password = (string) credentialsJson.SelectToken("Password"),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return connStringBuilder.ConnectionString;
        }

        private static X509Certificate2 GetClientCertificate(JToken credentialsJson)
        {
            var clientCert = (string) credentialsJson.SelectToken("ClientCert");
            var certBytes = Encoding.ASCII.GetBytes(clientCert);

            var clientKey = (string) credentialsJson.SelectToken("ClientKey");
            var keyBytes = Encoding.ASCII.GetBytes(clientKey);

            var cert = GetX509FromBytes(certBytes, keyBytes);
            return cert;
        }

        private static X509Certificate2 GetX509FromBytes(byte[] clientCertificate, byte[] clientKey)
        {
            var cert = new X509Certificate2(clientCertificate);
            object obj;

            using (var reader = new StreamReader(new MemoryStream(clientKey)))
            {
                obj = new PemReader(reader).ReadObject();
                if (obj is AsymmetricCipherKeyPair cipherKey) obj = cipherKey.Private;
            }

            var rsaKeyParams = (RsaPrivateCrtKeyParameters) obj;
            var rsa = RSAUtilities.ToRSA(rsaKeyParams);

            cert = cert.CopyWithPrivateKey(rsa);

            // Following is work around for https://github.com/dotnet/corefx/issues/24454
            var buffer = cert.Export(X509ContentType.Pfx, (string) null);
            return new X509Certificate2(buffer, (string) null);
        }
    }
}