package main

import (
	"fmt"
	"net/http"
	"strings"
)

func handleInitAPI(w http.ResponseWriter, r *http.Request) {
	handleInit(nil)
	w.Write([]byte("Repository initialized."))
}

func handleSwitchAPI(w http.ResponseWriter, r *http.Request) {
	worldName := r.URL.Query().Get("worldName")
	handleSwitch([]string{worldName})
	w.Write([]byte(fmt.Sprintf("Switched to world: %s", worldName)))
}

func handleGetAPI(w http.ResponseWriter, r *http.Request) {
	cid := r.URL.Query().Get("cid")
	handleGet([]string{cid})
	w.Write([]byte(fmt.Sprintf("Get operation completed for CID: %s", cid)))
}

func handlePutAPI(w http.ResponseWriter, r *http.Request) {
	args := r.URL.Query().Get("args")
	handlePut(strings.Split(args, " "))
	w.Write([]byte("Put operation completed."))
}

func handleSetPasswordAPI(w http.ResponseWriter, r *http.Request) {
	password := r.URL.Query().Get("password")
	handleSetPassword([]string{password})
	w.Write([]byte("Password set successfully."))
}

func handleCatAPI(w http.ResponseWriter, r *http.Request) {
	fileHash := r.URL.Query().Get("fileHash")
	handleCat([]string{fileHash})
	w.Write([]byte("Cat operation completed."))
}

func handleGetWorldCIDAPI(w http.ResponseWriter, r *http.Request) {
	handleGetWorldCID(nil)
	w.Write([]byte("Get World CID operation completed."))
}

func handleDownloadWorldAPI(w http.ResponseWriter, r *http.Request) {
	cid := r.URL.Query().Get("cid")
	handleDownloadWorld([]string{cid})
	w.Write([]byte(fmt.Sprintf("Download world operation completed for CID: %s", cid)))
}

func handleGetWorldDataAPI(w http.ResponseWriter, r *http.Request) {
	handleGetWorldData(nil)
	w.Write([]byte("Get world data operation completed."))
}
