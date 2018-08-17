using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using System;
using System.Numerics;

public class TransferToken : MonoBehaviour
{

    private string accountAddress;
    private string accountPrivateKey = "f270c7a3841bb40ea77b9dadbb7257d8a9e4744692622a89910ff1f0352bfc25";
    private string _url = "https://ropsten.infura.io";
    private DeployContractTransactionBuilder contractTransactionBuilder = new DeployContractTransactionBuilder();
    private string thirdAddress = "0x93B9030191562956024ABC8046d97b53dE347877";
    private TransferContract transferContract = new TransferContract();

    void Start()
    {
        importAccountFromPrivateKey();

        //deposit();
        //checkLogDepositEvent();
        //withdraw();
        //checkLogWithdrawEvent();
        //transfer();
        //checkLogTransferEvent();
        //checkMyBalance();

    }

    public void importAccountFromPrivateKey()
    {
        try
        {
            var address = Nethereum.Signer.EthECKey.GetPublicAddress(accountPrivateKey);
            accountAddress = address;
            Debug.Log("Imported account SUCCESS");
        }
        catch (Exception e)
        {
            Debug.Log("error" + e);
        }
    }

    public void checkMyBalance()
    {
        // we create a new balance request, sending the address we want to check
        // in this case we use our public address
        //StartCoroutine(getBalanceRequest(accountAddress));
        StartCoroutine(getBalanceOfRequest(accountAddress));
    }

    public void deposit()
    {
        StartCoroutine(depositRequest(thirdAddress, Nethereum.Util.UnitConversion.Convert.ToWei(2010)));
    }

    public void checkLogDepositEvent()
    {
        StartCoroutine(getLogDepositEventRequest());
    }

    public void checkLogWithdrawEvent()
    {
        StartCoroutine(getLogWithdrawEventRequest());
    }

    public void checkLogTransferEvent()
    {
        StartCoroutine(getLogTransferEventRequest());
    }

    public void withdraw()
    {
        StartCoroutine(withdrawRequest(thirdAddress, Nethereum.Util.UnitConversion.Convert.ToWei(2010)));
    }

    public void transfer()
    {
        StartCoroutine(transferRequest(thirdAddress, Nethereum.Util.UnitConversion.Convert.ToWei(2010)));
    }


    // we are going to use this function, to call the CheckTransactionReceiptIsMined() function
    // we created in the pingTokenService class, this will trigger every 10 seconds until
    // the transaction hash we sent is mined, throws an error or tries 999 times (just for testing)
    public void checkTx(string txHash, Action<bool> callback)
    {
        StartCoroutine(transferContract.CheckTransactionReceiptIsMined(
            _url,
            txHash,
            (cb) => {
                Debug.Log("The transaction has been mined succesfully");
                // we send a callback to the function, here you can add some more logic if you wish
                callback(true);
            }
        ));
    }

    // here we define all the IEnumerator functions for the requests
    // Basically, the requests needs two things
    // First, a request, EthCallUnityRequest for calls,
    // EthGetLogsUnityRequest for events, and TransactionSignedUnityRequest for transactions), 
    // Second, an input, we created it in the ContractService for each case.
    // After that, we just yield return the request, sending the input and
    // the block you want to check (depending on what we are doing)

