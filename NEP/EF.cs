using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NSFH = Neo.SmartContract.Framework.Helper;

namespace NEP
{
    internal static class EF
    {
        internal static object Create(byte[] scriptHash, byte[] entityName, byte[] name)
        {
            if (!Runtime.CheckWitness(scriptHash)) return false;

            BigInteger entityID, entityCount;
            string entityMaxIdKey, entityKey, entityCountKey;

            entityMaxIdKey = Key.Generate(Prefix.EntityMaxId, entityName, scriptHash);
            entityCountKey = Key.Generate(Prefix.EntityCount, entityName, scriptHash);


            byte[] entityMaxIdBytes = DB.Get(entityMaxIdKey);
            byte[] entityCountBytes = DB.Get(entityCountKey);

            if (entityMaxIdBytes == null)
            {
                DB.Put(entityMaxIdKey, 1);
                DB.Put(entityCountKey, 1);
                entityID = 1;
            }
            else
            {
                entityID = NSFH.AsBigInteger(entityMaxIdBytes) + 1;
                entityCount = NSFH.AsBigInteger(entityCountBytes) + 1;
                DB.Put(entityMaxIdKey, entityID);
            }
            entityKey = Key.Generate(entityName, scriptHash, entityID);
            DB.Put(entityKey, name);
            Runtime.Notify(entityName + " created successfully", entityID, name.AsString());
            return entityID;
        }

        internal static object List(object[] args)
        {
            byte[] scriptHash = (byte[])args[0];
            byte[] entityName = (byte[])args[1];

            int itemPerView = 10;
            if (args.Length == 3)
                itemPerView = (int)args[2];

            int startId = 1;
            if (args.Length == 3)
                startId = (int)args[3];

            if (!Runtime.CheckWitness(scriptHash)) return false;

            BigInteger entityMaxId, endId;
            string entityMaxIdKey;
            entityMaxIdKey = Key.Generate(Prefix.EntityMaxId, entityName, scriptHash);
            byte[] entityMaxIdBytes = DB.Get(entityMaxIdKey);
            if (entityMaxIdBytes == null)
            {
                Runtime.Notify("total " + entityName + " available", 0);
                return NSFH.AsByteArray(0);
            }
            entityMaxId = NSFH.AsBigInteger(entityMaxIdBytes);
            if (itemPerView <= 0) itemPerView = 10;
            if (startId > entityMaxId) startId = 1;
            endId = startId + itemPerView - 1;

            for (int i = startId; i <= endId; i++)
            {
                string entityKey = Key.Generate(entityName, scriptHash, i);
                byte[] entityValue = DB.Get(entityKey);
                if (entityValue != null)
                    Runtime.Notify(entityName, i, entityValue.AsString());
            }
            //TODO Changed to return Array of values
            return NSFH.AsByteArray(1);
        }

        internal static object Get(byte[] scriptHash, byte[] entityName, int id)
        {
            if (!Runtime.CheckWitness(scriptHash)) return false;

            string entityKey = Key.Generate(entityName, scriptHash, id);
            byte[] entityValue = DB.Get(entityKey);
            if (entityValue != null)
            {
                Runtime.Notify(entityName, id, entityValue.AsString());
                return entityValue;
            }
            Runtime.Notify(entityName, id, "do not exist");
            return NSFH.AsByteArray(0);
        }

        internal static object Update(byte[] scriptHash, byte[] entityName, int id, byte[] value)
        {
            if (!Runtime.CheckWitness(scriptHash)) return false;

            string entityKey = Key.Generate(entityName, scriptHash, id);
            byte[] entityValue = DB.Get(entityKey);
            if (entityValue != null)
            {
                DB.Put(entityKey, value);
                Runtime.Notify(entityName, id, entityValue.AsString(), "Updated to", value);
                return entityValue;
            }
            Runtime.Notify(entityName, id, entityValue.AsString(), "do not exist");
            return NSFH.AsByteArray(0);
        }
    }
}
