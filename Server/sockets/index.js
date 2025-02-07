const userSocketHandler = require("./userSocket");
const WorkerManager = require("../services/workers");

const workerManager = new WorkerManager();

module.exports = (io) => {
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

module.exports.workerManager = workerManager;