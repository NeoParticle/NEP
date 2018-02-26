using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace NEP
{
    internal static class DB
    {
        internal static byte[] Get(byte[] key) => Storage.Get(Storage.CurrentContext, key);
        internal static byte[] Get(string key) => Storage.Get(Storage.CurrentContext, key);
        internal static void Put(byte[] key, byte[] value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Put(byte[] key, BigInteger value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Put(byte[] key, string value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Put(string key, byte[] value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Put(string key, BigInteger value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Put(string key, string value) => Storage.Put(Storage.CurrentContext, key, value);
        internal static void Delete(string key) => Storage.Delete(Storage.CurrentContext, key);
        internal static void Delete(byte[] key) => Storage.Delete(Storage.CurrentContext, key);
    }
}
