const express = require("express");
const router = express.Router();

router.get("/", async (req, res) => {
  try {
    res.status(200);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

router.post("/", async (req, res) => {
  try {
    res.status(201);
  } catch (error) {
    res.status(400).json({ error: error.message });
  }
});

module.exports = router;
