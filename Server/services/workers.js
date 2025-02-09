class WorkerManager {
    constructor() {
        this.workers = [];
    }

    addWorker(id, data = {}) {
        this.workers.push({ id, available: true, data });
    }

    getAvailableWorkers() {
        return this.workers.filter(worker => worker.available);
    }
    getOneRandomAvailableWorkerWitoutMe(excludedId) {
        const availableWorkers = this.getAvailableWorkers().filter(worker => worker.id !== excludedId);
        if (availableWorkers.length === 0) return null;
        const randomIndex = Math.floor(Math.random() * availableWorkers.length);
        return availableWorkers[randomIndex];
    }

    getOneRandomAvailableWorker() {
        const availableWorkers = this.getAvailableWorkers();
        if (availableWorkers.length === 0) return null;
        const randomIndex = Math.floor(Math.random() * availableWorkers.length);
        return availableWorkers[randomIndex];
    }

    setWorkerStatus(id, available) {
        const worker = this.workers.find(worker => worker.id === id);
        if (worker) {
            worker.available = available;
        } else {
            console.log(`Worker with ID ${id} not found.`);
        }
    }

    getWorkerData(id) {
        const worker = this.workers.find(worker => worker.id === id);
        return worker ? worker.data : null;
    }

    removeWorker(id) {
        this.workers = this.workers.filter(worker => worker.id !== id);
    }
}

module.exports = WorkerManager;
