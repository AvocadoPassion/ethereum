using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Collections;
using Nethereum.JsonRpc.UnityClient;
using UnityEngine;

public class TransferContract
{

    public static string ABI = @"[{
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""getBalance"",
        ""outputs"": [{
                ""name"": """",
                ""type"": ""uint256""}],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""},{
        ""constant"": false,
        ""inputs"": [],
        ""name"": ""withdraw"",
        ""outputs"": [{
                ""name"": ""success"",
                ""type"": ""bool""}],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""},{
        ""constant"": true,
        ""inputs"": [{
                ""name"": """",
                ""type"": ""address""}],
        ""name"": ""balanceOf"",
        ""outputs"": [{
                ""name"": """",
                ""type"": ""uint256""
            }],
        ""payable"": false,
        ""stateMutability"": ""view"",
        ""type"": ""function""},{
        ""constant"": false,
        ""inputs"": [],
        ""name"": ""transfer"",
        ""outputs"": [{
                ""name"": ""success"",
                ""type"": ""bool""}],
        ""payable"": false,
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""},{
        ""constant"": false,
        ""inputs"": [],
        ""name"": ""deposit"",
        ""outputs"": [{
                ""name"": ""success"",
                ""type"": ""bool"" }],
        ""payable"": true,
        ""stateMutability"": ""payable"",
        ""type"": ""function""},{
        ""anonymous"": false,
        ""inputs"": [{
                ""indexed"": false,
                ""name"": ""owner"",
                ""type"": ""address""},{
                ""indexed"": false,
                ""name"": ""amount"",
                ""type"": ""uint256""}],
        ""name"": ""LogDeposit"",
        ""type"": ""event""},{
        ""anonymous"": false,
        ""inputs"": [{
                ""indexed"": false,
                ""name"": ""receiver"",
                ""type"": ""address""},{
                ""indexed"": false,
                ""name"": ""amount"",
                ""type"": ""uint256""}],
        ""name"": ""LogWithdraw"",
        ""type"": ""event""},{
        ""anonymous"": false,
        ""inputs"": [{
                ""indexed"": false,
                ""name"": ""owner"",
                ""type"": ""address""},{
                ""indexed"": false,
                ""name"": ""to"",
                ""type"": ""address""},{
                ""indexed"": false,
                ""name"": ""amount"",
                ""type"": ""uint256""}],
        ""name"": ""LogTransfer"",
        ""type"": ""event""}]";

    private static string contractAddress = "0xaAb4d662c98500A64373C5C588De0F00FFCA4771";
    private Contract contract;

    public TransferContract()
    {
        this.contract = new Contract(null, ABI, contractAddress);
    }

    public Nethereum.Contracts.Event LogDepositEvent()
    {
        return contract.GetEvent("LogDeposit");
    }

    public Nethereum.Contracts.Event LogWithdrawEvent()
    {
        return contract.GetEvent("LogWithdraw");
    }

    public Nethereum.Contracts.Event LogTransferEvent()
    {
        return contract.GetEvent("LogTransfer");
    }

    public Function DepositFunction()
    {
        return contract.GetFunction("deposit");
    }

    public Function GetBalanceOfFunction()
    {
        return contract.GetFunction("getBalance");
    }

    public Function WithdrawFunction()
    {
        return contract.GetFunction("withdraw");
    }

    public Function TransferFunction()
    {
        return contract.GetFunction("transfer");
    }

    HexBigInteger gas = new HexBigInteger(500000000000);
    HexBigInteger gasPrice = new HexBigInteger(400);

    public TransactionInput CreateDepositInput(string addressFrom, string addressTo, BigInteger value, HexBigInteger gas,
        HexBigInteger gasPrice, HexBigInteger valueAmount = null)
    {
        var function = DepositFunction();
        return function.CreateTransactionInput(addressFrom, gas, gasPrice, valueAmount);
    }

    public NewFilterInput CreateLogDepositInput()
    {
        var evt = LogDepositEvent();
        return evt.CreateFilterInput(Nethereum.RPC.Eth.DTOs.BlockParameter.CreateEarliest(), Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
    }

    public NewFilterInput CreateLogWithdrawInput()
    {
        var evt = LogWithdrawEvent();
        return evt.CreateFilterInput(Nethereum.RPC.Eth.DTOs.BlockParameter.CreateEarliest(), Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
    }

    public NewFilterInput CreateLogTransferInput()
    {
        var evt = LogTransferEvent();
        return evt.CreateFilterInput(Nethereum.RPC.Eth.DTOs.BlockParameter.CreateEarliest(), Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
    }

    public CallInput CreateBalanceOfInput(string addressFrom)
    {
        var function = GetBalanceOfFunction();
        return function.CreateCallInput(addressFrom.RemoveHexPrefix());
    }

    public int DecodeBalance(string balance)
    {
        // We will use this function later to parse the result of encoded balance (Hexadecimal 0x0f)
        // into a decoded output (Integer 15)
        var function = GetBalanceOfFunction();
        var decodedBalance = function.DecodeSimpleTypeOutput<BigInteger>(balance);
        return (int)Nethereum.Util.UnitConversion.Convert.FromWei(decodedBalance, 18);
    }

    public TransactionInput CreateWithdrawInput(string addressFrom, string addressTo, BigInteger value, HexBigInteger gas = null,
    HexBigInteger gasPrice = null, HexBigInteger valueAmount = null)
    {
        var function = WithdrawFunction();
        return function.CreateTransactionInput(addressFrom, gas, gasPrice, valueAmount);
    }

    public TransactionInput CreateTransferInput(string addressFrom, string addressTo, BigInteger value, HexBigInteger gas = null,
    HexBigInteger gasPrice = null, HexBigInteger valueAmount = null)
    {
        var function = TransferFunction();
        return function.CreateTransactionInput(addressFrom, gas, gasPrice, valueAmount);
    }


    //HexBigInteger gas = new HexBigInteger(3000000);
    //HexBigInteger gasPrice = new HexBigInteger(20);



    // We are going to use this IEnumerator to loop every 10 seconds and check the 
    // transaction hash, to see if its mined in the blockchain
    public IEnumerator CheckTransactionReceiptIsMined(
        string url, string txHash, System.Action<bool> callback
    )
    {
        var mined = false;
        // we are going to set the tries to 999 for testing purposes
        var tries = 999;
        while (!mined)
        {
            if (tries > 0)
            {
                tries = tries - 1;
            }
            else
            {
                mined = true;
                Debug.Log("Performing last try..");
            }
            Debug.Log("Checking receipt for: " + txHash);
            var receiptRequest = new EthGetTransactionReceiptUnityRequest(url);
            yield return receiptRequest.SendRequest(txHash);
            if (receiptRequest.Exception == null)
            {
                if (receiptRequest.Result != null && receiptRequest.Result.Logs.HasValues)
                {
                    var txType = receiptRequest.Result.Logs[0]["type"].ToString();
                    if (txType == "mined")
                    {
                        // if we have a transaction type == mined we return the callback
                        // and exit the loop
                        mined = true;
                        callback(mined);
                    }
                }
            }
            else
            {
                // If we had an error doing the request
                Debug.Log("Error checking receipt: " + receiptRequest.Exception.Message);
            }
            yield return new WaitForSeconds(10);
        }
    }
}


