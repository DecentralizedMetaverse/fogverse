package ipfs

import (
	"fmt"
	"os/exec"
	"regexp"
)

func InitIPFS() error {
	fmt.Println("[IPFS] Init")
	// installを開始する
	err := InstallIPFS()
	if err != nil {
		fmt.Println("[IPFS] Install error:", err)
		return err
	}
	return nil
}

func Upload(filePath string) (string, error) {
	fmt.Println("[IPFS] Upload:", filePath)

	cmd := exec.Command(ipfsExecutablePath, "add", filePath)
	output, err := cmd.CombinedOutput()
	if err != nil {
		return "", fmt.Errorf("[IPFS] Add error: %v, %s", err, string(output))
	}
	fmt.Println("[IPFS] Add output:", string(output))

	re := regexp.MustCompile(`added (\S+)`)
	matches := re.FindStringSubmatch(string(output))
	if len(matches) < 2 {
		return "", fmt.Errorf("[IPFS] Unexpected output: %s", output)
	}
	return matches[1], nil
}

func Download(cid, filePath string) error {
	fmt.Println("[IPFS] Download:", cid, filePath)

	cmd := exec.Command(ipfsExecutablePath, "get", cid, "-o", filePath)
	output, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("[IPFS] Get error: %v, %s", err, string(output))
	}

	fmt.Println("[IPFS] Get output:", string(output))

	return nil
}
