using UnityEngine;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using DC;

public class IpfsInstaller : MonoBehaviour
{
   private const string IpfsVersionsUrl = "https://dist.ipfs.tech/kubo/versions";
   private const string IpfsDownloadUrlFormat = "https://dist.ipfs.tech/kubo/{0}/kubo_{0}_windows-amd64.zip";
   private const string IpfsZipPath = "ipfs.zip";
   private const string IpfsPath = "ipfs";
   private static readonly string SavePath = $"{Application.dataPath}/../{IpfsPath}";
   private const string IpfsVersionFilePath = "ipfs_version.txt";

   private async void Start()
   {
       string installedVersion = GetInstalledVersion();
       string latestVersion = await GetLatestStableVersion();

       if (!IsIpfsInstalled() || installedVersion != latestVersion)
       {
           await DownloadIpfs(latestVersion);
           SaveInstalledVersion(latestVersion);
           GM.Msg("IPFSInit");
       }
       else
       {
           Debug.Log("[IPFS] Already installed.");
       }
   }

   private bool IsIpfsInstalled()
   {
       string ipfsExecutablePath = $"{SavePath}/kubo/ipfs.exe";
       return File.Exists(ipfsExecutablePath);
   }

   private string GetInstalledVersion()
   {
       var versionFilePath = Path.Combine(SavePath, IpfsVersionFilePath);
       if (File.Exists(versionFilePath))
       {
           return File.ReadAllText(versionFilePath).Trim();
       }
       return null;
   }

   private void SaveInstalledVersion(string version)
   {
       string versionFilePath = Path.Combine(SavePath, IpfsVersionFilePath);
       File.WriteAllText(versionFilePath, version);
   }

   private async UniTask<string> GetLatestStableVersion()
   {
       Debug.Log("[IPFS] Getting latest stable version...");
       using var webClient = new WebClient();
       try
       {
           var versionsContent = await webClient.DownloadStringTaskAsync(IpfsVersionsUrl);
           var versions = versionsContent.Split('\n');
           for (int i = versions.Length - 1; i >= 0; i--)
           {
               var version = versions[i].Trim();
               if (!string.IsNullOrEmpty(version) && !version.Contains("-rc"))
               {
                   return version;
               }
           }
       }
       catch (WebException ex)
       {
           Debug.LogError($"[IPFS] Failed to download IPFS versions: {ex.Message}");
       }

       return null;
   }

   private async UniTask DownloadIpfs(string version)
   {
       string downloadUrl = string.Format(IpfsDownloadUrlFormat, version);
       string zipPath = Path.Combine(Application.dataPath, IpfsZipPath);

       using (var webClient = new WebClient())
       {
           Debug.Log($"[IPFS] Downloading IPFS {version}...");
           await webClient.DownloadFileTaskAsync(downloadUrl, zipPath);
           Debug.Log("[IPFS] IPFS downloaded.");
       }

       if (!Directory.Exists(SavePath))
       {
           Directory.CreateDirectory(SavePath);
       }

       Debug.Log("[IPFS] Extracting IPFS...");
       System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, SavePath);
       Debug.Log("[IPFS] IPFS extracted.");

       File.Delete(zipPath);

       Debug.Log("[IPFS] IPFS installation complete.");
   }
}
