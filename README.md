# AIFileManager API

A .NET 8 Web API that allows users to explore, manage, and analyze their local files directly from Docker or a standard host environment. The project supports directory scanning, file operations (move, delete, metadata retrieval), and optional AI-powered file analysis modules.

---

## ⚙️ Requirements

* **.NET 8 SDK**
* **Docker Desktop** (Windows / Linux / macOS)
* **Git** (optional)

---

## 🚀 How to Build and Run

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/<your-username>/AIFileManagerRepo.git
cd AIFileManagerRepo
```

### 2️⃣ Build the Docker Image

```bash
docker build -t aifilemanager-backend .
```

### 3️⃣ Run the Container

```bash
docker run -d --name aifilemanager-api -p 5000:5000 aifilemanager-backend
```

### 4️⃣ Open Swagger

👉 [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)

You can now test all available endpoints.

---

## 🧩 Docker Compose (Optional)

To run multiple services (e.g., backend + frontend):

```bash
docker compose up --build -d
```

To stop everything:

```bash
docker compose down
```

---

## 🧱 API Overview

| Endpoint                                  | Description                       |
| ----------------------------------------- | --------------------------------- |
| `/api/Storage/getDrive`                   | Lists available drives            |
| `/api/Storage/getFolderList?path=/path`   | Lists folders inside a directory  |
| `/api/Storage/getFileList?path=/path`     | Lists files in a folder           |
| `/api/Storage/getFileMetadata?path=/path` | Shows metadata and partial hashes |
| `/api/Storage/deleteFile?path=/path`      | Deletes or recycles a file        |
| `/api/Storage/deleteFolder?path=/path`    | Deletes or recycles a folder      |

---

## ⚡ Quick Start — “Easy Use” Commands

This section is for anyone who just wants to **run the API and check files** in a specific local folder quickly using Docker.

### 🧱 1️⃣ Create the Docker Network (Only Once)

```bash
docker network create aifilemanager_net
```

If it already exists, Docker will show:

```
Error response from daemon: network with name aifilemanager_net already exists
```

✅ That’s fine — skip to the next step.

---

### 🏗️ 2️⃣ Build the Project

From your backend folder (where the `Dockerfile` and `.sln` are located):

```bash
docker build -t aifilemanager-backend .
```

This creates a Docker image of your **AIFileManager API**.

---

### 💾 3️⃣ Run the API and Mount Any Folder You Want to Check

When you run the container, you can mount **any folder on your host** (e.g., `Downloads`, `Documents`, or a custom project folder).

#### Example: Mount `Downloads` folder

```bash
docker run -d --name aifilemanager-api \
  --network aifilemanager_net \
  -p 5000:5000 \
  -v "C:\\Users\\User\\Downloads:/app/files" \
  aifilemanager-backend
```

#### Example: Mount `Documents` folder

```bash
docker run -d --name aifilemanager-api \
  --network aifilemanager_net \
  -p 5000:5000 \
  -v "C:\\Users\\User\\Documents:/app/files" \
  aifilemanager-backend
```

🧠 **Explanation:**

| Flag                                         | Description                                 |
| -------------------------------------------- | ------------------------------------------- |
| `--network aifilemanager_net`                | Connects container to shared Docker network |
| `-p 5000:5000`                               | Maps host port 5000 → container port 5000   |
| `-v "C:\\Users\\User\\Downloads:/app/files"` | Mounts local folder into the container      |
| `aifilemanager-backend`                      | The Docker image name you built earlier     |

---

### 🌐 4️⃣ Open and Use the API

Once the container is running, open:
👉 [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)

Then try this:

```
GET /api/Storage/getFileList?path=/app/files
```

✅ It will show the list of files from the folder you mounted (e.g., your Downloads or Documents).

---

### 🧹 5️⃣ Stop and Remove Containers (Optional)

When you’re done testing:

```bash
docker stop aifilemanager-api
docker rm aifilemanager-api
```

If you want to start again, just re-run the `docker run ...` command.

---

### 🧠 Notes

* The folder you mount **can be changed anytime** by editing the path in the `-v` option.
* You can mount **multiple drives** if needed by extending the volume list in `docker-compose.yml`.
* Inside Docker, your mounted folder is always located at:

  ```
  /app/files
  ```
* Swagger calls should use:

  ```
  path=/app/files
  ```

---

✅ **Now your AIFileManager API can run anywhere, list any folder you mount, and be managed entirely from Docker.**
