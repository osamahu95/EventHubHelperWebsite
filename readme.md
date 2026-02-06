# Local Azure Event Hubs Emulator + MVC Event Publisher

This repository provides a **fully local Azure Event Hubs–like development environment** using Docker, along with a **local MVC web application** hosted on IIS to inspect and publish events without relying on Azure or HTTP-triggered functions.

The goal is to make Event Hub–based development **fast, visual, and repeatable** on a single machine.

---

## ✨ What This Setup Solves

* No Azure subscription required for local development
* No Azure Functions HTTP trigger needed just to publish events
* No repeated JSON file uploads to test payloads
* Ability to **view events, sequence numbers, and payloads** locally
* Works fully offline once Docker images are pulled

---

## 🧱 Architecture Overview

```
┌──────────────┐
│  MVC Website │  (IIS – Local Machine)
│              │
│ - View events│
│ - See seq no │
│ - Paste JSON │
│ - Publish EH │
└──────┬───────┘
       │ 
┌──────▼─────────────────────────┐
│ Azure Event Hubs Emulator       │
│                                 │
│ - Namespaces                    │
│ - Event Hubs                    │
│ - Consumer Groups               │
└──────┬─────────────────────────┘
       │ Metadata / Checkpoints
┌──────▼────────┐   ┌─────────────▼────────┐
│ Azurite       │   │ Cosmos DB Emulator    │
│ (Blob/Queue) │   │ (For your development) │
└──────────────┘   └───────────────────────┘
```

---

## 📦 Components Used

| Component               | Purpose                        |
| ----------------------- | ------------------------------ |
| **Event Hubs Emulator** | Local Event Hub runtime        |
| **Azurite**             | Blob storage for metadata      |
| **Cosmos DB Emulator**  | For now onlyDevelopment        |
| **Docker Compose**      | Orchestration                  |
| **ASP.NET MVC (IIS)**   | UI to inspect & publish events |

---

## 🔧 Prerequisites

### 1. Install Docker

* **Windows / macOS**: Install **Docker Desktop**

  * [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
* Ensure Docker is running before continuing

Verify installation:

```bash
docker --version
docker compose version
```

---

### 2. Enable Windows Features (Windows Only)

If you are on Windows, ensure the following are enabled:

* Hyper-V
* Containers
* WSL 2

```powershell
wsl --install
```

Restart after enabling if required.

---

## 📁 Configuration

### Event Hubs Emulator Configuration (`config.json`)

This file defines:

* Event Hub namespaces
* Event Hubs
* Partition count
* Consumer groups

```json
{
  "UserConfig": {
    "NamespaceConfig": [
      {
        "Type": "EventHub",
        "Name": "emulatorNs1",
        "Entities": [
          {
            "Name": "eh1",
            "PartitionCount": "2",
            "ConsumerGroups": [
              { "Name": "cg-default" },
              { "Name": "orchestrator_group" },
              { "Name": "ordermgmt_group" },
              { "Name": "fullfilment_group" }
            ]
          }
        ]
      }
    ],
    "LoggingConfig": {
      "Type": "File"
    }
  }
}
```

### Docker Installations (`docker-compose.yml`)
```json
version: "3.8"

version: "3.8"

services:
  # --- Event Hubs Emulator ---
  emulator:
    container_name: eventhubs-emulator
    image: mcr.microsoft.com/azure-messaging/eventhubs-emulator:latest
    volumes:
      - "${CONFIG_PATH}:/Eventhubs_Emulator/ConfigFiles/Config.json:ro"
    ports:
      - "5672:5672"
      - "9092:9092"
    environment:
      BLOB_SERVER: azurite
      METADATA_SERVER: azurite
      ACCEPT_EULA: ${ACCEPT_EULA}
    depends_on:
      - azurite
      - cosmos-emulator # Added dependency
    networks:
      eh-emulator:
        aliases:
          - eventhubs-emulator

  # --- Azurite ---
  azurite:
    container_name: azurite
    image: mcr.microsoft.com/azure-storage/azurite:latest
    ports:
      - "10000:10000"
      - "10001:10001"
      - "10002:10002"
    networks:
      eh-emulator:
        aliases:
          - azurite

  # --- Cosmos DB Emulator (Final Corrected Version) ---
  cosmos-emulator:
    container_name: cosmos-emulator
    image: mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview
    # FIX: Explicitly tell the container to use HTTPS for the gateway and explorer
    command: ["--protocol", "https", "--explorer-protocol", "https"]
    ports:
      - "8081:8081"
      - "1234:1234"
    environment:
      - AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10
      - AZURE_COSMOS_EMULATOR_ENABLE_DATA_EXPLORER=true
      - AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=127.0.0.1
      - AZURE_COSMOS_EMULATOR_ALLOWED_ORIGINS=https://localhost:8081,https://localhost:1234
    volumes:
      - cosmos_data:/var/lib/cosmosdb
    deploy:
      resources:
        limits:
          memory: 4G

networks:
  eh-emulator:
    driver: bridge

volumes:
  cosmos_data:
```

### .env file
"# path to Config.json file (absolute preferred)
CONFIG_PATH="C:\\Users\\[User Folder Name]\\Documents\\eh-emulator\\Config.json"

# Accept EULA (must be Y for the emulator to run)
ACCEPT_EULA="Y"

📌 **This is the single source of truth for your Event Hub topology.**

---

## 🐳 Docker Compose Setup

### `docker-compose.yaml`

This file spins up:

* Event Hubs Emulator
* Azurite
* Cosmos DB Emulator

Key services:

* **Event Hubs Emulator**

  * Microsoft Azure Eventhub Emulator

* **Azurite**

  * Blob
  * Queue
  * Table

* **Cosmos DB Emulator**

  * Explorer

---

## ▶️ Running the Environment

From the root of the repository:

```bash
docker compose up -d
```

Check running containers:

```bash
docker ps
```

To stop:

```bash
docker compose down
```

---

## 🌐 Local MVC Website (IIS)

Instead of using an **Azure Function HTTP trigger**, this project uses a **local ASP.NET MVC application hosted on IIS**.

### What the MVC App Does

* Displays a list of Event Hubs
* Shows received events
* Displays **sequence numbers**
* Renders JSON payloads in a readable format
* Allows you to **paste JSON and publish directly to Event Hub**

### Why This Approach

✅ Faster local feedback loop
✅ No redeploying functions
✅ No uploading JSON files repeatedly
✅ Visual inspection of events

This makes it ideal for:

* Debugging event schemas
* Testing orchestration flows
* Local POCs and spikes

---

## 🔑 Connection Details (Local)

Example connection settings:

```text
Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```

---

## 🧪 Typical Use Case Flow

1. Start Docker environment
2. Open MVC website on IIS
3. Select Event Hub
4. Paste JSON payload
5. Publish event
6. View event list with sequence numbers
7. Validate downstream consumers

---

## 🧹 Cleanup

To remove containers and volumes:

```bash
docker compose down -v
```

---

## 📌 Notes

* Cosmos Emulator requires **at least 4GB RAM**
* First startup may take a few minutes
* HTTPS certificate warnings are expected for Cosmos Emulator

---

## 🤝 Contributions

Feel free to open issues or PRs if you want to:

* Improve the issues in this MVC Project
* Improve the docker-compose.yml file

---

## 📜 License

MIT License
