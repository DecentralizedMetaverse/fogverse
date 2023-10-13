
import json
import asyncio
import websockets

clients = {}
offer_id = ""
sdp_data = {}
candidate_data = {}


async def handle_websocket(websocket, path):
    global offer_id

    async for message in websocket:
        data = json.loads(message)

        if websocket not in clients:
            # New connection
            id = data["id"]
            clients[websocket] = id
        print(data)
        msg_data_type = data.get("type")
        if msg_data_type:
            function = functions.get(msg_data_type)
            if function:
                await function(data, websocket)

    id = clients.get(websocket)
    if id:
        del clients[websocket]
        del clients_by_id[id]


async def connect(data, client):
    global offer_id
    global sdp_data
    
    result_data = {}
    id = data["id"]

    if not offer_id:
        # No offer yet
        print(f"offer_id: {offer_id}")
        offer_id = id
        result_data["type"] = "offer"
        await send_message(client, result_data)
    elif id == offer_id:
        # Duplicate
        pass
    else:
        print(f"offer_id: {offer_id}")
        # Send offer
        result_data["type"] = "offer"
        if not offer_id in sdp_data:
            return 
        result_data["data"] = sdp_data[offer_id]
        result_data["id"] = offer_id
        await send_message(client, result_data)


async def offer(data, _):
    global sdp_data
    id = data["id"]
    sdp_data[id] = data["data"]


async def answer(data, _):
    await send_answer(data)
    await send_candidate(data)
    global offer_id
    offer_id = ""


async def send_answer(data):
    result_data = {}
    id = data["id"]
    result_data["type"] = "answer"
    result_data["data"] = data["data"]
    result_data["id"] = id
    result_data["target_id"] = offer_id
    client = clients_by_id[offer_id]
    await send_message(client, result_data)


async def send_candidate(data):
    id = offer_id
    if id not in candidate_data:
        return

    return_data = {}
    answer_id = data["id"]
    client = clients_by_id[answer_id]
    return_data["type"] = "candidate"
    return_data["candidate"] = "|".join(candidate_data[id])
    await send_message(client, return_data)


async def candidate_add(data, _):
    id = data["id"]
    candidate = data["candidate"]
    target_id = data.get("target_id")

    if target_id:
        client = clients_by_id.get(target_id)
        if client:
            return_data = {"type": "candidate", "candidate": candidate}
            await send_message(client, return_data)
        return

    if id not in candidate_data:
        candidate_data[id] = [candidate]
    else:
        candidate_data[id].append(candidate)


async def send_message(client, data):
    message = json.dumps(data)
    await client.send(message)


functions = {
    "connect": connect,
    "offer": offer,
    "answer": answer,
    "candidateAdd": candidate_add,
}

clients_by_id = {}


start_server = websockets.serve(handle_websocket, "localhost", 8080)

asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()