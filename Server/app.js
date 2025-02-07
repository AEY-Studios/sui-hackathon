require("dotenv").config();
const express = require("express");
const http = require("http");
const { Server } = require("socket.io");
const routes = require("./routes"); 
const socketHandler = require("./sockets"); 
const cors = require("cors");

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: { origin: "*" }, 
});

app.use(express.json()); 
app.use(cors());

app.use("/api", routes);

socketHandler(io);

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => console.log(`Server running on port ${PORT}`));
