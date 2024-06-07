package main

import (
	"bytes"
	"compress/gzip"
	"crypto/aes"
	"crypto/cipher"
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"fw/ipfs"
	"github.com/google/uuid"
	"gopkg.in/yaml.v2"
	"io"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

type Command struct {
	Name string
	Args []string
}

type CommandHandler func(args []string)

type WorldData struct {
	Name string   `yaml:"name"`
	GUID string   `yaml:"guid"`
	CID  []string `yaml:"cid"`
}
type WorldSingleData struct {
	CID  string  `yaml:"cid"`
	File string  `yaml:"file"`
	X    float64 `yaml:"x"`
	Y    float64 `yaml:"y"`
	Z    float64 `yaml:"z"`
	RX   float64 `yaml:"rx"`
	RY   float64 `yaml:"ry"`
	RZ   float64 `yaml:"rz"`
	SX   float64 `yaml:"sx"`
	SY   float64 `yaml:"sy"`
	SZ   float64 `yaml:"sz"`
}
type HeadData struct {
	CurrentWorld string `yaml:"currentWorld"`
	Guid         string `yaml:"guid"`
}

var key []byte

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: fw <command>")
		os.Exit(1)
	}

	cmd := Command{
		Name: os.Args[1],
		Args: os.Args[2:],
	}

	commandHandler := map[string]CommandHandler{
		"init":           handleInit,          // create directory
		"switch":         handleSwitch,        // create world
		"get":            handleGet,           // metaCIDからファイルを取得
		"put":            handlePut,           // put world binary data
		"set-password":   handleSetPassword,   // set encryption password
		"cat":            handleCat,           // view encrypted file content
		"get-world-cid":  handleGetWorldCID,   // get world data
		"download-world": handleDownloadWorld, // download world data
		"get-world-data": handleGetWorldData,  // get world data
	}

	if handler, found := commandHandler[cmd.Name]; found {
		handler(cmd.Args)
	} else {
		fmt.Printf("[World] Unknown command: %s\n", cmd.Name)
		os.Exit(1)
	}
}

func handleGetWorldData(args []string) {
	// headから読み込み
	headPath := filepath.Join(".fw", "HEAD")
	headData, err := os.ReadFile(headPath)
	if err != nil {
		fmt.Printf("[World] Error reading HEAD: %v\n", err)
		os.Exit(1)
	}

	var headYamlData HeadData
	err = yaml.Unmarshal(headData, &headYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing HEAD: %v\n", err)
		os.Exit(1)
	}

	// worldData読み込み
	worldDataPath := filepath.Join(".fw", "worlds", headYamlData.Guid)
	worldDataStr, err := os.ReadFile(worldDataPath)
	if err != nil {
		fmt.Printf("[World] Error reading world data: %v\n", err)
		os.Exit(1)
	}

	var worldYamlData WorldData
	err = yaml.Unmarshal(worldDataStr, &worldYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		os.Exit(1)
	}

	err = loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	// metaFileを全て取得
	worldAllData := make([]WorldSingleData, 0)
	metaFilePath := ""
	for _, cid := range worldYamlData.CID {
		metaFilePath = filepath.Join(".fw", "objects", cid)
		file, err := os.Open(metaFilePath)
		byteArray, err := readFile(file)
		if err != nil {
			fmt.Printf("[World] Error reading file: %v\n", err)
			os.Exit(1)
		}

		decryptedData, err := decryptData(byteArray)
		if err != nil {
			fmt.Printf("[World] Error decrypting world data: %v\n", err)
			os.Exit(1)
		}

		decompressedData, err := decompressData(decryptedData)
		if err != nil {
			fmt.Printf("[World] Error decompressing world data: %v\n", err)
			os.Exit(1)
		}
		var worldSingleData WorldSingleData
		err = yaml.Unmarshal(decompressedData, &worldSingleData)
		if err != nil {
			fmt.Printf("[World] Error parsing world data: %v\n", err)
			os.Exit(1)
		}
		worldAllData = append(worldAllData, worldSingleData)
	}

	fmt.Println(worldAllData)
	// .fw/contentにyamlで保存
	savePath := filepath.Join(".fw", "content", headYamlData.CurrentWorld+".yaml")
	worldAllDataYaml, err := yaml.Marshal(&worldAllData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		os.Exit(1)

	}
	err = os.WriteFile(savePath, worldAllDataYaml, 0644)
}

