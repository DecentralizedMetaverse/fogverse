package main

import (
	"fmt"
	"os"
)

func runCommand(name string, args []string) {
	commandHandler := map[string]func([]string){
		"init":           handleInit,
		"switch":         handleSwitch,
		"get":            handleGet,
		"put":            handlePut,
		"set-password":   handleSetPassword,
		"cat":            handleCat,
		"get-world-cid":  handleGetWorldCID,
		"download-world": handleDownloadWorld,
		"get-world-data": handleGetWorldData,
	}

	if handler, found := commandHandler[name]; found {
		handler(args)
	} else {
		fmt.Printf("Unknown command: %s\n", name)
		os.Exit(1)
	}
}
