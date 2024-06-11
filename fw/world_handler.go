package main

import (
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"fw/ipfs"
	"io"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"

	"github.com/google/uuid"
	"gopkg.in/yaml.v2"
)

func handleGetWorldData(args []string) {
	headData, err := memFS.ReadFile(".fw/HEAD")
	if err != nil {
		fmt.Printf("[World] Error reading HEAD: %v\n", err)
		return
	}

	var headYamlData HeadData
	err = yaml.Unmarshal(headData, &headYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing HEAD: %v\n", err)
		return
	}

	worldDataStr, err := memFS.ReadFile(filepath.Join(".fw", "worlds", headYamlData.Guid))
	if err != nil {
		fmt.Printf("[World] Error reading world data: %v\n", err)
		return
	}

	var worldYamlData WorldData
	err = yaml.Unmarshal(worldDataStr, &worldYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		return
	}

	err = loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		return
	}

	worldAllData := make([]WorldSingleData, 0)
	for _, cid := range worldYamlData.CID {
		metaFilePath := filepath.Join(".fw", "objects", cid)
		file, err := memFS.ReadFile(metaFilePath)
		if err != nil {
			fmt.Printf("[World] Error reading file: %v\n", err)
			return
		}

		decryptedData, err := decryptData(file)
		if err != nil {
			fmt.Printf("[World] Error decrypting world data: %v\n", err)
			return
		}

		decompressedData, err := decompressData(decryptedData)
		if err != nil {
			fmt.Printf("[World] Error decompressing world data: %v\n", err)
			return
		}
		var worldSingleData WorldSingleData
		err = yaml.Unmarshal(decompressedData, &worldSingleData)
		if err != nil {
			fmt.Printf("[World] Error parsing world data: %v\n", err)
			return
		}
		worldAllData = append(worldAllData, worldSingleData)
	}

	savePath := filepath.Join(".fw", "content", headYamlData.CurrentWorld+".yaml")
	worldAllDataYaml, err := yaml.Marshal(&worldAllData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		return
	}
	err = memFS.WriteFile(savePath, worldAllDataYaml)
	if err != nil {
		fmt.Printf("[World] Error saving world data: %v\n", err)
		return
	}
}

func handleDownloadWorld(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw download-world <cid>")
		return
	}

	cid := args[0]
	worldPath := filepath.Join(".fw", "objects", cid)
	// Simulate IPFS Download
	// err := ipfs.Download(cid, worldPath)
	err := ipfs.Download(cid, worldPath)
	if err != nil {
		fmt.Printf("[World] Error downloading world data: %v\n", err)
		return
	}

	err = loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		return
	}

	worldData, err := memFS.ReadFile(worldPath)
	if err != nil {
		fmt.Printf("[World] Error reading world data: %v\n", err)
		return
	}

	decryptedData, err := decryptData(worldData)
	if err != nil {
		fmt.Printf("[World] Error decrypting world data: %v\n", err)
		return
	}

	decompressedData, err := decompressData(decryptedData)
	if err != nil {
		fmt.Printf("[World] Error decompressing world data: %v\n", err)
		return
	}

	var worldYamlData WorldData
	err = yaml.Unmarshal(decompressedData, &worldYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		return
	}

	newWorldPath := filepath.Join(".fw", "worlds", worldYamlData.GUID)

	err = memFS.WriteFile(newWorldPath, decompressedData)
	if err != nil {
		fmt.Printf("[World] Error saving world data: %v\n", err)
		return
	}
}