func handleDownloadWorld(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw download-world <cid>")
		os.Exit(1)
	}

	cid := args[0]
	worldPath := filepath.Join(".fw", "objects", cid)
	err := ipfs.Download(cid, worldPath)
	if err != nil {
		fmt.Printf("[World] Error downloading world data: %v\n", err)
		os.Exit(1)
	}

	// 復号化, 解凍
	err = loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	worldData, err := os.ReadFile(worldPath)
	if err != nil {
		fmt.Printf("[World] Error reading world data: %v\n", err)
		os.Exit(1)
	}

	decryptedData, err := decryptData(worldData)
	if err != nil {
		fmt.Printf("[World] Error decrypting world data: %v\n", err)
		os.Exit(1)
	}

	decompressedData, err := decompressData(decryptedData)
	if err != nil {
		fmt.Printf("[World] Error decompressing world data: %v\n", err)
		os.Exit(1)
	}

	var worldYamlData WorldData
	err = yaml.Unmarshal(decompressedData, &worldYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing world data: %v\n", err)
		os.Exit(1)
	}

	newWorldPath := filepath.Join(".fw", "worlds", worldYamlData.GUID)

	// 保存
	err = os.WriteFile(newWorldPath, decompressedData, 0644)
	if err != nil {
		fmt.Printf("[World] Error saving world data: %v\n", err)
		os.Exit(1)
	}
}

