
import json
import asyncio
import websockets
import ssl

class SignalingServer:
    def __init__(self):
        self.clients = {}
        self.client_a_id = ""
        self.sdp_data = {}
        self.candidate_data = {}
        self.clients_by_id = {}
        
        self.functions = {
            "connect": self.connect,
            "offer":self.offer,
            "answer": self.answer,
            "candidate_add": self.candidate_add,
        }
        
    async def handle_websocket(self, websocket, path):
        async for message in websocket:
            data = json.loads(message)

            if websocket not in self.clients:
                # New connection
                id = data["id"]
                self.clients[websocket] = id
                self.clients_by_id[id] = websocket
            print(f"[Receive] {data}")
            msg_data_type = data.get("type")
            if msg_data_type:
                function = self.functions.get(msg_data_type)
                if function:
                    await function(data, websocket)

        id = self.clients.get(websocket)
        if id:
            del self.clients[websocket]
            del self.clients_by_id[id]


    async def connect(self, data, client):
        result_data = {}
        id = data["id"]

        if not self.client_a_id:
            # A1
            print(f"offer_id: {self.client_a_id}")
            self.client_a_id = id
            result_data["type"] = "offer"
            await self.send_message(client, result_data)
            
        elif id != self.client_a_id:
            # B1
            print(f"offer_id: {self.client_a_id}")
            # Send offer
            result_data["type"] = "offer"
            if not self.client_a_id in self.sdp_data:
                return 
            result_data["data"] = self.sdp_data[self.client_a_id]
            result_data["target_id"] = self.client_a_id
            await self.send_message(client, result_data)
        else:
            # Duplicate
            pass


    async def offer(self,data, _):
        # A2
        id = data["id"]
        self.sdp_data[id] = data["data"]


    async def answer(self,data, _):
        # B2
        await self.send_answer(data)
        await self.send_candidate(data)
        self.client_a_id = ""


    async def send_answer(self,data):
        # A3
        result_data = {}
        id = data["id"]
        result_data["type"] = "answer"
        result_data["data"] = data["data"]
        result_data["id"] = id
        result_data["target_id"] = self.client_a_id
        client = self.clients_by_id[self.client_a_id]
        
        # if self.client_a_id in self.candidate_data:
        #     result_data["candidate"] = "|".join(self.candidate_data[self.client_a_id])
            
        await self.send_message(client, result_data)


    async def send_candidate(self,data):
        id = self.client_a_id
        if id not in self.candidate_data:
            return

        return_data = {}
        answer_id = data["id"]
        
        client = self.clients_by_id[answer_id]
        return_data["type"] = "candidate"
        return_data["candidate"] = "|".join(self.candidate_data[id])
        await self.send_message(client, return_data)


    async def candidate_add(self,data, _):
        id = data["id"]
        candidate = json.dumps(data["candidate"])
        target_id = data.get("target_id")

        if target_id:
            # 相手が分かっている場合はそのまま送る
            client = self.clients_by_id.get(target_id)
            if client:
                return_data = {"type": "candidate", "candidate": candidate}
                await self.send_message(client, return_data)
            return

        if id not in self.candidate_data:
            # まだ相手が到着していないので、記録しておく
            self.candidate_data[id] = [candidate]
        else:
            self.candidate_data[id].append(candidate)


    async def send_message(self,client, data):
        message = json.dumps(data)
        print(f"[Send] {data}")
        print()
        await client.send(message)

    def run(self):
        # SSLコンテキストを作成
        ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        ssl_context.load_cert_chain('certificate.pem', 'private-key.pem')

        # WSSを使用してWebSocketサーバを開始
        # start_server = websockets.serve(sig.handle_websocket, "localhost", 8080, ssl=ssl_context)

        start_server = websockets.serve(self.handle_websocket, "localhost", 8080)

        asyncio.get_event_loop().run_until_complete(start_server)
        asyncio.get_event_loop().run_forever()
        
if __name__ == "__main__":
    sig = SignalingServer()
    sig.run()