func handleGetWorldCID(args []string) {
	headData, err := memFS.ReadFile(".fw/HEAD")
	if err != nil {
		fmt.Printf("[World] Error reading HEAD: %v\n", err)
		return
	}

	var headYamlData HeadData
	err = yaml.Unmarshal(headData, &headYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing HEAD: %v\n", err)
		return
	}
	guid := headYamlData.Guid
	filePath := filepath.Join(".fw", "worlds", guid)

	file, err := memFS.ReadFile(filePath)
	if err != nil {
		fmt.Printf("[World] Error opening file %s: %v\n", filePath, err)
		return
	}

	hash := sha256.Sum256(file)
	hashString := hex.EncodeToString(hash[:])

	compressedData, err := compressData(file)
	if err != nil {
		fmt.Printf("[World] Error compressing file: %v\n", err)
		return
	}

	err = loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		return
	}

	encryptedData, err := encryptData(compressedData)
	if err != nil {
		fmt.Printf("[World] Error encrypting file: %v\n", err)
		return
	}

	objectPath := filepath.Join(".fw", "objects", hashString)
	err = memFS.WriteFile(objectPath, encryptedData)
	if err != nil {
		fmt.Printf("[World] Error saving file %s: %v\n", objectPath, err)
		return
	}

	// Simulate IPFS Upload
	cid, err := ipfs.Upload(objectPath)
	if err != nil {
		fmt.Printf("[World] Error uploading file to IPFS: %v\n", err)
		return
	}

	fmt.Printf("[World] World data uploaded to IPFS with CID: %s\n", cid)
}

func handleSwitch(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw create <world-name>")
		return
	}

	worldName := args[0]

	fmt.Println("[World] Creating new world...")

	worldListPath := filepath.Join(".fw", "world_list")
	worldDict, err := loadWorldList(worldListPath)
	if err != nil {
		fmt.Printf("[World] Error reading world list: %v\n", err)
		return
	}

	if _, exists := worldDict[worldName]; exists {
		fmt.Printf("[World] Error: World %s already exists\n", worldName)
		return
	}

	guid, err := uuid.NewUUID()
	if err != nil {
		fmt.Printf("[World] Error generating GUID: %v\n", err)
		return
	}

	worldGuid := guid.String()
	err = updateHead(worldName, worldGuid)
	if err != nil {
		fmt.Printf("[World] Error updating HEAD: %v\n", err)
		return
	}

	err = createWorldFile(worldName, worldGuid)
	if err != nil {
		fmt.Printf("[World] Error creating world file: %v\n", err)
		return
	}

	worldDict[worldName] = worldGuid
	err = saveWorldList(worldListPath, worldDict)
	if err != nil {
		fmt.Printf("[World] Error updating world list: %v\n", err)
		return
	}
}

func loadWorldList(worldListPath string) (map[string]string, error) {
	worldListData, err := memFS.ReadFile(worldListPath)
	if err != nil {
		return nil, err
	}

	worldList := strings.Split(string(worldListData), "\n")
	worldDict := make(map[string]string)
	for _, line := range worldList {
		if line == "" {
			continue
		}
		kv := strings.Split(line, ": ")
		worldDict[kv[0]] = kv[1]
	}
	return worldDict, nil
}

func updateHead(worldName, worldGuid string) error {
	headPath := filepath.Join(".fw", "HEAD")
	headContent := fmt.Sprintf("currentWorld: %s\nguid: %s\n", worldName, worldGuid)
	return memFS.WriteFile(headPath, []byte(headContent))
}

func createWorldFile(worldName, worldGuid string) error {
	worldPath := filepath.Join(".fw", "worlds", worldGuid)
	worldMeta := fmt.Sprintf("name: %s\nguid: %s\ncid: %s\n", worldName, worldGuid, "")
	return memFS.WriteFile(worldPath, []byte(worldMeta))
}

func saveWorldList(worldListPath string, worldDict map[string]string) error {
	var newWorldData []byte
	for k, v := range worldDict {
		newWorldData = append(newWorldData, []byte(fmt.Sprintf("%s: %s\n", k, v))...)
	}
	return memFS.WriteFile(worldListPath, newWorldData)
}

