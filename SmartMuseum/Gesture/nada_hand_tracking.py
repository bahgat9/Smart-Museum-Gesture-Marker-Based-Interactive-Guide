import cv2
import mediapipe as mp
import socket
import pickle
import math

#socket#
traj_client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
traj_client.connect(("127.0.0.1", 5000))
print("Connected to Maram Server")

gui_client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
gui_client.connect(("127.0.0.1", 6000))
print("Connected to GUI Server")
#socket#

mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(1)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)


trajectory = []
last_point = None
recording = False


def distance(p1, p2):
    return math.sqrt((p1[0] - p2[0])**2 + (p1[1] - p2[1])**2)

def is_pinching(p1, p2, threshold=45):
    return distance(p1, p2) < threshold


with mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=1,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7
) as hands:

    while True:
        success, frame = cap.read()

        if not success or frame is None:
            print("Failed to read frame from camera")
            continue

        frame = cv2.flip(frame, 1)
        h, w, _ = frame.shape

        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = hands.process(rgb)

        current_point = None
        pinching = False

       
        if results.multi_hand_landmarks:
            hand_landmarks = results.multi_hand_landmarks[0]

            mp_draw.draw_landmarks(
                frame,
                hand_landmarks,
                mp_hands.HAND_CONNECTIONS
            )

           
            index_tip = hand_landmarks.landmark[8]
            thumb_tip = hand_landmarks.landmark[4]

            ix, iy = int(index_tip.x * w), int(index_tip.y * h)
            tx, ty = int(thumb_tip.x * w), int(thumb_tip.y * h)

            index_point = (ix, iy)
            thumb_point = (tx, ty)

            current_point = index_point

           
            cv2.circle(frame, index_point, 10, (0, 255, 0), -1)
            cv2.circle(frame, thumb_point, 10, (255, 0, 0), -1)

            
            pinching = is_pinching(index_point, thumb_point)

            
            try:
                gui_client.send(f"MOVE:{ix},{iy}\n".encode())
                gui_client.send(f"PINCH:{1 if pinching else 0}\n".encode())
            except:
                pass

       
        if current_point is not None:

            if last_point is None:
                last_point = current_point

            
            if pinching and not recording:
                recording = True
                trajectory = [current_point]
                print("Pinch START - Recording")

            
            elif pinching and recording:
                if distance(current_point, last_point) > 5:
                    trajectory.append(current_point)

            
            elif not pinching and recording:

                
                if len(trajectory) > 20:
                    print("Pinch RELEASE - Sending")

                    payload = {"trajectory": trajectory}

                    try:
                        traj_client.send(pickle.dumps(payload))

                        response = traj_client.recv(1024)
                        result = pickle.loads(response)

                        print("Gesture:", result)

                    except Exception as e:
                        print("Maram error:", e)
                else:
                    print("Ignored small movement (selection, not gesture)")

                recording = False
                trajectory = []

            last_point = current_point

        else:
            if recording and len(trajectory) > 20:
                print("Hand lost - Sending")

                payload = {"trajectory": trajectory}

                try:
                    traj_client.send(pickle.dumps(payload))

                    response = traj_client.recv(1024)
                    result = pickle.loads(response)

                    print("Gesture:", result)

                except Exception as e:
                    print("Maram error:", e)

            recording = False
            trajectory = []
            last_point = None

        for i in range(1, len(trajectory)):
            cv2.line(frame, trajectory[i - 1], trajectory[i], (255, 0, 0), 2)

        if pinching:
            cv2.putText(frame, "PINCHING", (10, 70),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)

        cv2.putText(frame, f"Recording: {recording}", (10, 30),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 255), 2)

        cv2.imshow("Nada + Pinch + Socket", frame)

        key = cv2.waitKey(10) & 0xFF
        if key == ord('q') or key == 27:
            break
        elif key == ord('c'):
            trajectory.clear()

cap.release()
cv2.destroyAllWindows()
traj_client.close()
gui_client.close()