    public IEnumerator getBalanceOfRequest(string address)
    {
        var getBalanceRequest = new EthCallUnityRequest(_url);
        var getBalanceInput = transferContract.CreateBalanceOfInput(address);
        Debug.Log("Getting balance of: " + address);
        yield return getBalanceRequest.SendRequest(getBalanceInput, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
        if (getBalanceRequest.Exception == null)
        {
            // So, basically this is the same for all the requests, if we have no exception
            // we decode (if needed) the result, and if we have an exception we show 
            // the exception message
            Debug.Log("Balance: " + transferContract.DecodeBalance(getBalanceRequest.Result));
            GetComponent<TextMesh>().text = "Balance: " + transferContract.DecodeBalance(getBalanceRequest.Result);
        }
        else
        {
            Debug.Log("Error getting balance: " + getBalanceRequest.Exception.Message);
        }
    }

    public IEnumerator getLogDepositEventRequest()
    {
        var getLogDepositRequest = new EthGetLogsUnityRequest(_url);
        var getLogDepositInput = transferContract.CreateLogDepositInput();
        Debug.Log("Checking deposit event...");
        yield return getLogDepositRequest.SendRequest(getLogDepositInput);
        if (getLogDepositRequest.Exception == null)
        {
            Debug.Log("Deposit Event: " + getLogDepositRequest.Result);
        }
        else
        {
            Debug.Log("Error getting deposit event: " + getLogDepositRequest.Exception.Message);
        }
    }

    public IEnumerator getLogWithdrawEventRequest()
    {
        var getLogWithdrawRequest = new EthGetLogsUnityRequest(_url);
        var getLogWithdrawInput = transferContract.CreateLogWithdrawInput();
        Debug.Log("Checking withdraw event...");
        yield return getLogWithdrawRequest.SendRequest(getLogWithdrawInput);
        if (getLogWithdrawRequest.Exception == null)
        {
            Debug.Log("Withdraw Event: " + getLogWithdrawRequest.Result);
        }
        else
        {
            Debug.Log("Error getting withdraw event: " + getLogWithdrawRequest.Exception.Message);
        }
    }

    public IEnumerator getLogTransferEventRequest()
    {
        var getLogTransferRequest = new EthGetLogsUnityRequest(_url);
        var getLogTransferInput = transferContract.CreateLogTransferInput();
        Debug.Log("Checking transfer event...");
        yield return getLogTransferRequest.SendRequest(getLogTransferInput);
        if (getLogTransferRequest.Exception == null)
        {
            Debug.Log("Transfer Event: " + getLogTransferRequest.Result);
        }
        else
        {
            Debug.Log("Error getting transfer event: " + getLogTransferRequest.Exception.Message);
        }
    }

    public IEnumerator depositRequest(string address, BigInteger value)
    {
        var transactionInput = transferContract.CreateDepositInput(
            accountAddress, address, value,
            new HexBigInteger(200000), new HexBigInteger(190), new HexBigInteger(50)
        );
        Debug.Log("Transfering tokens for deposit to: " + address);
        var transactionSignedRequest = new TransactionSignedUnityRequest(_url, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created for deposit: " + transactionSignedRequest.Result);
            // Now we check the transaction until it's mined in the blockchain as we did in
            // the pingRequest, when the callback is triggered (transaction mined),
            // we execute the getBalanceRequest for the address we send tokens to
            checkTx(transactionSignedRequest.Result, (cb) => {
                //StartCoroutine(getBalanceOfRequest(address));
            });
        }
        else
        {
            Debug.Log("Error transfering tokens for deposit: " + transactionSignedRequest.Exception.Message);
        }
    }

    public IEnumerator withdrawRequest(string address, BigInteger value)
    {
        var transactionInput = transferContract.CreateWithdrawInput(
            accountAddress, address, value,
            new HexBigInteger(200000), new HexBigInteger(190), new HexBigInteger(0)
        );
        Debug.Log("Transfering tokens for withdrawal: " + address);
        var transactionSignedRequest = new TransactionSignedUnityRequest(_url, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created for withdrawal: " + transactionSignedRequest.Result);
            // Now we check the transaction until it's mined in the blockchain as we did in
            // the pingRequest, when the callback is triggered (transaction mined),
            // we execute the getBalanceRequest for the address we send tokens to
            checkTx(transactionSignedRequest.Result, (cb) => {
                //StartCoroutine(getBalanceOfRequest(address));
            });
        }
        else
        {
            Debug.Log("Error transfering tokens for withdrawal: " + transactionSignedRequest.Exception.Message);
        }
    }

    public IEnumerator transferRequest(string address, BigInteger value)
    {
        var transactionInput = transferContract.CreateTransferInput(
            accountAddress, address, value,
            new HexBigInteger(200000), new HexBigInteger(190), new HexBigInteger(0)
        );
        Debug.Log("Transfering tokens: " + address);
        var transactionSignedRequest = new TransactionSignedUnityRequest(_url, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created: " + transactionSignedRequest.Result);
            // Now we check the transaction until it's mined in the blockchain as we did in
            // the pingRequest, when the callback is triggered (transaction mined),
            // we execute the getBalanceRequest for the address we send tokens to
            checkTx(transactionSignedRequest.Result, (cb) => {
                //StartCoroutine(getBalanceOfRequest(address));
            });
        }
        else
        {
            Debug.Log("Error transfering tokens: " + transactionSignedRequest.Exception.Message);
        }
    }

    public void deployEthereumContract()
    {
        print("Deploying contract...");
        // Here we have our ABI & bytecode required for both creating and accessing our contract.
        var abi = @"[{
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

        var byteCode = @"608060405260326001556007600255600b60035534801561001f57600080fd5b506103468061002f6000396000f30060806040526004361061006c5763ffffffff7c010000000000000000000000000000000000000000000000000000000060003504166312065fe081146100715780633ccfd60b1461009857806370a08231146100c15780638a4068dd146100ef578063d0e30db014610104575b600080fd5b34801561007d57600080fd5b5061008661010c565b60408051918252519081900360200190f35b3480156100a457600080fd5b506100ad610111565b604080519115158252519081900360200190f35b3480156100cd57600080fd5b5061008673ffffffffffffffffffffffffffffffffffffffff600435166101bf565b3480156100fb57600080fd5b506100ad6101d1565b6100ad6102b4565b303190565b600254336000908152602081905260408120549091111561013157600080fd5b600280543360008181526020819052604080822080549490940390935592549151909282156108fc02929190818181858888f1935050505015801561017a573d6000803e3d6000fd5b5060025460408051338152602081019290925280517f4ce7033d118120e254016dccf195288400b28fc8936425acd5f17ce2df3ab7089281900390910190a150600190565b60006020819052908152604090205481565b600354336000908152602081905260408120549091829110156101f357600080fd5b506003805433600090815260208190526040808220805493909303909255915490517393b9030191562956024abc8046d97b53de34787792839280156108fc02929091818181858888f19350505050158015610253573d6000803e3d6000fd5b506003546040805133815273ffffffffffffffffffffffffffffffffffffffff8416602082015280820192909252517f0a85107a334eae0d22d21cdf13af0f8e8125039ec60baaa843d2c4c5b06801749181900360600190a1600191505090565b60015460009034146102c557600080fd5b336000818152602081815260409182902080543490810190915582519384529083015280517f1b851e1031ef35a238e6c67d0c7991162390df915f70eaf9098dbf0b175a61989281900390910190a1506001905600a165627a7a723058206f42b1a6e99e1e117c013eabca208bb7a434b6a832c4fcc94e4584b0245692550029";

        StartCoroutine(deployContract(abi, byteCode, accountAddress, (result) => {
            print("Result " + result);
        }));

    }
    public IEnumerator deployContract(string abi, string byteCode, string senderAddress, System.Action<string> callback)
    {
        //Amount of gas required to create the contract
        var gas = new HexBigInteger(900000);

        //First we build the transaction
        var transactionInput = contractTransactionBuilder.BuildTransaction(abi, byteCode, senderAddress, gas, null);

        // Here we create a new signed transaction Unity Request with the url, the private and public key
        // (this will sign the transaction automatically)
        var transactionSignedRequest = new TransactionSignedUnityRequest(_url, accountPrivateKey, accountAddress);

        // Then we send the request and wait for the transaction hash
        Debug.Log("Sending Deploy contract transaction...");
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            // If we don't have exceptions we just return the result!
            callback(transactionSignedRequest.Result);
        }
        else
        {
            // if we had an error in the UnityRequest we just display the Exception error
            throw new System.InvalidOperationException("Deploy contract tx failed:" + transactionSignedRequest.Exception.Message);
        }

    }

}


