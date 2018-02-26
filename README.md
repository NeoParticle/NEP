# NEP [ONLY ON NEO TESTNET]
Generic Smart Contract which can be utilized to build Smart Contract

Direction Of Usage

```js
[AppCall("0xb693f49e295e3b72db4fe2f589756b9527266b3f")] // ScriptHash
public static extern int AnotherContract (byte[] activityType, params object[] args);

public static void Main ()
{
      //To check total Supply
     AnotherContract ((byte[])"totalSupply");
}
```
