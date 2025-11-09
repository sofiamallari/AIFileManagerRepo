# 🧠 AIFileManager.API

## 🤝 Integration Guide (Backend + Frontend)

This guide explains how to connect **Sofia’s .NET 8 Web API** backend (`AIFileManager.API`) with your **React frontend** using **Docker**.  
Both services run locally in containers and communicate through a shared Docker network.

---

## 📂 Folder Structure (Confirmed)

```
AIFileManagerRepo/
└── AIFileManager.API/
    ├── AIFileManager.API/              ← Source code and .csproj
    │   ├── AIFileManager.API.csproj
    │   ├── Controllers/
    │   ├── Services/
    │   └── Program.cs
    │
    ├── AIFileManager.API.sln           ← Solution file
    └── Dockerfile                      ← Dockerfile in this folder
```

✅ This structure ensures the Dockerfile can correctly find and build the project.

---

## ⚙️ Architecture Overview

```
[ React Frontend ] ---> [ .NET 8 Web API ] ---> [ Python AI Service (optional) ]
      (Port 5173)             (Port 5000)               (Port 8000)
```

---

## 🧩 Prerequisites

Both backend and frontend require:

- 🐳 [Docker Desktop](https://www.docker.com/products/docker-desktop)
- 🧱 Docker Compose v2+
- 💻 Node.js 18+ (only required if running React locally)
- 🔧 .NET 8 SDK (only required for local API debugging)

---

## 🐳 1️⃣ Create a Shared Docker Network

Run this once to allow containers to communicate:

```bash
docker network create aifilemanager_net
```

---

## 🧠 2️⃣ Build and Run Sofia’s Backend (.NET 8 API)

Go to the backend folder (where the Dockerfile is):

```bash
cd C:\Users\User\source\repos\AIFileManagerRepo\AIFileManager.API
```

Then run the following commands:

```bash
# Build the backend image
docker build -t aifilemanager-backend .

# Run the backend container
docker run -d --name aifilemanager-api --network aifilemanager_net -p 5000:5000 aifilemanager-backend
```

Check if it’s running:
```bash
docker ps
```

✅ Once it starts, your API should be available at:
- Local: [http://localhost:5000](http://localhost:5000)
- Inside Docker network: `http://aifilemanager-api:5000`

---

## 🖥️ 3️⃣ Configure the Frontend

In your **React project**, edit or create your `.env` file:

```env
VITE_API_BASE_URL=http://aifilemanager-api:5000
```

If running React **locally** (not in Docker):
```env
VITE_API_BASE_URL=http://localhost:5000
```

Use the variable in your API calls:
```typescript
const API_URL = import.meta.env.VITE_API_BASE_URL;
const response = await fetch(`${API_URL}/api/files`);
```

---

## 🧩 4️⃣ Build and Run the React Frontend in Docker

Go to the frontend folder:
```bash
cd ../frontend
```

Build and run the frontend container:

```bash
docker build -t aifilemanager-frontend .
docker run -d --name aifilemanager-ui --network aifilemanager_net -p 5173:5173 aifilemanager-frontend
```

✅ The React app will be available at:  
👉 [http://localhost:5173](http://localhost:5173)

It will automatically connect to the backend via the shared network.

---

## 🐋 5️⃣ Run Both via Docker Compose (Optional)

If you want a one-command setup, create a `docker-compose.yml` in your root folder:

```yaml
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
```

Then run:
```bash
docker compose up --build
```

✅ This builds and runs both backend and frontend automatically.

---

## 🧪 Testing Connection

1. Open [http://localhost:5173](http://localhost:5173)  
2. The frontend should call `http://backend:5000` inside Docker.  
3. If your drive list or test data loads — ✅ everything is working!

---

## 🧱 Full Command Summary

```bash
# Create shared network
docker network create aifilemanager_net

# Build and run backend
cd backend
docker build -t aifilemanager-backend .
docker run -d --name aifilemanager-api --network aifilemanager_net -p 5000:5000 aifilemanager-backend

# Build and run frontend
cd ../frontend
docker build -t aifilemanager-frontend .
docker run -d --name aifilemanager-ui --network aifilemanager_net -p 5173:5173 aifilemanager-frontend

# OR (easier) run both together
cd ..
docker compose up --build
```

---

## 🧰 Troubleshooting

| Issue | Cause | Fix |
|-------|--------|-----|
| 🚫 `localhost refused to connect` | API not listening on 0.0.0.0 | Add `builder.WebHost.UseUrls("http://0.0.0.0:5000");` in `Program.cs` |
| 🚫 Network not found | You forgot to create it | `docker network create aifilemanager_net` |
| 🚫 Port in use | Another process is using 5000 or 5173 | Change mapping, e.g. `-p 5001:5000` |
| 🚫 `Project file does not exist` | Wrong folder or case mismatch | Ensure folder names match exactly (`AIFileManager.API`) |
| 🚫 No response | App crashed | Check logs with `docker logs aifilemanager-api` |

---

## ✅ Final Notes

- Backend API (outside Docker): `http://localhost:5000`  
- Backend API (inside Docker): `http://aifilemanager-api:5000`  
- Frontend (React): `http://localhost:5173`  
- API Swagger UI: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

💡 **Tip:**  
When cloning, place this repo inside your `backend/` folder so it looks like:
```
project-root/
├── backend/
│   └── AIFileManager.API/
└── frontend/
```
Then all Docker commands will work exactly as written above.
