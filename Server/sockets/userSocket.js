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

    socket.on("getWorkers", () => {
        console.log(workerManager.getAvailableWorkers())
        socket.emit("workerList", workerManager.getAvailableWorkers());
    });
  };
  