func handleGet(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw get <cid>")
		return
	}

	err := loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		return
	}

	cid := args[0]
	metaDataPath := filepath.Join(".fw", "objects", cid)

	// Simulate IPFS Download
	err = ipfs.Download(cid, metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error downloading metadata: %v\n", err)
		return
	}

	encryptedMetaData, err := memFS.ReadFile(metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error reading metadata %s: %v\n", metaDataPath, err)
		return
	}

	decryptedMetaData, err := decryptData(encryptedMetaData)
	if err != nil {
		fmt.Printf("[World] Error decrypting metadata %s: %v\n", metaDataPath, err)
		return
	}

	decompressedMetaData, err := decompressData(decryptedMetaData)
	if err != nil {
		fmt.Printf("[World] Error decompressing metadata %s: %v\n", metaDataPath, err)
		return
	}

	metaDataContent := string(decompressedMetaData)
	fileCid, fileName, err := extractFileDetailsFromMetaData(metaDataContent)
	if err != nil {
		fmt.Printf("[World] Error extracting file details from metadata %s: %v\n", metaDataPath, err)
		return
	}

	filePath := filepath.Join(".fw", "content", filepath.Base(fileName))
	// Simulate IPFS Download
	err = ipfs.Download(fileCid, filePath)
	if err != nil {
		fmt.Printf("[World] Error downloading file from IPFS: %v\n", err)
		return
	}

	encryptedFileData, err := memFS.ReadFile(filePath)
	if err != nil {
		fmt.Printf("[World] Error reading file %s: %v\n", filePath, err)
		return
	}

	decryptedFileData, err := decryptData(encryptedFileData)
	if err != nil {
		fmt.Printf("[World] Error decrypting file %s: %v\n", filePath, err)
		return
	}

	decompressedFileData, err := decompressData(decryptedFileData)
	if err != nil {
		fmt.Printf("[World] Error decompressing file %s: %v\n", filePath, err)
		return
	}

	err = memFS.WriteFile(filePath, decompressedFileData)
	if err != nil {
		fmt.Printf("[World] Error saving decompressed file %s: %v\n", filePath, err)
		return
	}

	fmt.Printf("[World] File downloaded, decrypted, and saved to %s\n", filePath)
}

func handlePut(args []string) {
	if len(args) < 10 {
		fmt.Println("[World] Usage: fw put <file> <x> <y> <z> <rx> <ry> <rz> <sx> <sy> <sz>")
		return
	}

	if err := loadPassword(); err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		return
	}

	filePath, err := filepath.Abs(args[0])
	if err != nil {
		fmt.Printf("[World] Invalid file path: %v\n", err)
		return
	}

	coords, err := parseCoordinates(args[1:])
	if err != nil {
		fmt.Printf("[World] Invalid input: %v\n", err)
		return
	}

	fmt.Printf("[World] Putting world binary data from %s with coordinates (%f, %f, %f), rotation (%f, %f, %f), scale (%f, %f, %f)\n",
		filePath, coords[0], coords[1], coords[2], coords[3], coords[4], coords[5], coords[6], coords[7], coords[8])

	file, err := os.Open(filePath)
	if err != nil {
		fmt.Printf("[World] Error opening file %s: %v\n", filePath, err)
		return
	}
	defer file.Close()

	hash := sha256.New()
	if _, err := io.Copy(hash, file); err != nil {
		fmt.Printf("[World] Error hashing file: %v\n", err)
		return
	}
	file.Seek(0, 0)
	hashString := hex.EncodeToString(hash.Sum(nil))

	byteArray, err := ioutil.ReadAll(file)
	if err != nil {
		fmt.Printf("[World] Error reading file: %v\n", err)
		return
	}

	compressedData, err := compressData(byteArray)
	if err != nil {
		fmt.Printf("[World] Error compressing file: %v\n", err)
		return
	}

	encryptedData, err := encryptData(compressedData)
	if err != nil {
		fmt.Printf("[World] Error encrypting file: %v\n", err)
		return
	}

	objectPath := filepath.Join(".fw", "objects", hashString)
	err = memFS.WriteFile(objectPath, encryptedData)
	if err != nil {
		fmt.Printf("[World] Error saving file %s: %v\n", objectPath, err)
		return
	}

	// Simulate IPFS Upload
	cid, err := ipfs.Upload(objectPath)
	if err != nil {
		fmt.Printf("[World] Error uploading file to IPFS: %v\n", err)
		return
	}

	err = memFS.Rename(objectPath, filepath.Join(".fw", "objects", cid))
	if err != nil {
		fmt.Printf("[World] Error renaming file: %v\n", err)
		return
	}

	metaDataContent := generateMetaDataContent(filePath, cid, coords)
	encryptedMetaData, err := encryptAndCompressMetaData(metaDataContent)
	if err != nil {
		fmt.Printf("[World] Error processing metadata: %v\n", err)
		return
	}

	metaDataPath := filepath.Join(".fw", "objects", hashString)
	err = memFS.WriteFile(metaDataPath, encryptedMetaData)
	if err != nil {
		fmt.Printf("[World] Error creating metadata file %s: %v\n", metaDataPath, err)
		return
	}

	metaCid, err := ipfs.Upload(metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error uploading metadata to IPFS: %v\n", err)
		return
	}

	err = memFS.Rename(metaDataPath, filepath.Join(".fw", "objects", metaCid))
	if err != nil {
		fmt.Printf("[World] Error renaming metadata file: %v\n", err)
		return
	}

	fmt.Printf("[World] File saved as %s\n", filepath.Join(".fw", "objects", cid))
	fmt.Printf("[World] Metadata saved as %s\n", filepath.Join(".fw", "objects", metaCid))

	err = updateWorldData(metaCid)
	if err != nil {
		fmt.Printf("[World] Error updating world data: %v\n", err)
		return
	}

	fmt.Println("[World] World data updated successfully.")
}