func handleGetWorldCID(args []string) {
	// worldのcidを取得する
	// guid fileをipfsに保存し、そのcidを返す
	// head fileからguidを取得
	headPath := filepath.Join(".fw", "HEAD")
	headData, err := os.ReadFile(headPath)
	if err != nil {
		fmt.Printf("[World] Error reading HEAD: %v\n", err)
		os.Exit(1)
	}
	// yaml読み込み
	// guid取得
	var headYamlData HeadData
	err = yaml.Unmarshal(headData, &headYamlData)
	if err != nil {
		fmt.Printf("[World] Error parsing HEAD: %v\n", err)
		os.Exit(1)
	}
	guid := headYamlData.Guid
	filePath := filepath.Join(".fw", "worlds", guid)

	file, err := os.Open(filePath)
	if err != nil {
		fmt.Printf("[World] Error opening file %s: %v\n", filePath, err)
		os.Exit(1)
	}
	defer file.Close()

	hash, err := generateFileHash(file)
	if err != nil {
		fmt.Printf("[World] Error hashing file: %v\n", err)
		os.Exit(1)
	}

	byteArray, err := readFile(file)
	if err != nil {
		fmt.Printf("[World] Error reading file: %v\n", err)
		os.Exit(1)
	}

	compressedData, err := compressData(byteArray)
	if err != nil {
		fmt.Printf("[World] Error compressing file: %v\n", err)
		os.Exit(1)
	}

	if err := loadPassword(); err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	encryptedData, err := encryptData(compressedData)
	if err != nil {
		fmt.Printf("[World] Error encrypting file: %v\n", err)
		os.Exit(1)
	}

	objectPath := filepath.Join(".fw", "objects", hash)
	if err := saveFile(objectPath, encryptedData); err != nil {
		fmt.Printf("[World] Error saving file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	cid, err := ipfs.Upload(objectPath)
	if err != nil {
		fmt.Printf("[World] Error uploading file to IPFS: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("[World] World data uploaded to IPFS with CID: %s\n", cid)
}

func handleSetPassword(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw set-password <password>")
		os.Exit(1)
	}

	password := args[0]
	hashedPassword := sha256.Sum256([]byte(password))
	key = hashedPassword[:]

	err := os.WriteFile(".fw/password", []byte(hex.EncodeToString(key)), 0644)
	if err != nil {
		fmt.Printf("[World] Error saving password: %v\n", err)
		os.Exit(1)
	}

	fmt.Println("[World] Password set successfully.")
}

func loadPassword() error {
	data, err := os.ReadFile(".fw/password")
	if err != nil {
		return err
	}

	key, err = hex.DecodeString(string(data))
	if err != nil {
		return err
	}

	return nil
}

func handleInit(args []string) {
	if _, err := os.Stat(".fw"); err == nil {
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

	createDirectories(directories)
	createFiles(files)

	err := ipfs.InitIPFS()
	if err != nil {
		fmt.Printf("[World] Error initializing IPFS: %v\n", err)
		os.Exit(1)
	}
}

func createDirectories(directories []string) {
	for _, dir := range directories {
		absDir, err := filepath.Abs(dir)
		if err != nil {
			fmt.Printf("[World] Error creating directory %s: %v\n", dir, err)
			os.Exit(1)
		}
		err = os.MkdirAll(absDir, 0755)
		if err != nil {
			fmt.Printf("[World] Error creating directory %s: %v\n", absDir, err)
			os.Exit(1)
		}
	}
}

func createFiles(files map[string]string) {
	for file, content := range files {
		absFile, err := filepath.Abs(file)
		if err != nil {
			fmt.Printf("[World] Error creating file %s: %v\n", file, err)
			os.Exit(1)
		}
		err = os.WriteFile(absFile, []byte(content), 0644)
		if err != nil {
			fmt.Printf("[World] Error creating file %s: %v\n", absFile, err)
			os.Exit(1)
		}
	}
}

func handleSwitch(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw create <world-name>")
		os.Exit(1)
	}

	worldName := args[0]

	fmt.Println("[World] Creating new world...")

	worldListPath := filepath.Join(".fw", "world_list")
	worldDict, err := loadWorldList(worldListPath)
	if err != nil {
		fmt.Printf("[World] Error reading world list: %v\n", err)
		os.Exit(1)
	}

	if _, exists := worldDict[worldName]; exists {
		fmt.Printf("[World] Error: World %s already exists\n", worldName)
		os.Exit(1)
	}

	guid, err := uuid.NewUUID()
	if err != nil {
		fmt.Printf("[World] Error generating GUID: %v\n", err)
		os.Exit(1)
	}

	worldGuid := guid.String()
	err = updateHead(worldName, worldGuid)
	if err != nil {
		fmt.Printf("[World] Error updating HEAD: %v\n", err)
		os.Exit(1)
	}

	err = createWorldFile(worldName, worldGuid)
	if err != nil {
		fmt.Printf("[World] Error creating world file: %v\n", err)
		os.Exit(1)
	}

	worldDict[worldName] = worldGuid
	err = saveWorldList(worldListPath, worldDict)
	if err != nil {
		fmt.Printf("[World] Error updating world list: %v\n", err)
		os.Exit(1)
	}
}

func loadWorldList(worldListPath string) (map[string]string, error) {
	worldListData, err := os.ReadFile(worldListPath)
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
	return os.WriteFile(headPath, []byte(headContent), 0644)
}

func createWorldFile(worldName, worldGuid string) error {
	worldPath := filepath.Join(".fw", "worlds", worldGuid)
	worldMeta := fmt.Sprintf("name: %s\nguid: %s\ncid: %s\n", worldName, worldGuid, "")
	return os.WriteFile(worldPath, []byte(worldMeta), 0644)
}

func saveWorldList(worldListPath string, worldDict map[string]string) error {
	var newWorldData []byte
	for k, v := range worldDict {
		newWorldData = append(newWorldData, []byte(fmt.Sprintf("%s: %s\n", k, v))...)
	}
	return os.WriteFile(worldListPath, newWorldData, 0644)
}

func handlePut(args []string) {
	if len(args) < 10 {
		fmt.Println("[World] Usage: fw put <file> <x> <y> <z> <rx> <ry> <rz> <sx> <sy> <sz>")
		os.Exit(1)
	}

	if err := loadPassword(); err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	filePath, err := filepath.Abs(args[0])
	if err != nil {
		fmt.Printf("[World] Invalid file path: %v\n", err)
		os.Exit(1)
	}

	coords, err := parseCoordinates(args[1:])
	if err != nil {
		fmt.Printf("[World] Invalid input: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("[World] Putting world binary data from %s with coordinates (%f, %f, %f), rotation (%f, %f, %f), scale (%f, %f, %f)\n",
		filePath, coords[0], coords[1], coords[2], coords[3], coords[4], coords[5], coords[6], coords[7], coords[8])

	file, err := os.Open(filePath)
	if err != nil {
		fmt.Printf("[World] Error opening file %s: %v\n", filePath, err)
		os.Exit(1)
	}
	defer file.Close()

	hash, err := generateFileHash(file)
	if err != nil {
		fmt.Printf("[World] Error hashing file: %v\n", err)
		os.Exit(1)
	}

	byteArray, err := readFile(file)
	if err != nil {
		fmt.Printf("[World] Error reading file: %v\n", err)
		os.Exit(1)
	}

	compressedData, err := compressData(byteArray)
	if err != nil {
		fmt.Printf("[World] Error compressing file: %v\n", err)
		os.Exit(1)
	}

	encryptedData, err := encryptData(compressedData)
	if err != nil {
		fmt.Printf("[World] Error encrypting file: %v\n", err)
		os.Exit(1)
	}

	objectPath := filepath.Join(".fw", "objects", hash)
	if err := saveFile(objectPath, encryptedData); err != nil {
		fmt.Printf("[World] Error saving file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	cid, err := ipfs.Upload(objectPath)
	if err != nil {
		fmt.Printf("[World] Error uploading file to IPFS: %v\n", err)
		os.Exit(1)
	}

	if err := renameFile(objectPath, filepath.Join(".fw", "objects", cid)); err != nil {
		fmt.Printf("[World] Error renaming file: %v\n", err)
		os.Exit(1)
	}

	metaDataContent := generateMetaDataContent(filePath, cid, coords)
	encryptedMetaData, err := encryptAndCompressMetaData(metaDataContent)
	if err != nil {
		fmt.Printf("[World] Error processing metadata: %v\n", err)
		os.Exit(1)
	}

	metaDataPath := filepath.Join(".fw", "objects", hash)
	if err := saveFile(metaDataPath, encryptedMetaData); err != nil {
		fmt.Printf("[World] Error creating metadata file %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	metaCid, err := ipfs.Upload(metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error uploading metadata to IPFS: %v\n", err)
		os.Exit(1)
	}

	if err := renameFile(metaDataPath, filepath.Join(".fw", "objects", metaCid)); err != nil {
		fmt.Printf("[World] Error renaming metadata file: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("[World] File saved as %s\n", filepath.Join(".fw", "objects", cid))
	fmt.Printf("[World] Metadata saved as %s\n", filepath.Join(".fw", "objects", metaCid))

	err = updateWorldData(metaCid)
	if err != nil {
		fmt.Printf("[World] Error updating world data: %v\n", err)
		os.Exit(1)
	}

	fmt.Println("[World] World data updated successfully.")
}

func generateFileHash(file *os.File) (string, error) {
	hash := sha256.New()
	if _, err := io.Copy(hash, file); err != nil {
		return "", err
	}
	if _, err := file.Seek(0, 0); err != nil {
		return "", err
	}
	return hex.EncodeToString(hash.Sum(nil)), nil
}

func readFile(file *os.File) ([]byte, error) {
	return io.ReadAll(file)
}

func saveFile(path string, data []byte) error {
	return os.WriteFile(path, data, 0644)
}

func renameFile(oldPath, newPath string) error {
	return os.Rename(oldPath, newPath)
}

func generateMetaDataContent(filePath, cid string, coords []float64) string {
	fileName := filepath.Base(filePath)
	return fmt.Sprintf("file: %s\ncid: %s\nx: %f\ny: %f\nz: %f\nrx: %f\nry: %f\nrz: %f\nsx: %f\nsy: %f\nsz: %f\n",
		fileName, cid, coords[0], coords[1], coords[2], coords[3], coords[4], coords[5], coords[6], coords[7], coords[8])
}

func encryptAndCompressMetaData(metaDataContent string) ([]byte, error) {
	compressedMetaData, err := compressData([]byte(metaDataContent))
	if err != nil {
		return nil, err
	}

	return encryptData(compressedMetaData)
}

func updateWorldData(metaCid string) error {
	headPath := filepath.Join(".fw", "HEAD")
	headData, err := os.ReadFile(headPath)
	if err != nil {
		return err
	}

	worldGuid, err := extractWorldGuid(headData)
	if err != nil {
		return err
	}

	worldDataPath := filepath.Join(".fw", "worlds", worldGuid)
	worldDataStr, err := os.ReadFile(worldDataPath)
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

	return os.WriteFile(worldDataPath, newWorldData, 0644)
}

func extractWorldGuid(headData []byte) (string, error) {
	headLines := strings.Split(string(headData), "\n")
	for _, line := range headLines {
		if strings.HasPrefix(line, "guid: ") {
			return strings.TrimPrefix(line, "guid: "), nil
		}
	}
	return "", fmt.Errorf("world GUID not found")
}

func handleCat(args []string) {
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw cat <file-hash>")
		os.Exit(1)
	}

	err := loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	fileHash := args[0]
	objectPath := filepath.Join(".fw", "objects", fileHash)

	encryptedData, err := os.ReadFile(objectPath)
	if err != nil {
		fmt.Printf("[World] Error reading file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	decryptedData, err := decryptData(encryptedData)
	if err != nil {
		fmt.Printf("[World] Error decrypting file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	decompressedData, err := decompressData(decryptedData)
	if err != nil {
		fmt.Printf("[World] Error decompressing file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	fmt.Printf("[World] Decrypted and decompressed file content:\n%s\n", string(decompressedData))
}

func handleGet(args []string) {
	// metaDataを読み込むのはそこにfile名が書いてあるから
	if len(args) < 1 {
		fmt.Println("[World] Usage: fw get <cid>")
		os.Exit(1)
	}

	err := loadPassword()
	if err != nil {
		fmt.Printf("[World] Error loading password: %v\n", err)
		os.Exit(1)
	}

	cid := args[0]
	metaDataPath := filepath.Join(".fw", "objects", cid)

	err = ipfs.Download(cid, metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error downloading metadata: %v\n", err)
		os.Exit(1)
	}

	encryptedMetaData, err := os.ReadFile(metaDataPath)
	if err != nil {
		fmt.Printf("[World] Error reading metadata %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	decryptedMetaData, err := decryptData(encryptedMetaData)
	if err != nil {
		fmt.Printf("[World] Error decrypting metadata %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	decompressedMetaData, err := decompressData(decryptedMetaData)
	if err != nil {
		fmt.Printf("[World] Error decompressing metadata %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	metaDataContent := string(decompressedMetaData)
	fileCid, fileName, err := extractFileDetailsFromMetaData(metaDataContent)
	if err != nil {
		fmt.Printf("[World] Error extracting file details from metadata %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	filePath := filepath.Join(".fw", "content", filepath.Base(fileName))
	err = ipfs.Download(fileCid, filePath)
	if err != nil {
		fmt.Printf("[World] Error downloading file from IPFS: %v\n", err)
		os.Exit(1)
	}

	encryptedFileData, err := os.ReadFile(filePath)
	if err != nil {
		fmt.Printf("[World] Error reading file %s: %v\n", filePath, err)
		os.Exit(1)
	}

	decryptedFileData, err := decryptData(encryptedFileData)
	if err != nil {
		fmt.Printf("[World] Error decrypting file %s: %v\n", filePath, err)
		os.Exit(1)
	}

	decompressedFileData, err := decompressData(decryptedFileData)
	if err != nil {
		fmt.Printf("[World] Error decompressing file %s: %v\n", filePath, err)
		os.Exit(1)
	}

	err = saveFile(filePath, decompressedFileData)
	if err != nil {
		fmt.Printf("[World] Error saving decompressed file %s: %v\n", filePath, err)
		os.Exit(1)
	}

	fmt.Printf("[World] File downloaded, decrypted, and saved to %s\n", filePath)
}

func extractFileDetailsFromMetaData(metaDataContent string) (string, string, error) {
	var fileCid, fileName string
	lines := strings.Split(metaDataContent, "\n")
	for _, line := range lines {
		if strings.HasPrefix(line, "file: ") {
			fileName = strings.TrimPrefix(line, "file: ")
		} else if strings.HasPrefix(line, "cid: ") {
			fileCid = strings.TrimPrefix(line, "cid: ")
		}
	}

	if fileCid == "" {
		return "", "", fmt.Errorf("file CID not found in metadata")
	}
	return fileCid, fileName, nil
}

func encryptData(plainData []byte) ([]byte, error) {
	block, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(block)
	if err != nil {
		return nil, err
	}

	hash := sha256.Sum256(plainData)
	nonce := hash[:gcm.NonceSize()]

	return gcm.Seal(nonce, nonce, plainData, nil), nil
}

func decryptData(encryptedData []byte) ([]byte, error) {
	block, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(block)
	if err != nil {
		return nil, err
	}

	nonceSize := gcm.NonceSize()
	if len(encryptedData) < nonceSize {
		return nil, fmt.Errorf("[World] ciphertext too short")
	}

	nonce, ciphertext := encryptedData[:nonceSize], encryptedData[nonceSize:]
	return gcm.Open(nil, nonce, ciphertext, nil)
}

func compressData(data []byte) ([]byte, error) {
	var compressedData bytes.Buffer
	writer := gzip.NewWriter(&compressedData)
	_, err := writer.Write(data)
	if err != nil {
		return nil, err
	}
	err = writer.Close()
	if err != nil {
		return nil, err
	}
	return compressedData.Bytes(), nil
}

func decompressData(data []byte) ([]byte, error) {
	reader, err := gzip.NewReader(bytes.NewReader(data))
	if err != nil {
		return nil, err
	}
	defer reader.Close()

	var decompressedData bytes.Buffer
	_, err = io.Copy(&decompressedData, reader)
	if err != nil {
		return nil, err
	}
	return decompressedData.Bytes(), nil
}

func parseCoordinates(args []string) ([]float64, error) {
	coords := make([]float64, 9)
	for i := 0; i < 9; i++ {
		coord, err := strconv.ParseFloat(args[i], 64)
		if err != nil {
			return nil, fmt.Errorf("invalid input %s: %v", args[i], err)
		}
		coords[i] = coord
	}
	return coords, nil
}
