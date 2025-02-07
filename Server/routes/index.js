const express = require("express");
const router = express.Router();
const userRoutes = require("./userRoutes");

router.get("/", (req, res) => {
  res.send({ message: "API is running!" });
});

router.use("/users", userRoutes);

module.exports = router;
