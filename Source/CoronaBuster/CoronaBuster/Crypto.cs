using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace CoronaBuster {
    public class Crypto {

        public static void Test() {
            var alice = GenerateKeyPair();
            var bob = GenerateKeyPair();

            var s1 = GetSharedSecret(alice.privateKey, bob.publicKey);
            var s2 = GetSharedSecret(bob.privateKey, alice.publicKey);

            for (int i = 0; i < s1.Length; i++) {
                if (s1[i] != s2[i]) throw new Exception("FAIL!");
            }
        }

        public static (byte[] publicKey, byte[] privateKey) GenerateKeyPair() {
            var random = new SecureRandom();
            
            var curve = NistNamedCurves.GetByName("P-256");

            var pGen = new ECKeyPairGenerator();
            var genParam = new ECKeyGenerationParameters(
                new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed()),
                random);

            pGen.Init(genParam);

            var pair = pGen.GenerateKeyPair();

            var param = new ParametersWithRandom(pair.Private, random);

            var publicKey = ((ECPublicKeyParameters)pair.Public).Q.GetEncoded(compressed: true);
            var privateKey = ((ECPrivateKeyParameters)pair.Private).D.ToByteArray();

            return (publicKey, privateKey);
        }

        public static uint GetShortSharedSecret(byte[] privateKey, byte[] publicKey, uint salt) {
            var sharedSecret = GetSharedSecret(privateKey, publicKey);
            return Hash(sharedSecret, salt);
        }

        public static byte[] GetSharedSecret(byte[] PrivateKeyIn, byte[] PublicKeyIn) {
            var agreement = new ECDHCBasicAgreement();
            
            var curve = NistNamedCurves.GetByName("P-256");
            var ecParam = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var privKey = new ECPrivateKeyParameters(new BigInteger(PrivateKeyIn), ecParam);
            var point = ecParam.Curve.DecodePoint(PublicKeyIn);
            var pubKey = new ECPublicKeyParameters(point, ecParam);

            agreement.Init(privKey);

            BigInteger secret = agreement.CalculateAgreement(pubKey);

            return secret.ToByteArrayUnsigned();
        }

        public static uint Hash(byte[] data, uint id) {
            var salt = BitConverter.GetBytes(id);
            var pdb = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
            pdb.Init(data, salt, 1);
            var key = (KeyParameter)pdb.GenerateDerivedMacParameters(32);
            var keyBytes = key.GetKey();
            return BitConverter.ToUInt32(keyBytes, 0);
        }
    }
}
