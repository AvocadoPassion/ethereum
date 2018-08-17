# ethereum
Augmented Smart Contracts

This repository stores the final codes I have for my master dissertation on integrating Ethereum smart contracts with the Microsoft Hololens via Unity.

WithdrawContract_Remix and TransferContract_Remix are the codes from the Remix IDE with which we can create and store the contract on the blockchain via the ABI and ByteCode. Both codes come from the below Stack Exchange post which describes how to deposit ether on a contract, withdraw from that contract to the account the contract is stored upon, and transfer ether from the contract to a different account. We only modifed the code slightly mainly by adding hard coded values and hard coding our different address as otherwise we were getting problems connecting it to Unity.
https://ethereum.stackexchange.com/questions/19080/is-it-possible-to-send-ether-from-contract-address-to-my-account-address-using-t

WithdrawContract.cs, WithdrawToken.cs, TransferContract.cs, and TransferToken.cs are taken from the "Integrating Unity3D with the Ethereum blockchain [PART 3] — Events & Tokens" tutorial on the following website:
https://blog.e11.io/integrating-unity3d-with-the-ethereum-blockchain-part-3-events-tokens-232a340c3477
The codes have been modified to fit my specific contract and functions.

For our application we use the latest Nethereum package, 3.0.0-rc1 which can be found here: https://github.com/Nethereum/Nethereum/releases
Documentation on Nethereum can be found here: https://nethereum.readthedocs.io/en/latest/

We also use the Holotoolkit which can be found here: https://github.com/Microsoft/MixedRealityToolkit-Unity.
We used this as we couldn't run the Holographic Remoting Player without it and at the same time it is useful as it sets up the camera, cursor, and gaze for the Hololens.

For our final application we used Unity 2018.2.2f1 and the Holographic Remoting Player which we had to download on the Microsoft Hololens. This can be found here: https://docs.microsoft.com/en-us/windows/mixed-reality/holographic-remoting-player.

Our Scene folder stores the menu scene.


(ConversationContract, MyToken, and Smart Contract were the codes we were working on until we had to completley restructure our smart contracts as even though the contract was being funded no money was ever being transferred.)
