# 🏛️ Smart Museum Interactive Guide

An interactive museum system that allows users to explore artifacts using **gestures, physical markers, and Bluetooth identification**.

---

## 🎯 Project Overview

This system simulates a **real smart museum table** where users can:

- 📱 Select themselves using Bluetooth  
- 🟡 Place physical markers to explore artifacts  
- ✋ Use hand gestures to control the interface  
- 🧠 Get personalized content based on their profile  

---

## 🧠 System Features

### 🔹 Bluetooth User Detection
- Detects nearby devices  
- Matches users (VIP / Premium / Guest)  
- Personalizes the experience  

---

### 🔹 TUIO Marker Interaction
- Detect marker ID  
- Detect rotation  

**Marker Actions:**
- Marker 1 → Zoom artifact  
- Marker 2 → Switch weapon  
- Marker 3 → Control audio  
- Marker 4 → Zoom map  

---

### 🔹 Gesture Control
- Move cursor using hand  
- 🤏 Pinch → Click  
- 🔵 Circle → Return to menu  

---

### 🔹 Smart Logic (Integration)
- Combines:
  - User data  
  - Marker data  
  - Gesture data  
- Sends final action to GUI  

---

### 🔹 GUI (WPF)

**Pages:**
- Bluetooth  
- TUIO  
- Tutorial  

**Displays:**
- Images  
- Audio  
- System actions  

---

### 🔹 Tutorial System
- ▶️ Plays tutorial video automatically  
- 🔁 Returns to menu after finishing  

---

## 🏗️ System Architecture
### Bluetooth (Menna)
### ↓
### TUIO Markers (Nour)
### ↓
### Gestures (Nada + Maram)
### ↓
### Integration & Socket Logic (Bahgat)
### ↓
### GUI & Gesture Integration (Awad)

---

## 🧪 Technologies Used

- 💻 C# (WPF GUI)  
- 🐍 Python (MediaPipe, Socket Programming)  
- 🎯 TUIO (reacTIVision)  
- 📷 OpenCV  
- ✋ $1 Gesture Recognizer  
- 📡 Bluetooth Scanning  

---

## ▶️ How to Run the System

### 1️⃣ Run GUI (WPF)
Open and run:
- GestureGUI.sln

### 2️⃣ Run Integration Server
- python integration_server.py

### 3️⃣ Run Gesture Server
- python Server.py

### 4️⃣ Run Hand Tracking
- python nada_hand_tracking.py

### 5️⃣ Run Bluetooth Scan
- python bluetooth_user.py

### 6️⃣ Run reacTIVision
- Select second camera
- Start TUIO

## 📁 Project Structure
- GestureGUI/        → C# GUI
- SmartMuseum/       → Python modules
- TUIO11_NET-master/ → TUIO handling

---

## 👥 Team Roles

| Member | Role |
|--------|------|
| Menna  | Bluetooth + Users |
| Nour   | TUIO + Markers |
| Nada   | Hand Tracking |
| Maram  | Gesture Recognition |
| Bahgat | Integration & Logic |
| Awad   | GUI & UX |

---

## ⭐ Key Idea

This is **not a traditional application**.

It is a **multi-modal interaction system** that combines:

- Physical interaction (TUIO markers)  
- Vision-based interaction (gestures)  
- Context-aware intelligence (user + logic)  

---

## 🚀 Future Improvements

- 🗄️ Add database integration  
- 🎨 Improve UI/UX design  
- ✋ Add more gesture controls  
- 🤖 Enhance intelligent recommendations  

---

## 💡 Final Note

This project demonstrates how multiple technologies can be combined to create a **real interactive experience**, similar to modern smart museums.
