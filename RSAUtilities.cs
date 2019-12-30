using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace PostgresExample
{
    public static class RSAUtilities
    {
        public static RSA ToRSA(RsaPrivateCrtKeyParameters privKey)
        {
            return CreateRSAProvider(ToRSAParameters(privKey));
        }

        private static RSAParameters ToRSAParameters(RsaPrivateCrtKeyParameters privKey)
        {
            var rp = new RSAParameters();
            rp.Modulus = privKey.Modulus.ToByteArrayUnsigned();
            rp.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
            rp.P = privKey.P.ToByteArrayUnsigned();
            rp.Q = privKey.Q.ToByteArrayUnsigned();
            rp.D = ConvertRSAParametersField(privKey.Exponent, rp.Modulus.Length);
            rp.DP = ConvertRSAParametersField(privKey.DP, rp.P.Length);
            rp.DQ = ConvertRSAParametersField(privKey.DQ, rp.Q.Length);
            rp.InverseQ = ConvertRSAParametersField(privKey.QInv, rp.Q.Length);
            return rp;
        }

        private static byte[] ConvertRSAParametersField(BigInteger n, int size)
        {
            var bs = n.ToByteArrayUnsigned();

            if (bs.Length == size)
                return bs;

            if (bs.Length > size)
                throw new ArgumentException("Specified size too small", "size");

            var padded = new byte[size];
            Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
            return padded;
        }

        private static RSA CreateRSAProvider(RSAParameters rp)
        {
            // var csp = new CspParameters();
            // csp.KeyContainerName = string.Format("BouncyCastle-{0}", Guid.NewGuid());
            // var rsaCsp = new RSACryptoServiceProvider(csp);
            var rsaCsp = RSA.Create();
            rsaCsp.ImportParameters(rp);
            return rsaCsp;
        }
    }
}