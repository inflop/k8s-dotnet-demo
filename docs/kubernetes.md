# Kubernetes

## Kubernetes Architecture

- A Kubernetes cluster consists of a control plane (AKA master node) and worker nodes
- The control plane exposes the API, distributes work to worker nodes, monitors cluster and application state, ensures the desired state is maintained, and provides integration with cloud providers
- Worker nodes execute work assigned by the control plane

## Core Kubernetes Objects

- Node
- Pod
- Service
- Deployment
- ReplicaSet
- ConfigMap
- Secret
- Ingress

### Node

- A Node is a machine where computations are performed
- It can be a VM, a cloud instance, or a physical machine
- It is the environment where Pods run
- A cluster typically contains multiple Nodes

### Pod

- Represents a single instance of a process running on the cluster
- A Pod itself does not run applications - they are run by containers
- Can contain one or more containers (tightly coupled)
- Is the unit of application scaling
- Can only run within a single Node
- Is easily disposable (pet vs cattle)
- Is immutable
- We typically don't manage Pods directly

### Service

- A way to expose an application running on n-pods as a network service
- A stable networking abstraction (has DNS, IP address, and port) over ephemeral pods
- Provides load-balancing between pods
- 4 types:
  - ClusterIP (accessible only within the cluster)
  - NodePort (exposes the service on a static port of the node)
  - LoadBalancer (exposes the service using a load balancer)
  - ExternalName (returns a Canonical Name Record - redirect)

> We don't need to use LoadBalancer type to balance traffic within k8s itself. With a Service handling n Pods, k8s uses kube-proxy to distribute workload. LoadBalancer is needed for integration with an external load balancer.

### Deployment

- The primary object we work with when managing k8s
- Is a wrapper around Pods
- We describe the desired state - the Deployment Controller changes the current state to achieve the desired state (through ReplicaSet, which creates and deletes Pods)
- Provides self-healing, scaling, rolling-update, rollback

### ConfigMap

- Simple key-value configuration
- Used for storing non-sensitive data
- Allows decoupling configuration from the built image
- Does not provide encryption or encoding
- Not intended for storing large data - max 1 MiB (mebibyte)
- There are 4 ways to use it:
  - Commands inside a container
  - Environment variable for a container
  - Adding a file to the container as a volume
  - Writing code inside the Pod that uses the Kubernetes API

### Secret

- Equivalent of ConfigMap for sensitive data
- Contains small amounts of data such as passwords, tokens, keys
- Values are base64 encoded
- By default, values are stored on cluster storage in unencrypted form. Anyone with access to the Secret can read its value (encryption at REST is opt-in)

> Creating a Secret alone does not make it more secure than a ConfigMap. There are projects that allow using Secrets securely, but they are not an integral part of k8s.
