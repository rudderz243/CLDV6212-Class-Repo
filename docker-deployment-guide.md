# Visual Studio Container Deployment and Azurite Setup Guide

This guide details the procedure for building, publishing, and executing a containerized Visual Studio application alongside an Azurite storage emulator using Docker Hub.

---

## 1. Publishing the Application Image to Docker Hub

1. Authenticate with your Docker Hub account via your terminal:
   ```bash
   docker login
   ```
2. Note your Docker Hub username.
3. Open your solution in Visual Studio.
4. In the **Solution Explorer**, right-click the solution node and select **Open Folder in File Explorer**.
5. Within the root folder containing your solution file, open a terminal window.
6. Build and tag the Docker image using the project Dockerfile:
   ```bash
   docker build -t <dockerhub_username>/<lowercase_assignment_name>:v1.0 -f <project_name>/Dockerfile .
   ```
   _Example:_
   ```bash
   docker build -t gwyndolin7/tablefunctiong2:v1.0 -f TableFunctionG2/Dockerfile .
   ```
   _Expected output:_
   ```text
   => => unpacking to docker.io/gwyndolin7/tablefunctiong2:v1.0
   ```
7. Push the local image to your remote Docker Hub repository:
   ```bash
   docker push <dockerhub_username>/<lowercase_assignment_name>:v1.0
   ```
   _Example:_
   ```bash
   docker push gwyndolin7/tablefunctiong2:v1.0
   ```
   _Expected output:_
   ```text
   v1.0: digest: sha256:4477b20a455d389614f9d4e5b1d0f7d6a1c08691053e3d175baa87e252bbc343 size: 856
   ```

---

## 2. Azurite Storage Configuration and Networking

When containerized services communicate with local emulators, standard loopback addresses (`localhost` or `127.0.0.1`) resolve internally to the application container rather than the host system. To bridge this boundary, route storage endpoints through `host.docker.internal`.

### Connection String Template

```text
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;
```

---

## 3. Preparing the Azurite Container Image

1. Pull the official Microsoft Azurite image:
   ```bash
   docker pull mcr.microsoft.com/azure-storage/azurite:latest
   ```
   _Expected output:_
   ```text
   Status: Downloaded newer image for mcr.microsoft.com/azure-storage/azurite:latest
   ```
2. Tag the base image under your personal repository namespace:
   ```bash
   docker tag mcr.microsoft.com/azure-storage/azurite:latest <dockerhub_username>/azurite:v1.0
   ```
   _Example:_
   ```bash
   docker tag mcr.microsoft.com/azure-storage/azurite:latest gwyndolin7/azurite:v1.0
   ```
   _Note: A successful command returns a clean prompt with no standard output._
3. Push your repository tag to Docker Hub:
   ```bash
   docker push <dockerhub_username>/azurite:v1.0
   ```
   _Example:_
   ```bash
   docker push gwyndolin7/azurite:v1.0
   ```
   _Expected output:_
   ```text
   v1.0: digest: sha256:f9791d0a0613e93057c06ae31f7ac0c0222f0530590e67e020b0986c66a36fa6 size: 2415
   ```
4. Verify deployment availability by pulling the image:
   ```bash
   docker pull <dockerhub_username>/azurite:v1.0
   ```
   _Example:_
   ```bash
   docker pull gwyndolin7/azurite:v1.0
   ```
   _Expected output:_
   ```text
   Status: Downloaded newer image for gwyndolin7/azurite:v1.0
   ```

---

## 4. Running the Multi-Container Environment

1. Ensure both images are present locally:
   ```bash
   docker pull <dockerhub_username>/<lowercase_assignment_name>:v1.0
   docker pull <dockerhub_username>/azurite:v1.0
   ```
2. Start the Azurite container in detached mode with published ports for Blob (10000), Queue (10001), and Table (10002) endpoints:
   ```bash
   docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 <dockerhub_username>/azurite:v1.0
   ```
   _Example:_
   ```bash
   docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 gwyndolin7/azurite:v1.0
   ```
3. Validate the emulator via **Azure Storage Explorer**:
   - Expand **Emulator & Attached** -> **Emulator Default Ports**.
   - Confirm that **Blob Containers**, **Queues**, and **Tables** directories appear without connection faults.
4. Launch the application container with the storage connection string and runtime configuration injected as environment variables:
   ```bash
   docker run -d --name tablefunction -p 34303:80 \
     -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;" \
     -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" \
     <dockerhub_username>/<lowercase_assignment_name>:v1.0
   ```
   _Example:_
   ```bash
   docker run -d --name tablefunction -p 34303:80 \
     -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;" \
     -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" \
     gwyndolin7/tablefunctiong2:v1.0
   ```
5. Send test requests to `http://localhost:34303` using an API client (e.g., Postman) to verify application functionality.

---

## 5. When Making Changes

Follow this cycle whenever source code changes occur:

1. **Build:** Recompile and build a new container image from the updated source.
2. **Push:** Upload the updated tag to Docker Hub.
3. **Pull:** Fetch the fresh build onto your target runtime system.
4. **Remove:** Stop and remove the existing container instance using Docker Desktop or `docker rm -f <container_name>`.
5. **Run:** Instantiate a new container instance referencing the updated image.
