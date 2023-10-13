package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"strings"

	"github.com/gorilla/handlers"
	"github.com/gorilla/websocket"
)

var (
	upgrader = websocket.Upgrader{}

	clients              = make(map[*websocket.Conn]string)
	clientsByID          = make(map[string]*websocket.Conn)
	broadcast            = make(chan []byte)
	offerId       string = ""
	functions            = make(map[string]interface{})
	sdpData              = make(map[string]string)
	candidateData        = make(map[string][]string)
)

func main() {
	functions["connect"] = connect
	functions["offer"] = offer
	functions["answer"] = answer
	functions["candidateAdd"] = candidateAdd
	// http.HandleFunc("/ws", handleWebSocket)

	//------------------------------------------------
	// CORS設定
	cors := handlers.CORS(
		handlers.AllowedOrigins([]string{"*"}),
		handlers.AllowedOrigins([]string{"http://localhost:8000"}),
		handlers.AllowedMethods([]string{"GET", "POST", "PUT", "DELETE", "OPTIONS"}),
		handlers.AllowedHeaders([]string{"Content-Type", "Authorization"}),
	)
	//------------------------------------------------
	http.Handle("/ws", cors(http.HandlerFunc(handleWebSocket)))
	go handleMessages()
	http.ListenAndServe(":8080", nil)
}

func handleWebSocket(w http.ResponseWriter, r *http.Request) {
	conn, _ := upgrader.Upgrade(w, r, nil) // error ignored for sake of simplicity

	for {
		_, message, err := conn.ReadMessage()
		if _, ok := clients[conn]; !ok {
			// 新規接続
			var jsonStr = string(message)
			var data map[string]interface{}
			err := json.Unmarshal([]byte(jsonStr), &data)
			if err != nil {
				panic(err)
			}

			// idの登録
			id := data["id"].(string)
			clients[conn] = id
			clientsByID[id] = conn
		}

		if err != nil {
			log.Println(err)
			delete(clientsByID, clients[conn])
			delete(clients, conn)
			break
		}

		broadcast <- message
	}
	offerId = ""
	defer conn.Close()
}

func handleMessages() {
	for {
		message := <-broadcast
		// text -> json
		var jsonStr = string(message)
		fmt.Println(jsonStr)
		var data map[string]interface{}
		err := json.Unmarshal([]byte(jsonStr), &data)
		if err != nil {
			panic(err)
		}

		// 処理分岐
		msgDataType := data["type"].(string)
		function := functions[msgDataType].(func(map[string]interface{}))
		function(data)
	}
}

func connect(data map[string]interface{}) {
	resultData := make(map[string]string)

	id := data["id"].(string)
	client := clientsByID[id]

	// offerを送ってもらう
	if len(offerId) == 0 {
		// offerがまだない場合
		offerId = id
		resultData["type"] = "offer"
		bytes := jsonToBytes(resultData)
		sendMessage(client, bytes)
		return
	} else if id == offerId {
		// 重複
		return
	}

	// offerを送る
	resultData["type"] = "offer"
	resultData["sdp"] = sdpData[offerId]
	resultData["id"] = offerId
	bytes := jsonToBytes(resultData)
	sendMessage(client, bytes)
}

func offer(data map[string]interface{}) {
	fmt.Println("[Offer]")
	id := data["id"].(string)
	sdp, _ := json.Marshal(data["sdp"])
	sdpData[id] = string(sdp)
}

func answer(data map[string]interface{}) {
	// offerの送り主にanswerを返す
	sendAnswer(data)

	// answerの送り主にcandidateを送る
	sendCandidate(data)

	offerId = ""
}

func sendAnswer(data map[string]interface{}) {
	fmt.Println("[Answer]")
	resultData := make(map[string]string)
	resultData["type"] = "answer"
	id := data["id"].(string)
	sdp, _ := json.Marshal(data["sdp"])
	resultData["sdp"] = string(sdp)
	resultData["id"] = id
	resultData["target_id"] = offerId

	client := clientsByID[offerId]
	bytes := jsonToBytes(resultData)
	sendMessage(client, bytes)
}

func sendCandidate(data map[string]interface{}) {
	returnData := make(map[string]string)
	id := offerId
	fmt.Println("check candidate" + id)
	if _, ok := candidateData[id]; !ok {
		return
	}

	fmt.Println("保存されたcandidateを送信")
	answerId := data["id"].(string)
	client := clientsByID[answerId]
	fmt.Println("[Candidate]")
	returnData["type"] = "candidate"
	returnData["candidate"] = strings.Join(candidateData[id], "|")
	bytes := jsonToBytes(returnData)
	sendMessage(client, bytes)
}

func candidateAdd(data map[string]interface{}) {
	fmt.Println("[Candidate Add]")
	resultData := make(map[string]string)

	// 相手が已經接続的話、candidateDataに入れずに直接送る
	id := data["id"].(string)
	candidateByte, _ := json.Marshal(data["candidate"])
	candidate := string(candidateByte)

	// target_idにdataがあるかどうかで判定
	target_id := data["target_id"].(string)
	if target_id != "" {
		if client, ok2 := clientsByID[target_id]; ok2 {
			// 相手が有的話
			fmt.Println("[Candidate]")
			resultData["type"] = "candidate"
			resultData["candidate"] = candidate
			bytes := jsonToBytes(resultData)
			sendMessage(client, bytes)
			return
		}
	}

	// 相手が還沒來 -> 保存
	fmt.Println("Answer還沒來" + id + candidate)
	if _, ok := candidateData[id]; !ok {
		candidateData[id] = []string{candidate}
	} else {
		candidateData[id] = append(candidateData[id], candidate)
	}
}

// 訊息送信
func sendMessage(client *websocket.Conn, bytes []byte) {
	err := client.WriteMessage(websocket.TextMessage, bytes)
	if err != nil {
		log.Println(err)
		client.Close()
		delete(clients, client)
	}
}

func jsonToBytes(result map[string]string) []byte {
	jsonText, err := json.Marshal(result)
	if err != nil {
		panic(err)
	}

	bytes := []byte(jsonText)
	return bytes
}
