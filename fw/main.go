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
	"io"
	"os"
	"path/filepath"
	"strconv"
)

type Command struct {
	Name string
	Args []string
}

type CommandHandler func(args []string)

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
		"init":         handleInit,   // create directory
		"create":       handleCreate, // create world
		"switch":       nil,          // switch world
		"get":          nil,          // get world binary data
		"get-raw":      nil,
		"put":          handlePut,         // put world binary data
		"set-password": handleSetPassword, // set encryption password
		"cat":          handleCat,         // view encrypted file content
	}

	if handler, found := commandHandler[cmd.Name]; found {
		handler(cmd.Args)
	} else {
		fmt.Printf("[FW] Unknown command: %s\n", cmd.Name)
		os.Exit(1)
	}
}

func handleSetPassword(args []string) {
	if len(args) < 1 {
		fmt.Println("[FW] Usage: fw set-password <password>")
		os.Exit(1)
	}

	password := args[0]
	hashedPassword := sha256.Sum256([]byte(password))
	key = hashedPassword[:]

	err := os.WriteFile(".fw/password", []byte(hex.EncodeToString(key)), 0644)
	if err != nil {
		fmt.Printf("[FW] Error saving password: %v\n", err)
		os.Exit(1)
	}

	fmt.Println("[FW] Password set successfully.")
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

func handlePut(args []string) {
	if len(args) < 10 {
		fmt.Println("[FW] Usage: fw put <file> <x> <y> <z> <rx> <ry> <rz> <sx> <sy> <sz>")
		os.Exit(1)
	}

	err := loadPassword()
	if err != nil {
		fmt.Printf("[FW] Error loading password: %v\n", err)
		os.Exit(1)
	}

	filePath, err := filepath.Abs(args[0])
	if err != nil {
		fmt.Printf("[FW] Invalid file path: %v\n", err)
		os.Exit(1)
	}
	x, err := strconv.ParseFloat(args[1], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid x coordinate: %v\n", err)
		os.Exit(1)
	}
	y, err := strconv.ParseFloat(args[2], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid y coordinate: %v\n", err)
		os.Exit(1)
	}
	z, err := strconv.ParseFloat(args[3], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid z coordinate: %v\n", err)
		os.Exit(1)
	}
	rx, err := strconv.ParseFloat(args[4], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid rx rotation: %v\n", err)
		os.Exit(1)
	}
	ry, err := strconv.ParseFloat(args[5], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid ry rotation: %v\n", err)
		os.Exit(1)
	}
	rz, err := strconv.ParseFloat(args[6], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid rz rotation: %v\n", err)
		os.Exit(1)
	}
	sx, err := strconv.ParseFloat(args[7], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid sx scale: %v\n", err)
		os.Exit(1)
	}
	sy, err := strconv.ParseFloat(args[8], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid sy scale: %v\n", err)
		os.Exit(1)
	}
	sz, err := strconv.ParseFloat(args[9], 64)
	if err != nil {
		fmt.Printf("[FW] Invalid sz scale: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("[FW] Putting world binary data from %s with coordinates (%f, %f, %f), rotation (%f, %f, %f), scale (%f, %f, %f)\n", filePath, x, y, z, rx, ry, rz, sx, sy, sz)

	// Open the file
	file, err := os.Open(filePath)
	if err != nil {
		fmt.Printf("[FW] Error opening file %s: %v\n", filePath, err)
		os.Exit(1)
	}
	defer file.Close()

	// Create GUID
	hash := sha256.New()
	if _, err := io.Copy(hash, file); err != nil {
		fmt.Printf("[FW] Error hashing file: %v\n", err)
		os.Exit(1)
	}
	guid := hex.EncodeToString(hash.Sum(nil))

	// Get file content
	_, err = file.Seek(0, 0)
	if err != nil {
		fmt.Printf("[FW] Error seeking file: %v\n", err)
		os.Exit(1)
	}
	byteArray, err := io.ReadAll(file)
	if err != nil {
		fmt.Printf("[FW] Error reading file: %v\n", err)
		os.Exit(1)
	}

	// Compress file
	compressedData, err := compressData(byteArray)
	if err != nil {
		fmt.Printf("[FW] Error compressing file: %v\n", err)
		os.Exit(1)
	}

	// Encrypt file
	encryptedData, err := encryptData(compressedData)
	if err != nil {
		fmt.Printf("[FW] Error encrypting file: %v\n", err)
		os.Exit(1)
	}

	// Create the path to save the object
	objectPath := filepath.Join(".fw", "objects", guid)

	// Save the file
	err = os.WriteFile(objectPath, encryptedData, 0644)
	if err != nil {
		fmt.Printf("[FW] Error saving file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	// Upload to IPFS
	cid, err := ipfs.Upload(objectPath)
	if err != nil {
		fmt.Printf("[FW] Error uploading file to IPFS: %v\n", err)
		os.Exit(1)
	}

	// Rename file GUID -> CID
	err = os.Rename(objectPath, filepath.Join(".fw", "objects", cid))
	if err != nil {
		fmt.Printf("[FW] Error renaming file: %v\n", err)
		os.Exit(1)
	}

	// Create and encrypt metadata file
	metaDataContent := fmt.Sprintf("file: %s\ncid: %s\nx: %f\ny: %f\nz: %f\nrx: %f\nry: %f\nrz: %f\nsx: %f\nsy: %f\nsz: %f\n", filePath, cid, x, y, z, rx, ry, rz, sx, sy, sz)

	// Compress metadata
	compressedMetaData, err := compressData([]byte(metaDataContent))
	if err != nil {
		fmt.Printf("[FW] Error compressing metadata: %v\n", err)
		os.Exit(1)
	}

	// Encrypt metadata
	encryptedMetaData, err := encryptData(compressedMetaData)
	if err != nil {
		fmt.Printf("[FW] Error encrypting metadata: %v\n", err)
		os.Exit(1)
	}

	// Save metadata
	metaDataPath := filepath.Join(".fw", "objects", guid)
	err = os.WriteFile(metaDataPath, encryptedMetaData, 0644)
	if err != nil {
		fmt.Printf("[FW] Error creating metadata file %s: %v\n", metaDataPath, err)
		os.Exit(1)
	}

	// Upload metadata to IPFS
	metaCid, err := ipfs.Upload(metaDataPath)
	if err != nil {
		fmt.Printf("[FW] Error uploading metadata to IPFS: %v\n", err)
		os.Exit(1)
	}

	// Rename metadata GUID -> CID
	err = os.Rename(metaDataPath, filepath.Join(".fw", "objects", metaCid))
	if err != nil {
		fmt.Printf("[FW] Error renaming metadata file: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("[FW] File saved as %s\n", filepath.Join(".fw", "objects", cid))
	fmt.Printf("[FW] Metadata saved as %s\n", filepath.Join(".fw", "objects", metaCid))
}

func handleCat(args []string) {
	if len(args) < 1 {
		fmt.Println("[FW] Usage: fw cat <file-hash>")
		os.Exit(1)
	}

	err := loadPassword()
	if err != nil {
		fmt.Printf("[FW] Error loading password: %v\n", err)
		os.Exit(1)
	}

	fileHash := args[0]
	objectPath := filepath.Join(".fw", "objects", fileHash)

	// Read the encrypted data from the file
	encryptedData, err := os.ReadFile(objectPath)
	if err != nil {
		fmt.Printf("[FW] Error reading file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	// Decrypt the data
	decryptedData, err := decryptData(encryptedData)
	if err != nil {
		fmt.Printf("[FW] Error decrypting file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	// Decompress the data
	decompressedData, err := decompressData(decryptedData)
	if err != nil {
		fmt.Printf("[FW] Error decompressing file %s: %v\n", objectPath, err)
		os.Exit(1)
	}

	fmt.Printf("[FW] Decrypted and decompressed file content:\n%s\n", string(decompressedData))
}

func encryptFile(file *os.File) ([]byte, error) {
	fileInfo, err := file.Stat()
	if err != nil {
		return nil, err
	}

	plainData := make([]byte, fileInfo.Size())
	if _, err := file.Read(plainData); err != nil {
		return nil, err
	}

	return encryptData(plainData)
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
		return nil, fmt.Errorf("[FW] ciphertext too short")
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

func handleInit(args []string) {

	// Initialize IPFS
	err := ipfs.InitIPFS()
	if err != nil {
		fmt.Printf("[FW] Error initializing IPFS: %v\n", err)
		os.Exit(1)
	}

	// If .fw already exists, exit
	if _, err := os.Stat(".fw"); err == nil {
		fmt.Println("[FW] .fw repository already exists.")
		return
	}

	fmt.Println("[FW] .fw repository initialized.")

	directories := []string{
		".fw",
		".fw/objects",
		".fw/worlds",
		".fw/worlds/heads",
		".fw/worlds/tags",
	}

	files := map[string]string{
		".fw/HEAD":        "ref: refs/heads/master\n",
		".fw/config":      "[core]\n\trepositoryformatversion = 0\n\tfilemode = true\n\tbare = false\n",
		".fw/description": "Unnamed repository; edit this file 'description' to name the repository.\n",
	}

	for _, dir := range directories {
		absDir, err := filepath.Abs(dir)
		if err != nil {
			fmt.Printf("[FW] Error creating directory %s: %v\n", dir, err)
			os.Exit(1)
		}
		err = os.MkdirAll(absDir, 0755)
		if err != nil {
			fmt.Printf("[FW] Error creating directory %s: %v\n", absDir, err)
			os.Exit(1)
		}
	}

	for file, content := range files {
		absFile, err := filepath.Abs(file)
		if err != nil {
			fmt.Printf("[FW] Error creating file %s: %v\n", file, err)
			os.Exit(1)
		}
		err = os.WriteFile(absFile, []byte(content), 0644)
		if err != nil {
			fmt.Printf("[FW] Error creating file %s: %v\n", absFile, err)
			os.Exit(1)
		}
	}
}

func handleCreate(args []string) {
	fmt.Println("[FW] Creating new world...")
}
