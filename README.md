## 完全分散型メタバースProject Decentralized Metaverse Project

![image](https://github.com/DecentralizedMetaverse/fogverse/assets/38463346/90f12030-1437-4644-b9ed-52df5b17b12c)



This project aims to create a decentralized metaverse. 

The metaverse allows users to interact with virtual environments and objects in a distributed manner.

### Requirements

- [IPFS](https://docs.ipfs.tech/install/): InterPlanetary File System (IPFS) is required for the content layer of the metaverse. Please add an environment variable so that the ipfs command can be used.

### Required Assets

Please import the following assets before starting with the project:

- [Runtime OBJ Importer](https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547): This asset enables runtime OBJ file importing into Unity.
- [Starter Assets - ThirdPerson | Updates in new CharacterController package | Essentials | Unity Asset Store](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526)

### How to Operate

- To upload files to the virtual space, drag and drop them directly. (Please note that this will not work on UnityEditor.)

- To add a component to an object, follow these steps:
  1. Select the object
  2. Press Shift + Click to bring up the component menu.
  3. Choose the desired component to add to the object.

### MistNet Protocol

The metaverse consists of two layers:

1. Content Layer:
   - IPFS: The InterPlanetary File System (IPFS) is used for storing and sharing the content within the metaverse. It allows for distributed file storage and retrieval.

2. Realtime Layer:
   - WebRTC: Web Real-Time Communication (WebRTC) is utilized for the realtime layer of the metaverse. It enables communication and synchronization between users in the virtual environment.
   - Sync Object: Within the metaverse, objects can be synchronized in terms of their location and variables, allowing for interactions between users.

Additionally, Lua scripting can be used to add custom scripts to objects within the metaverse. 

[API Reference](docs/api_reference.md).

### How to Contribute
Anyone can participate to a project.
For questions or code modifications, please send us an issue or pull request.
