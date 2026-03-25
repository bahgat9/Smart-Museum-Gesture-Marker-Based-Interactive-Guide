def decide_action(user_data, marker_data, gesture_data):
    if not marker_data or not gesture_data:
        return {"action": "Waiting for inputs"}

    marker_id = marker_data.get("marker_id")
    gesture = gesture_data.get("gesture")
    user_type = user_data.get("user_type") if user_data else "Guest"
    artifact = marker_data.get("artifact", "Unknown")

    if marker_id == 1 and gesture == "circle":
        return {"action": "Play Pharaoh Video"}

    if marker_id == 2 and gesture == "circle":
        return {"action": "Play Weapons Video"}

    if marker_id == 3 and gesture == "circle":
        return {"action": "Start Audio Guide"}

    if marker_id == 4 and gesture == "circle":
        return {"action": "Open Museum Map"}

    if user_type == "VIP" and marker_id == 1 and gesture == "stop":
        return {"action": "Show VIP Pharaoh Details"}

    if gesture == "swipe_left":
        return {"action": f"Previous content for {artifact}"}

    if gesture == "swipe_right":
        return {"action": f"Next content for {artifact}"}

    return {"action": "No rule matched"}