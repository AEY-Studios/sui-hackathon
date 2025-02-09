# Sui Hackathon Project

Welcome to the Sui Hackathon project repository. This project is organized into two main components:

- **Client**: The frontend application.
- **Server**: The backend application.

## Repository Structure

- `Client/`: Contains the frontend code.
- `Server/`: Contains the backend code.

## Technologies Used

- **C#**: Utilized in the backend server application.
- **JavaScript**: Utilized in the frontend client application.

## Getting Started

To set up and run the project locally, follow the instructions below.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (for the server)
- [Node.js](https://nodejs.org/) (for the client)

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/AEY-Studios/sui-hackathon.git
   cd sui-hackathon
   ```

2. **Set up the Server**:

   ```bash
   cd Server
   # Restore .NET dependencies
   dotnet restore
   # Build the server application
   dotnet build
   ```

3. **Set up the Client**:

   ```bash
   cd ../Client
   # Install Node.js dependencies
   npm install
   ```

### Running the Applications

1. **Start the Server**:

   ```bash
   cd Server
   # Run the server application
   dotnet run
   ```

2. **Start the Client**:

   ```bash
   cd Client
   # Run the client application
   npm start
   ```

# Command-Line Interface (CLI)

The server includes a command-line interface (CLI) to manage various functionalities. The available commands are:

- **`exit`**: Stops the application.
- **`status`**: Prints the current socket status.
- **`workers`**: Lists all workers.
- **`models`**: Lists available models.
- **`test --model <model> --message <message>`**: Runs a local test with a specified model.
- **`pull --model <model>`**: Pulls a model.
- **`allocate --message <message> --includeme <bool>`**: Allocates work to a worker.
- **`help`**: Lists all available commands.


## Contributing

We welcome contributions to enhance this project. To contribute:

1. Fork the repository.
2. Create a new branch: `git checkout -b feature/YourFeature`.
3. Commit your changes: `git commit -m 'Add YourFeature'`.
4. Push to the branch: `git push origin feature/YourFeature`.
5. Open a pull request.

## License

This project is licensed under the [MIT License](LICENSE).

## Contact

For questions or suggestions, please open an issue in this repository.
