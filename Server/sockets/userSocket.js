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
    
    socket.on("allocateWork", (raw) => {
      const data = JSON.parse(raw);
      let workerSocket
      console.log(data)
      if(data.includeMe){
        workerSocket = workerManager.getOneRandomAvailableWorker();
      } 
      else{
        workerSocket = workerManager.getOneRandomAvailableWorkerWitoutMe(socket.id);
      }
      if (workerSocket) {
        io.to(workerSocket.id).emit("workAllocation", {model:"deepseek-r1:7b", message: data.message, author: socket.id});
      } else {
        io.to(socket.id).emit("error", "No awailable machine!");
      }
    });

    socket.on("completeWork", (raw) => {
      console.log("completeWork Request: " + raw)
      const data = JSON.parse(raw);
      if (data.author) {
          io.to(data.author).emit("completeWork", data.message);
        } else {
          console.error(`Worker with ID ${workerId} not found.`);
        }
  });

    socket.on("getWorkers", () => {
        console.log(workerManager.getAvailableWorkers())
        socket.emit("workerList", workerManager.getAvailableWorkers());
    });
  };
  