func handleSetPassword(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw set-password <password>")
		return
	}

	password := args[0]
	hashedPassword := sha256.Sum256([]byte(password))
	key = hashedPassword[:]

	err := memFS.WriteFile(".fw/password", []byte(hex.EncodeToString(key)))
	if err != nil {
		fmt.Printf("[World] Error saving password: %v\n", err)
		return
	}

	fmt.Println("[World] Password set successfully.")
}

func updateWorldData(metaCid string) error {
	headData, err := memFS.ReadFile(".fw/HEAD")
	if err != nil {
		return err
	}

	var headYamlData HeadData
	err = yaml.Unmarshal(headData, &headYamlData)
	if err != nil {
		return err
	}

	worldDataPath := filepath.Join(".fw", "worlds", headYamlData.Guid)
	worldDataStr, err := memFS.ReadFile(worldDataPath)
	if err != nil {
		return err
	}

	var worldData WorldData
	err = yaml.Unmarshal(worldDataStr, &worldData)
	if err != nil {
		return err
	}

	worldData.CID = append(worldData.CID, metaCid)

	newWorldData, err := yaml.Marshal(&worldData)
	if err != nil {
		return err
	}

	return memFS.WriteFile(worldDataPath, newWorldData)
}

func handleInit(args []string) {
	if _, err := memFS.Stat(".fw"); err == nil {
		fmt.Println("[World] .fw repository already exists.")
		return
	}

	fmt.Println("[World] .fw repository initialized.")

	directories := []string{
		".fw",
		".fw/objects",
		".fw/worlds",
		".fw/worlds/heads",
		".fw/worlds/tags",
		".fw/content",
	}

	files := map[string]string{
		".fw/HEAD":        "",
		".fw/world_list":  "",
		".fw/config":      "[core]\n\trepositoryformatversion = 0\n\tfilemode = true\n\tbare = false\n",
		".fw/description": "Unnamed repository; edit this file 'description' to name the repository.\n",
	}

	for _, dir := range directories {
		memFS.MkdirAll(dir)
	}
	for file, content := range files {
		memFS.WriteFile(file, []byte(content))
	}
}

func handleCat(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw cat <path>")
		return
	}

	filePath := args[0]
	content, err := memFS.ReadFile(filePath)
	if err != nil {
		fmt.Printf("[World] Error reading file %s: %v\n", filePath, err)
		return
	}

	fmt.Printf("[World] Content of %s:\n%s\n", filePath, string(content))
}
