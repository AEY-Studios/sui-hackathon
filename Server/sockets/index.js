const userSocketHandler = require("./userSocket");
const WorkerManager = require("../services/workers");

const workerManager = new WorkerManager();

let ioInstance = null;

module.exports = (io) => {
  ioInstance = io
  io.on("connection", (socket) => {
    console.log(`Client connected: ${socket.id}`);

    workerManager.addWorker(socket.id)
    userSocketHandler(io, socket, workerManager);

    socket.on("disconnect", () => {
        workerManager.removeWorker(socket.id)
        console.log(`Client disconnected: ${socket.id}`);
    });
  });
};

module.exports.sendEventToWorker = (workerId, event, message) => {
    if (!ioInstance) {
      console.error("Socket.IO instance is not initialized.");
      return;
    }
    const workerSocket = workerManager.getWorkerData(workerId)
    if (workerSocket) {
      io.to(workerId).emit(event, {model:"deepseek-r1:7b", message: message});
    } else {
      console.error(`Worker with ID ${workerId} not found.`);
    }
  };

module.exports.workerManager = workerManager;