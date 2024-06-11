package main

import (
	"fmt"
	"sync"
)

type InMemoryFileSystem struct {
	files map[string][]byte
	mu    sync.RWMutex
}

var memFS = InMemoryFileSystem{files: make(map[string][]byte)}

func (fs *InMemoryFileSystem) Stat(path string) (bool, error) {
	fs.mu.RLock()
	defer fs.mu.RUnlock()

	_, exists := fs.files[path]
	if !exists {
		return false, fmt.Errorf("file not found: %s", path)
	}
	return true, nil
}

func (fs *InMemoryFileSystem) ReadFile(path string) ([]byte, error) {
	fs.mu.RLock()
	defer fs.mu.RUnlock()

	data, exists := fs.files[path]
	if !exists {
		fmt.Println("TestTest")
		return nil, fmt.Errorf("file not found: %s", path)
	}
	return data, nil
}

func (fs *InMemoryFileSystem) WriteFile(path string, data []byte) error {
	fs.mu.Lock()
	defer fs.mu.Unlock()

	fs.files[path] = data
	return nil
}

func (fs *InMemoryFileSystem) MkdirAll(path string) error {
	fs.mu.Lock()
	defer fs.mu.Unlock()

	// No-op for in-memory filesystem
	return nil
}

func (fs *InMemoryFileSystem) Rename(oldPath, newPath string) error {
	fs.mu.Lock()
	defer fs.mu.Unlock()

	data, exists := fs.files[oldPath]
	if !exists {
		return fmt.Errorf("file not found: %s", oldPath)
	}
	fs.files[newPath] = data
	delete(fs.files, oldPath)
	return nil
}
