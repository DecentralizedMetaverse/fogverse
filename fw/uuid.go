package main

import (
	"crypto/rand"
	"fmt"
	"io"
)

// UUIDサイズは16バイト（128ビット）
const uuidSize = 16

// NewUUID newUUIDは新しいUUID v4を生成する
func NewUUID() (string, error) {
	u := make([]byte, uuidSize)
	_, err := io.ReadFull(rand.Reader, u)
	if err != nil {
		return "", err
	}

	// バージョン4に設定
	u[6] = (u[6] & 0x0f) | 0x40
	// Variant RFC4122に設定
	u[8] = (u[8] & 0x3f) | 0x80

	return fmt.Sprintf("%08x-%04x-%04x-%04x-%012x", u[0:4], u[4:6], u[6:8], u[8:10], u[10:]), nil
}
