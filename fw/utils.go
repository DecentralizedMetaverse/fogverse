package main

import (
	"fmt"
	"path/filepath"
	"strconv"
	"strings"
)

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

func generateMetaDataContent(filePath, cid string, coords []float64) string {
	fileName := filepath.Base(filePath)
	return fmt.Sprintf("cid: %s\nfile: %s\nx: %f\ny: %f\nz: %f\nrx: %f\nry: %f\nrz: %f\nsx: %f\nsy: %f\nsz: %f\n",
		cid, fileName, coords[0], coords[1], coords[2], coords[3], coords[4], coords[5], coords[6], coords[7], coords[8])
}

func encryptAndCompressMetaData(metaDataContent string) ([]byte, error) {
	compressedMetaData, err := compressData([]byte(metaDataContent))
	if err != nil {
		return nil, err
	}

	return encryptData(compressedMetaData)
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
