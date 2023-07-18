# API Reference
## Scene
### Description

### Parameters
- System.String loadScene
### Returns
- System.Void
---
## SceneSingle
### Description

### Parameters
- System.String dt2
- System.String dt21
### Returns
- System.Void
---
## ShowLoading
### Description

### Returns
- Cysharp.Threading.Tasks.UniTask
---
## ShowLoadingAsync
### Description

### Returns
- System.Collections.IEnumerator
---
## CloseLoading
### Description

### Returns
- System.Void
---
## Run
### Description

### Parameters
- System.String code
### Returns
- Cysharp.Threading.Tasks.UniTask
---
## RunAsync
### Description

### Parameters
- System.String args
### Returns
- System.Collections.IEnumerator
---
## ExitScript
### Description

### Parameters
- System.Int32 runId
### Returns
- System.Void
---
## CMD
### Description

### Parameters
- System.String args
### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.String]
---
## Exe
### Description

### Parameters
- System.String command
- System.String args
### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.String]
---
## EncryptFile
### Description

### Parameters
- System.String path
### Returns
- System.Boolean
---
## DecryptFile
### Description

### Parameters
- System.String path
### Returns
- System.Boolean
---
## Encrypt
### Description

### Parameters
- System.Byte[] inputBytes
- System.String password
- System.Byte[] salt
### Returns
- System.Byte[]
---
## Decrypt
### Description

### Parameters
- System.Byte[] encryptedData
- System.String password
- System.Byte[] salt
### Returns
- System.Byte[]
---
## IPFSUpload
### Description

### Parameters
- System.String filePath
### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.String]
---
## IPFSDownload
### Description

### Parameters
- System.String cid
- System.String filePath
### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.Boolean]
---
## GenerateObj
### Description

### Parameters
- System.String path
### Returns
- UnityEngine.Transform
---
## RegisterObject
### Description

### Returns
- System.Void
---
## ReadYaml
### Description

### Parameters
- System.String path
### Returns
- System.Collections.Generic.Dictionary`2[System.String,System.Object]
---
## ReadYamlText
### Description

### Parameters
- System.String input
### Returns
- System.Collections.Generic.Dictionary`2[System.String,System.Object]
---
## WriteYaml
### Description

### Parameters
- System.String path
- System.Collections.Generic.Dictionary`2[System.String
- System.Object] data
### Returns
- System.Void
---
## GenerateComponent
### Description

### Parameters
- System.String path
- UnityEngine.Transform root
- System.Collections.Generic.Dictionary`2[System.String
- System.Object] custom
### Returns
- System.Void
---
## GetComponentExtensions
### Description

### Returns
- System.String[]
---
## ShortMessage
### Description

### Parameters
- System.String message
### Returns
- Cysharp.Threading.Tasks.UniTask
---
## ShortMessageAsync
### Description

### Parameters
- System.String args
### Returns
- System.Collections.IEnumerator
---
## ShowContentImporter
### Description

### Returns
- System.Void
---
## ShowContentManager
### Description

### Returns
- System.Void
---
## ReadQRCode
### Description

### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.String]
---
## ShowQRCodeReader
### Description

### Returns
- System.Void
---
## ShowWorldSelector
### Description

### Returns
- System.Void
---
## UpdateWorldID
### Description

### Parameters
- System.String worldID
### Returns
- System.Void
---
## ShowSettings
### Description

### Returns
- System.Void
---
## ShowComponentManager
### Description

### Parameters
- UnityEngine.Transform target
### Returns
- System.Void
---
## GenerateWorld
### Description

### Parameters
- System.String cid
### Returns
- System.Void
---
## DownloadContent
### Description

### Parameters
- System.String cid
### Returns
- Cysharp.Threading.Tasks.UniTask`1[System.String]
---
