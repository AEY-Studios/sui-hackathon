module.exports = (io, socket, workerManager) => {
    socket.on("sendMessage", (data) => {
      console.log(`Message from ${socket.id}: ${data.message}`);
      
      io.emit("receiveMessage", { userId: socket.id, message: data.message });
    });
  
    socket.on("joinRoom", (room) => {
      socket.join(room);
      console.log(`${socket.id} joined room: ${room}`);
      io.to(room).emit("roomMessage", { message: `${socket.id} has joined the room.` });
    });
    
    socket.on("allocateWork", (data) => {
        console.log("allocation Request: " + data)
        const workerSocket = workerManager.getOneRandomAvailableWorker();
        if (workerSocket) {
            io.to(workerSocket.id).emit("workAllocation", {model:"deepseek-r1:7b", message: data});
          } else {
            console.error(`Worker with ID ${workerId} not found.`);
          }
    });

    socket.on("getWorkers", () => {
        console.log(workerManager.getAvailableWorkers())
        socket.emit("workerList", workerManager.getAvailableWorkers());
    });
  };
  