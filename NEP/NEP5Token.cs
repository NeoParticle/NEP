using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NEP
{
    internal static class NEP5Token
    {
        //Token Settings
        public static string Name() => "NEO Particle";
        public static string Symbol() => "NEP";
        public static byte Decimals() => 8;
        private const ulong factor = 100000000;
        private const ulong neoDecimals = 100000000;

        //ICO Settings
        private static readonly byte[] neoId = new byte[] { 197, 111, 51, 252, 110, 207, 205, 12, 34, 92, 74, 179, 86, 254, 229, 147, 144, 175, 133, 96, 190, 14, 147, 15, 174, 190, 116, 166, 218, 255, 124, 155 };
        private const ulong totalAmount = 100000000 * factor;
        private const ulong preIcoCap = 10000000 * factor;
        private const ulong baseRate = 1000 * factor;
        private const int icoStartTime = 1519488000;
        private const int icoEndTime = 1521907200;

        [DisplayName("transfer")]
        public static event Action<byte[], byte[], BigInteger> Transferred;

        [DisplayName("refund")]
        public static event Action<byte[], BigInteger> Refund;

        public static object Deploy()
        {
            byte[] totalSupply = DB.Get(Prefix.TotalSupply);
            if (totalSupply.Length != 0) return false;
            DB.Put(Contract.OwnerScriptHash, preIcoCap);
            Runtime.Notify("PreICOCap", preIcoCap);
            DB.Put(Prefix.TotalSupply, preIcoCap);
            Runtime.Notify(Contract.OwnerScriptHash, preIcoCap);
            Transferred(null, Contract.OwnerScriptHash, preIcoCap);
            return true;
        }

        public static bool MintTokens()
        {
            byte[] sender = GetSenderScriptHash();

            if (sender.Length == 0)
                return false;

            ulong contributeValue = GetTotalContribution();

            ulong swapRate = CurrentSwapRate();

            if (swapRate == 0)
            {
                Refund(sender, contributeValue);
                return false;
            }

            ulong token = CurrentSwapToken(sender, contributeValue, swapRate);
            if (token == 0)
                return false;

            byte[] balanceBytes = DB.Get(sender);
            DB.Put(sender, token + balanceBytes.AsBigInteger());
            BigInteger totalSupply = DB.Get(Prefix.TotalSupply).AsBigInteger();
            DB.Put(Prefix.TotalSupply, token + totalSupply);
            Transferred(null, sender, token);
            return true;
        }

        public static BigInteger TotalSupply()
        {
            return DB.Get(Prefix.TotalSupply).AsBigInteger();
        }
        public static bool Transfer(byte[] fromScriptHash, byte[] toScriptHash, BigInteger value)
        {
            if (value <= 0) return false;
            if (!Runtime.CheckWitness(fromScriptHash)) return false;
            if (fromScriptHash == toScriptHash) return true;
            BigInteger fromValue = DB.Get(fromScriptHash).AsBigInteger();
            if (fromValue < value) return false;
            if (fromValue == value)
                DB.Delete(fromScriptHash);
            else
                DB.Put(fromScriptHash, fromValue - value);
            BigInteger toValue = DB.Get(toScriptHash).AsBigInteger();
            DB.Put(toScriptHash, toValue + value);
            Runtime.Notify("Transfer", toScriptHash, toValue + value);
            Transferred(fromScriptHash, toScriptHash, value);
            return true;
        }

        internal static void RefundToken()
        {
            byte[] sender = GetSenderScriptHash();
            ulong contributeValue = GetTotalContribution();
            if (contributeValue > 0 && sender.Length != 0)
            {
                Refund(sender, contributeValue);
            }
        }

        public static BigInteger BalanceOf(byte[] scriptHash)
        {
            byte[] balanceBytes = DB.Get(scriptHash);
            return DB.Get(scriptHash).AsBigInteger();
        }

        public static bool TransferFrom(byte[] originatorScriptHash, byte[] fromScriptHash, byte[] toScriptHash, BigInteger value)
        {
            if (!Runtime.CheckWitness(originatorScriptHash)) return false;

            BigInteger allowedTokens = DB.Get(fromScriptHash.Concat(originatorScriptHash)).AsBigInteger();
            BigInteger fromValInt = DB.Get(fromScriptHash).AsBigInteger();
            BigInteger toValInt = DB.Get(toScriptHash).AsBigInteger();

            if (fromValInt >= value && value >= 0 && allowedTokens >= 0)
            {
                DB.Put(fromScriptHash.Concat(originatorScriptHash), (allowedTokens - value));
                DB.Put(toScriptHash, (toValInt + value));
                DB.Put(fromScriptHash, (fromValInt - value));
                return true;
            }
            return false;
        }

        public static bool Approve(byte[] originatorScriptHash, byte[] toScriptHash, byte[] value)
        {
            if (!Runtime.CheckWitness(originatorScriptHash)) return false;
            DB.Put(originatorScriptHash.Concat(toScriptHash), value);
            return true;
        }

        public static object Allowance(byte[] fromScriptHash, byte[] toScriptHash)
        {
            if (!Runtime.CheckWitness(toScriptHash)) return false;
            return DB.Get(fromScriptHash.Concat(toScriptHash)).AsBigInteger();
        }

        private static ulong CurrentSwapToken(byte[] sender, ulong value, ulong swapRate)
        {
            ulong token = value / neoDecimals * swapRate;
            BigInteger totaSupply = DB.Get(Prefix.TotalSupply).AsBigInteger();
            BigInteger balance_token = totalAmount - totaSupply;
            if (balance_token <= 0)
            {
                Refund(sender, value);
                return 0;
            }
            else if (balance_token < token)
            {
                Refund(sender, (token - balance_token) / swapRate * neoDecimals);
                token = (ulong)balance_token;
            }
            return token;
        }

        private static byte[] GetSenderScriptHash()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] reference = tx.GetOutputs();
            foreach (TransactionOutput output in reference)
                if (output.AssetId == neoId) return output.ScriptHash;
            return new byte[] { };
        }

        private static ulong GetTotalContribution()
        {
            Transaction transaction = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = transaction.GetOutputs();
            ulong value = 0;
            foreach (TransactionOutput output in outputs)
                if (output.ScriptHash == ExecutionEngine.ExecutingScriptHash && output.AssetId == neoId)
                    value += (ulong)output.Value;
            return value;
        }

        private static ulong CurrentSwapRate()
        {
            const int icoDuration = icoEndTime - icoStartTime;
            uint now = Runtime.Time;
            int time = (int)now - icoStartTime;
            if (time < 0)
                return 0;
            if (time < icoDuration)
                return baseRate;
            return 0;
        }
    }
}
