using Neo.SmartContract.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NEP
{
    internal static class Key
    {
        internal static string Generate(string prefix, byte[] entityName, byte[] scriptHash)
        {
            return string.Concat(prefix, entityName, scriptHash.AsString());
        }

        internal static string Generate(byte[] entityName, byte[] scriptHash, BigInteger id)
        {
            return string.Concat(entityName, scriptHash.AsString(), id);
        }

    }
}
