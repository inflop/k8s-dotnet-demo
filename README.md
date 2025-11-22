# Kube.WebApp

A sample application demonstrating Kubernetes configuration and Helm Charts usage. This project serves educational purposes and presents a typical microservices architecture deployed on a Kubernetes cluster.

> **Note:** The three-tier API architecture (WebApp → WebApi → Database.Api) is intentionally over-engineered for this simple use case. In a real-world scenario, a Todo application wouldn't require this level of separation. This structure exists purely to demonstrate Kubernetes service-to-service communication, ConfigMaps, Secrets, and Helm chart dependencies.

## Architecture

The application consists of three microservices connected in a layered architecture:

```
                                                        ┌──────────────┐
                                                        │  SQL Server  │
                                                        │  (external)  │
                                                        └──────▲───────┘
                                                               │
┌──────────────────┐      ┌──────────────────┐      ┌──────────┴───────┐
│   Kube.WebApp    │─────►│   Kube.WebApi    │─────►│ Kube.Database.Api│
│  (Razor Pages)   │      │    (REST API)    │      │     (Dapper)     │
│    port: 8082    │      │    port: 8081    │      │    port: 8080    │
└──────────────────┘      └──────────────────┘      └──────────────────┘
       Ingress                 ClusterIP                  ClusterIP
```

### Projects

| Project | Description | Technology |
|---------|-------------|------------|
| **Kube.Contracts** | Shared DTO models | .NET 6 Class Library |
| **Kube.Database.Api** | Data access layer | .NET 6 Web API + Dapper |
| **Kube.WebApi** | Business logic layer | .NET 6 Web API |
| **Kube.WebApp** | User interface | .NET 6 Razor Pages |

## Requirements

- .NET 6 SDK
- Container runtime with Kubernetes (see platform-specific setup below)
- Helm 3.x
- kubectl
- SQL Server (local or containerized)

## Platform Setup

### Windows (Rancher Desktop)

