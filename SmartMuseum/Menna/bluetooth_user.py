import bluetooth
from datetime import datetime
import socket
import json

USERS_DB = {
    "64:D0:D6:65:26:35": {"user_id": "USR-001", "user_name": "Potato User",   "user_type": "VIP"},
    "AC:07:75:DF:14:65": {"user_id": "USR-002", "user_name": "AirPods Owner", "user_type": "Regular"},
    "AA:BB:CC:DD:EE:11": {"user_id": "USR-003", "user_name": "Menna Hossam",  "user_type": "Premium"},
    "AA:BB:CC:DD:EE:22": {"user_id": "USR-004", "user_name": "Ahmed Salah",   "user_type": "Staff"},
    "FC:18:3C:BE:45:8A": {"user_id": "USR-005", "user_name": "Bahgat Yasser",   "user_type": "VIP"},
}


def match_user(addr, name):
    mac_upper = addr.upper()

    if mac_upper in USERS_DB:
        user = USERS_DB[mac_upper]
        return {
            "device_name": name or user["user_name"],
            "mac"        : mac_upper,
            "user_type"  : user["user_type"],
            "user_id"    : user["user_id"],
            "user_name"  : user["user_name"],
            "matched"    : True,
        }

    return {
        "device_name": name or "Unknown",
        "mac"        : mac_upper,
        "user_type"  : "Guest",
        "user_id"    : None,
        "user_name"  : "Guest",
        "matched"    : False,
    }
#Socket#
def send_user_to_bahgat(user_result):
    try:
        print("Sending:", user_result)

        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.connect(("127.0.0.1", 8000))

        payload = user_result.copy()
        payload["source"] = "user"

        client.sendall((json.dumps(payload) + "\n").encode("utf-8"))
        client.close()

        print("Sent successfully")

    except Exception as e:
        print("error sending:", e)
#Socket#
def scan_bluetooth_devices():
    print("Scanning for bluetooth devices...")
    scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    try:
        devices = bluetooth.discover_devices(
            duration=8,
            lookup_names=True
        )

        print(f"\nScan completed at {scan_time}")
        print(f"Found {len(devices)} devices:\n")

        for addr, name in devices:
            result = match_user(addr, name)
            print(result)
            send_user_to_bahgat(result) #socket#

    except Exception as e:
        print(f"An error occurred: {str(e)}")


def main():
    print("\nStarting Bluetooth scan...")
    scan_bluetooth_devices()


if __name__ == "__main__":
    main()