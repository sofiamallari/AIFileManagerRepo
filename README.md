# 🧠 AIFileManager.API

## 🤝 Integration Guide (Frontend + Backend)

This guide explains how to connect **Sofia’s .NET 8 Web API** backend (`AIFileManager.API`) with your **React frontend** using **Docker**.  
Both services will run locally in containers and communicate through a shared Docker network.

---

## ⚙️ Overview

**Architecture**
[ React Frontend ] ---> [ .NET 8 Web API ] ---> [ Python AI Service (optional) ]
(Port 5173) (Port 5000) (Port 8000)
---

## 🧩 Prerequisites

Both systems (backend and frontend) must have:

- 🐳 [Docker Desktop](https://www.docker.com/products/docker-desktop)
- 🧱 Docker Compose v2+
- 💻 Node.js 18+ (only required if running React locally)
- 🔧 .NET 8 SDK (only required for local API debugging)

---

## 📁 Folder Setup
project-root/
├── backend/ ← Sofia’s .NET API (AIFileManager.API)
│ ├── Dockerfile
│ ├── appsettings.json
│ ├── AIFileManager.API.csproj
│ ├── Controllers/
│ └── ...
│
└── frontend/ ← Your React project
├── Dockerfile
├── package.json
└── ...

> 💡 Make sure both folders are inside the same parent directory so Docker Compose can connect them.

---

Summary of Docker Commands:
# Create shared network
docker network create aifilemanager_net

# Backend build + run
cd backend
docker build -t aifilemanager-backend .
docker run -d --name aifilemanager-api --network aifilemanager_net -p 5000:5000 aifilemanager-backend

# Frontend build + run
cd ../frontend
docker build -t aifilemanager-frontend .
docker run -d --name aifilemanager-ui --network aifilemanager_net -p 5173:5173 aifilemanager-frontend

# OR: Run both via Compose
cd ..
docker compose up --build


## 🧱 Docker Commands (Full Setup)

Below are **all commands** you need from start to finish.  
Run them **in order** from your project root or as specified.

---

### 🐳 1️⃣ Create a Shared Docker Network
Run this once to allow backend and frontend containers to talk:

docker network create aifilemanager_net

🧠 2️⃣ Build and Run Sofia’s Backend (.NET 8 API)

Go to the backend folder (where your Dockerfile is):

cd backend

docker build -t aifilemanager-backend .

docker run -d --name aifilemanager-api --network aifilemanager_net -p 5000:5000 aifilemanager-backend

docker ps

🖥️ 3️⃣ Configure the Frontend

In your React project, create or edit your .env file:

VITE_API_BASE_URL=http://aifilemanager-api:5000

If you’re running the React app locally (without Docker), instead use:
VITE_API_BASE_URL=http://localhost:5000

Your frontend should use this variable, e.g.:

const API_URL = import.meta.env.VITE_API_BASE_URL;
const response = await fetch(`${API_URL}/api/files`);


4️⃣ Build and Run the React Frontend in Docker

Go to the frontend folder:

cd ../frontend


Build the frontend image:

docker build -t aifilemanager-frontend .


Run the frontend container:

docker run -d --name aifilemanager-ui --network aifilemanager_net -p 5173:5173 aifilemanager-frontend


✅ The app will now be available at:
👉 http://localhost:5173

It will automatically connect to the backend via the shared Docker network.

🐋 5️⃣ (Optional) Run Both via Docker Compose

If you want to launch everything with a single command, create a docker-compose.yml in your root folder:

version: '3.9'

services:
  backend:
    build: ./backend
    container_name: aifilemanager-api
    ports:
      - "5000:5000"
    networks:
      - aifilemanager_net

  frontend:
    build: ./frontend
    container_name: aifilemanager-ui
    ports:
      - "5173:5173"
    environment:
      - VITE_API_BASE_URL=http://backend:5000
    depends_on:
      - backend
    networks:
      - aifilemanager_net

networks:
  aifilemanager_net:
    external: true


Then simply run:

docker compose up --build

✅ This automatically builds both images and starts both containers connected to the same network.

🔍 Testing Connection

Open http://localhost:5173

The frontend should call the backend at http://backend:5000 (inside Docker).

If you see your drive list or test data → ✅ Everything works!

