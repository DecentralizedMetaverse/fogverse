package main

import (
	"fmt"
	"net/http"
	"os"
)

func main() {
	if len(os.Args) > 1 {
		// コマンドライン引数が存在する場合、コマンドラインモードで実行
		runCommand(os.Args[1], os.Args[2:])
	} else {
		// 引数がない場合、HTTPサーバーを起動
		http.HandleFunc("/init", handleInitAPI)
		http.HandleFunc("/switch", handleSwitchAPI)
		http.HandleFunc("/get", handleGetAPI)
		http.HandleFunc("/put", handlePutAPI)
		http.HandleFunc("/set-password", handleSetPasswordAPI)
		http.HandleFunc("/cat", handleCatAPI)
		http.HandleFunc("/get-world-cid", handleGetWorldCIDAPI)
		http.HandleFunc("/download-world", handleDownloadWorldAPI)
		http.HandleFunc("/get-world-data", handleGetWorldDataAPI)

		fmt.Println("Starting server on :8080")
		http.ListenAndServe(":8080", nil)
	}
}