[Rancher Desktop](https://rancherdesktop.io/) provides Docker and Kubernetes on Windows without requiring a license.

**Installation:**

```powershell
# Using winget
winget install suse.RancherDesktop

# Or using Chocolatey
choco install rancher-desktop
```

**Configuration:**

1. Launch Rancher Desktop
2. In **Preferences > Kubernetes**, select your desired Kubernetes version
3. In **Preferences > Container Engine**, select **dockerd (moby)** for Docker CLI compatibility
4. Wait for Kubernetes to initialize (check the system tray icon)

**Verify installation:**

```powershell
kubectl cluster-info
docker info
```

### Linux (minikube)

[minikube](https://minikube.sigs.k8s.io/) is the simplest way to run a local Kubernetes cluster on Linux.

**Installation:**

```bash
# Debian/Ubuntu
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube

# Fedora/RHEL
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube

# Arch Linux
sudo pacman -S minikube
```

**Start cluster:**

```bash
# Start with Docker driver (recommended)
minikube start --driver=docker

# Enable ingress addon
minikube addons enable ingress

# Optional: Open Kubernetes dashboard
minikube dashboard
```

**Configure Docker environment:**

To build images directly in minikube's Docker daemon:

```bash
eval $(minikube docker-env)
```

**Verify installation:**

```bash
kubectl cluster-info
minikube status
```

## Directory Structure

```
├── Kube.Contracts/          # Shared models
├── Kube.Database.Api/
│   ├── k8s/
│   │   ├── helm/            # Helm chart
│   │   │   └── kube-database-api/
│   │   ├── deployment.yaml  # Raw K8s manifests
│   │   ├── service.yaml
│   │   └── secret.yaml
│   └── Dockerfile
├── Kube.WebApi/
│   ├── k8s/
│   │   ├── helm/
│   │   │   └── kube-webapi/
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   └── configmap.yaml
│   └── Dockerfile
├── Kube.WebApp/
│   ├── k8s/
│   │   ├── helm/
│   │   │   └── kube-webapp/
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   ├── configmap.yaml
│   │   └── ingress.yaml
│   └── Dockerfile
└── docs/
    └── kubernetes.md        # K8s documentation
```

## Configuration

### Environment Variables

| Service | Variable | Description |
|---------|----------|-------------|
| Kube.Database.Api | `ConnectionString` | SQL Server connection string |
| Kube.WebApi | `DatabaseApiUrl` | URL to Kube.Database.Api |
| Kube.WebApp | `TodoApiUrl` | URL to Kube.WebApi |

### Helm Values

Each service has its own Helm chart with configurable values in `values.yaml`:

- **replicaCount** - number of replicas
- **image.name/tag** - Docker image
- **service.type/port** - service type and port
- **resources.limits** - CPU/memory limits
- **strategy** - deployment strategy (RollingUpdate)

### Database Password Configuration

Before running the application, replace `YOUR_PASSWORD_HERE` placeholder with your actual SQL Server password in the following files:

| File | Format |
|------|--------|
| `Kube.Database.Api/appsettings.json` | Plain text |
| `Kube.Database.Api/k8s/docker-compose-sql-server.yaml` | Plain text |
| `Kube.Database.Api/k8s/secret.yaml` | Base64 encoded |
| `Kube.Database.Api/k8s/helm/kube-database-api/values.yaml` | Base64 encoded |

For base64 encoded files, encode your connection string:

```bash
# Linux/macOS
echo -n "Data Source=host.docker.internal;Initial Catalog=todo_db;User Id=sa;Password=YOUR_ACTUAL_PASSWORD;Enlist=false;TrustServerCertificate=true" | base64

# PowerShell
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("Data Source=host.docker.internal;Initial Catalog=todo_db;User Id=sa;Password=YOUR_ACTUAL_PASSWORD;Enlist=false;TrustServerCertificate=true"))
```

## Getting Started

### 1. Build Docker Images

```powershell
# Windows (PowerShell)
docker build -f .\Kube.Database.Api\Dockerfile -t kube.database.api .
docker build -f .\Kube.WebApi\Dockerfile -t kube.webapi .
docker build -f .\Kube.WebApp\Dockerfile -t kube.webapp .
```

```bash
# Linux (after running: eval $(minikube docker-env))
docker build -f ./Kube.Database.Api/Dockerfile -t kube.database.api .
docker build -f ./Kube.WebApi/Dockerfile -t kube.webapi .
docker build -f ./Kube.WebApp/Dockerfile -t kube.webapp .
```

### 2. Install Ingress Controller

Skip this step on minikube if you enabled the ingress addon.

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.2/deploy/static/provider/cloud/deploy.yaml
```

### 3. Deploy with Helm

```powershell
# Windows
helm upgrade --install kube-database-api .\Kube.Database.Api\k8s\helm\kube-database-api\
helm upgrade --install kube-webapi .\Kube.WebApi\k8s\helm\kube-webapi\
helm upgrade --install kube-webapp .\Kube.WebApp\k8s\helm\kube-webapp\
```

```bash
# Linux
helm upgrade --install kube-database-api ./Kube.Database.Api/k8s/helm/kube-database-api/
helm upgrade --install kube-webapi ./Kube.WebApi/k8s/helm/kube-webapi/
helm upgrade --install kube-webapp ./Kube.WebApp/k8s/helm/kube-webapp/
```

### 4. Access the Application

**Windows (Rancher Desktop):**

```powershell
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 8080:80
```

**Linux (minikube):**

```bash
minikube tunnel
# Or get the minikube IP and access directly
minikube ip
```

The application will be available at: http://localhost:8080 (or minikube IP)

### Alternative: Raw Kubernetes Manifests

Instead of Helm, you can use YAML manifests directly:

```bash
# Kube.Database.Api
kubectl apply -f ./Kube.Database.Api/k8s/secret.yaml
kubectl apply -f ./Kube.Database.Api/k8s/deployment.yaml
kubectl apply -f ./Kube.Database.Api/k8s/service.yaml

# Kube.WebApi
kubectl apply -f ./Kube.WebApi/k8s/configmap.yaml
kubectl apply -f ./Kube.WebApi/k8s/deployment.yaml
kubectl apply -f ./Kube.WebApi/k8s/service.yaml

# Kube.WebApp
kubectl apply -f ./Kube.WebApp/k8s/configmap.yaml
kubectl apply -f ./Kube.WebApp/k8s/deployment.yaml
kubectl apply -f ./Kube.WebApp/k8s/service.yaml
kubectl apply -f ./Kube.WebApp/k8s/ingress.yaml
```

## Useful kubectl Commands

```bash
# Pod status
kubectl get pod

# Pod details
kubectl describe pod <pod_name>

# Pod logs
kubectl logs <pod_name>

# Shell into pod
kubectl exec -it <pod_name> -- sh

# Pod environment variables
kubectl exec -it <pod_name> -- env

# Restart deployments
kubectl rollout restart deployment kube-database-api-deploy
kubectl rollout restart deployment kube-webapi-deploy
kubectl rollout restart deployment kube-webapp-deploy

# History and rollback
kubectl rollout history deployment <deployment_name>
kubectl rollout undo deployment <deployment_name>
```

## Cleanup

```bash
# Using Helm
helm uninstall kube-webapp
helm uninstall kube-webapi
helm uninstall kube-database-api

# Or manually
kubectl delete ingress kube-webapp
kubectl delete service kube-webapp kube-webapi kube-database-api
kubectl delete deploy kube-webapp kube-webapi kube-database-api
kubectl delete configmap kube-webapp kube-webapi
kubectl delete secret kube-database-api
```

## Educational Resources

Kubernetes documentation is available in [docs/kubernetes.md](docs/kubernetes.md) and covers:
- Kubernetes architecture (control plane, worker nodes)
- Core objects: `Node`, `Pod`, `Service`, `Deployment`, `ReplicaSet`, `ConfigMap`, `Secret`, `Ingress`
- Service types (`ClusterIP`, `NodePort`, `LoadBalancer`, `ExternalName`)
