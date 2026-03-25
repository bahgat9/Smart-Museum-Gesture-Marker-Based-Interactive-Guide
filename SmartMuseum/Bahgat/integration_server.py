import socket
import json
import threading
from shared_state import state
from rules import decide_action

HOST = "127.0.0.1"
PORT = 8000

GUI_HOST = "127.0.0.1"
GUI_PORT = 6001

BLUETOOTH_GUI_PORT = 6002
TUIO_GUI_PORT = 6003

def send_to_gui_port(port, payload):
    try:
        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.connect((GUI_HOST, port))
        client.sendall((json.dumps(payload) + "\n").encode("utf-8"))
        client.close()
    except Exception as e:
        print(f"GUI send error on port {port}:", e)

def send_action_to_gui(action_result):
    try:
        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.connect((GUI_HOST, GUI_PORT))
        client.sendall((json.dumps(action_result) + "\n").encode("utf-8"))
        client.close()
        print("Sent to GUI:", action_result)
    except Exception as e:
        print("GUI send error:", e)


def process_message(message):
    try:
        data = json.loads(message)
        source = data.get("source")

        if source == "user":
            state["user_data"] = data
            print("Updated user_data")
            send_to_gui_port(BLUETOOTH_GUI_PORT, data)

        elif source == "marker":
            state["marker_data"] = data
            print("Updated marker_data")
            send_to_gui_port(TUIO_GUI_PORT, data)

        elif source == "gesture":
            state["gesture_data"] = data
            print("Updated gesture_data")

        else:
            print("Unknown source:", source)
            return

        action_result = decide_action(
            state["user_data"],
            state["marker_data"],
            state["gesture_data"]
        )

        if action_result != state["last_action"]:
            state["last_action"] = action_result
            print("Final action:", action_result)
            send_action_to_gui(action_result)

    except Exception as e:
        print("Process message error:", e)


def handle_client(conn, addr):
    print("Connected:", addr)
    try:
        buffer = ""
        while True:
            data = conn.recv(1024)
            if not data:
                break

            buffer += data.decode("utf-8")

            while "\n" in buffer:
                line, buffer = buffer.split("\n", 1)
                if line.strip():
                    process_message(line.strip())

    except Exception as e:
        print("Client error:", e)
    finally:
        conn.close()


def main():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((HOST, PORT))
    server.listen(10)

    print(f"Integration server running on {HOST}:{PORT}")

    while True:
        conn, addr = server.accept()
        thread = threading.Thread(target=handle_client, args=(conn, addr), daemon=True)
        thread.start()


if __name__ == "__main__":
    main()