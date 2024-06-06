package ipfs

import (
	"archive/tar"
	"archive/zip"
	"compress/gzip"
	"fmt"
	"io"
	"io/ioutil"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
)

const (
	ipfsVersionsUrl     = "https://dist.ipfs.tech/kubo/versions"
	ipfsZipPath         = "ipfs.zip"
	ipfsTarGzPath       = "ipfs.tar.gz"
	ipfsPath            = ".fw/ipfs"
	ipfsVersionFilePath = "ipfs_version.txt"
)

var (
	savePath              = ipfsPath
	ipfsDownloadUrlFormat string
	ipfsExecutablePath    string
)

func init() {
	switch runtime.GOOS {
	case "windows":
		ipfsDownloadUrlFormat = "https://dist.ipfs.tech/kubo/%s/kubo_%s_windows-amd64.zip"
		ipfsExecutablePath = filepath.Join(savePath, "kubo", "ipfs.exe")
	case "linux":
		ipfsDownloadUrlFormat = "https://dist.ipfs.tech/kubo/%s/kubo_%s_linux-amd64.tar.gz"
		ipfsExecutablePath = filepath.Join(savePath, "kubo", "ipfs")
	case "darwin":
		ipfsDownloadUrlFormat = "https://dist.ipfs.tech/kubo/%s/kubo_%s_darwin-amd64.tar.gz"
		ipfsExecutablePath = filepath.Join(savePath, "kubo", "ipfs")
	default:
		fmt.Println("Unsupported OS")
		os.Exit(1)
	}
}

func InstallIPFS() error {
	installedVersion := getInstalledVersion()
	latestVersion, err := getLatestStableVersion()
	if err != nil {
		fmt.Println("[IPFS] Failed to get the latest version:", err)
		return err
	}

	if !isIpfsInstalled() || installedVersion != latestVersion {
		if err := downloadIpfs(latestVersion); err != nil {
			fmt.Println("[IPFS] Failed to download IPFS:", err)
			return err
		}
		saveInstalledVersion(latestVersion)
		fmt.Println("[IPFS] IPFS installation complete.")
	} else {
		fmt.Println("[IPFS] Already installed.")
		return nil
	}

	// IPFSの初期化を行う
	cmd := exec.Command(ipfsExecutablePath, "init")
	output, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("[IPFS] Init error: %v, %s", err, string(output))
	}

	fmt.Println("[IPFS] Init output:", string(output))
	return nil
}

func isIpfsInstalled() bool {
	if _, err := os.Stat(ipfsExecutablePath); os.IsNotExist(err) {
		return false
	}
	return true
}

func getInstalledVersion() string {
	versionFilePath := filepath.Join(savePath, ipfsVersionFilePath)
	if _, err := os.Stat(versionFilePath); os.IsNotExist(err) {
		return ""
	}
	version, err := ioutil.ReadFile(versionFilePath)
	if err != nil {
		fmt.Println("[IPFS] Failed to read installed version:", err)
		return ""
	}
	return strings.TrimSpace(string(version))
}

func saveInstalledVersion(version string) {
	versionFilePath := filepath.Join(savePath, ipfsVersionFilePath)
	err := ioutil.WriteFile(versionFilePath, []byte(version), 0644)
	if err != nil {
		fmt.Println("[IPFS] Failed to save installed version:", err)
	}
}

func getLatestStableVersion() (string, error) {
	fmt.Println("[IPFS] Getting latest stable version...")
	resp, err := http.Get(ipfsVersionsUrl)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	versions := strings.Split(string(body), "\n")
	for i := len(versions) - 1; i >= 0; i-- {
		version := strings.TrimSpace(versions[i])
		if version != "" && !strings.Contains(version, "-rc") {
			return version, nil
		}
	}
	return "", fmt.Errorf("no stable version found")
}

func downloadIpfs(version string) error {
	downloadUrl := fmt.Sprintf(ipfsDownloadUrlFormat, version, version)
	var filePath string
	if runtime.GOOS == "windows" {
		filePath = ipfsZipPath
	} else {
		filePath = ipfsTarGzPath
	}
	zipPath := filepath.Join(".", filePath)

	fmt.Println("[IPFS] Downloading IPFS", version, "...")
	resp, err := http.Get(downloadUrl)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	out, err := os.Create(zipPath)
	if err != nil {
		return err
	}
	defer out.Close()

	_, err = io.Copy(out, resp.Body)
	if err != nil {
		return err
	}
	fmt.Println("[IPFS] IPFS downloaded.")

	if err := os.MkdirAll(savePath, 0755); err != nil {
		return err
	}

	fmt.Println("[IPFS] Extracting IPFS...")
	if runtime.GOOS == "windows" {
		err = unzip(zipPath, savePath)
	} else {
		err = untar(zipPath, savePath)
	}
	if err != nil {
		return err
	}
	fmt.Println("[IPFS] IPFS extracted.")

	os.Remove(zipPath)

	return nil
}

func unzip(src, dest string) error {
	r, err := zip.OpenReader(src)
	if err != nil {
		return err
	}
	defer r.Close()

	for _, f := range r.File {
		fpath := filepath.Join(dest, f.Name)

		if f.FileInfo().IsDir() {
			os.MkdirAll(fpath, os.ModePerm)
			continue
		}

		if err := os.MkdirAll(filepath.Dir(fpath), os.ModePerm); err != nil {
			return err
		}

		outFile, err := os.OpenFile(fpath, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, f.Mode())
		if err != nil {
			return err
		}

		rc, err := f.Open()
		if err != nil {
			return err
		}

		_, err = io.Copy(outFile, rc)

		outFile.Close()
		rc.Close()

		if err != nil {
			return err
		}
	}
	return nil
}

func untar(src, dest string) error {
	file, err := os.Open(src)
	if err != nil {
		return err
	}
	defer file.Close()

	gzr, err := gzip.NewReader(file)
	if err != nil {
		return err
	}
	defer gzr.Close()

	tr := tar.NewReader(gzr)
	for {
		header, err := tr.Next()
		if err == io.EOF {
			break
		}
		if err != nil {
			return err
		}

		target := filepath.Join(dest, header.Name)
		switch header.Typeflag {
		case tar.TypeDir:
			if err := os.MkdirAll(target, os.ModePerm); err != nil {
				return err
			}
		case tar.TypeReg:
			if err := os.MkdirAll(filepath.Dir(target), os.ModePerm); err != nil {
				return err
			}
			outFile, err := os.Create(target)
			if err != nil {
				return err
			}
			if _, err := io.Copy(outFile, tr); err != nil {
				outFile.Close()
				return err
			}
			outFile.Close()
		}
	}
	return nil
}
