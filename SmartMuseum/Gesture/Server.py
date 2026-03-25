import socket
import pickle
import json
from recognizer import recognize_gesture

HOST = '0.0.0.0'
PORT = 5000

GUI_HOST = '127.0.0.1'
GUI_PORT = 6000
#socketbahgat#
SOCKET_HOST = '127.0.0.1'
SOCKET_PORT = 8000

def send_gesture_to_bahgat(result):
    try:
        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.connect((SOCKET_HOST, SOCKET_PORT))

        payload = {
            "source": "gesture",
            "gesture": result.get("gesture")
        }

        client.sendall((json.dumps(payload) + "\n").encode("utf-8"))
        client.close()

    except Exception as e:
        print("Integration server not connected:", e)
#socketbahgat#
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen(1)

print("Gesture Server Running... Waiting for connection")

conn, addr = server.accept()
print(f"Connected by {addr}")

while True:
    try:
        data = conn.recv(4096)
        if not data:
            break

        payload = pickle.loads(data)
        trajectory = payload["trajectory"]

        print("Received trajectory:", trajectory)

        result = recognize_gesture(trajectory)
        print("Recognized:", result)
        send_gesture_to_bahgat(result) #socket#

        
        conn.send(pickle.dumps(result))

        try:
            gui_client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            gui_client.connect((GUI_HOST, GUI_PORT))
            gesture_name = result.get("gesture")

            if gesture_name is not None:
                gui_client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                gui_client.connect((GUI_HOST, GUI_PORT))
                gui_client.send((gesture_name + "\n").encode())
                gui_client.close()
            gui_client.close()
        except Exception as e:
            print("GUI not connected:", e)

    except Exception as e:
        print("Error:", e)
        break

conn.close()