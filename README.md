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
