using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;

namespace NEP
{
    public class Contract : SmartContract
    {
        public static readonly byte[] OwnerScriptHash = "ASH7gNFExYgMovKKUk4y5hWshrTZDt9BJj".ToScriptHash();

        public static object Main(byte[] activityType, params object[] args)
        {
            string actionType = "";
            if (args != null && args.Length > 0)
                actionType = activityType.AsString();

            //NEP5 Token Methods
            if (actionType == "deploy") return NEP5Token.Deploy();
            if (actionType == "mintTokens") return NEP5Token.MintTokens();
            if (actionType == "totalSupply") return NEP5Token.TotalSupply();
            if (actionType == "name") return NEP5Token.Name();
            if (actionType == "symbol") return NEP5Token.Symbol();
            if (actionType == "transfer") return NEP5Token.Transfer((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
            if (actionType == "balanceOf") return NEP5Token.BalanceOf((byte[])args[0]);
            if (actionType == "decimals") return NEP5Token.Decimals();
            if (actionType == "transferFrom") return NEP5Token.TransferFrom((byte[])args[0], (byte[])args[1], (byte[])args[2], (BigInteger)args[3]);
            if (actionType == "approve") return NEP5Token.Approve((byte[])args[0], (byte[])args[1], (byte[])args[2]);
            if (actionType == "allowance") return NEP5Token.Allowance((byte[])args[0], (byte[])args[1]);

            //EF
            if (actionType == "ef-create") return EF.Create((byte[])args[0], (byte[])args[1], (byte[])args[2]);
            if (actionType == "ef-list") return EF.List(args);
            if (actionType == "ef-get") return EF.Get((byte[])args[0], (byte[])args[1], (int)args[2]);
            if (actionType == "ef-update") return EF.Update((byte[])args[0], (byte[])args[1], (int)args[2], (byte[])args[3]);

            return Runtime.CheckWitness(OwnerScriptHash);
        }
    }
}
