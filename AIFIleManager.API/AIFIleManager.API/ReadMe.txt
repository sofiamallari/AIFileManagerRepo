# 
🤝 AI File Manager – Integration Guide (Frontend + Backend)

This guide explains how to connect **Sofia’s .NET Core Web API** backend with your **React frontend** using **Docker**.  
Both services will run locally in containers and communicate through a shared Docker network.

---

## 
⚙️ Overview

**Architecture**

[ React Frontend ] ---> [ .NET 8 Web API ] ---> [ Python AI Service (optional) ]
(Port 5173) (Port 5000) (Port 8000)


---

## 
🧩 Prerequisites

Both must have:

- 🐳 [Docker Desktop](https://www.docker.com/products/docker-desktop)
- 🧱 Docker Compose v2+
- 💻 Node.js 18+ (only if you want to run React locally without Docker)
- 🔧 .NET 8 SDK (only if you want to debug the API locally)

---

## 📁 Folder Setup

You should have two separate projects:
project-root/
├── backend/ ← Sofia’s .NET API (you’ll pull this repo)
│ ├── Dockerfile
│ ├── appsettings.json
│ └── ...
│
└── frontend/ ← Your React app
├── Dockerfile
├── package.json
└── ...

Make sure both folders are inside the same parent directory so Docker Compose can connect them.

---

## 
🐳 1. Create a Shared Docker Network
Run this once to make both containers discover each other:

docker network create aifilemanager_net

🐳 2. Run Sofia’s Backend (.NET API)
In the backend/ folder, run:

docker build -t aifilemanager-backend .
docker run -d --name aifilemanager-api --network aifilemanager_net -p 5000:5000 aifilemanager-backend

Once it starts, your API should be available at
→ http://localhost:5000
 (for you)
→ http://aifilemanager-api:5000
 (inside Docker network for your classmate’s frontend container)

🖥️ 3. Configure the Frontend
In your React project, open .env or .env.local and add:
VITE_API_BASE_URL=http://aifilemanager-api:5000
If you’re not using Docker and running the frontend locally, use:
VITE_API_BASE_URL=http://localhost:5000

Make sure your frontend fetch calls use this environment variable, e.g.:

const API_URL = import.meta.env.VITE_API_BASE_URL;
const response = await fetch(`${API_URL}/api/files`);

🐳 4. Run the React Frontend in Docker
In your frontend/ folder:
docker build -t aifilemanager-frontend .
docker run -d --name aifilemanager-ui --network aifilemanager_net -p 5173:5173 aifilemanager-frontend
✅ The app will be available at:
👉 http://localhost:5173

It will automatically connect to the backend through the shared Docker network.

5. (Optional) Run Both via Docker Compose
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

Then just run:
docker-compose up --build


✅ Testing Connection

Open http://localhost:5173

The frontend should call http://backend:5000 inside Docker.

If you see your drive list or test data, the setup works.

If something fails:

Run docker ps to verify both containers are up.

Use docker logs aifilemanager-api for backend logs.

Check .env in the frontend.