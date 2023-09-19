open System
open System.Net
open System.Net.Sockets
open System.IO

let handleClient (clientSocket: TcpClient) =
    let stream = clientSocket.GetStream()
    let reader = new StreamReader(stream)
    let writer = new StreamWriter(stream)

    writer.AutoFlush <- true

    // Send a welcome message to the client
    writer.WriteLine("Hello!")
    writer.Flush()

    // writer.WriteLine(reader.ReadLine())

    let rec receiveClientMessage () =
        let client_msg = reader.ReadLine()
        Console.WriteLine("Client "+clientSocket.Client.RemoteEndPoint.ToString()+" says:")
        Console.WriteLine(client_msg)
        receiveClientMessage()

    receiveClientMessage()


let serverMain port =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = new TcpListener(ipAddress, port)

    listener.Start()
    Console.WriteLine("Server is running and listening on port " + port.ToString())

    let rec acceptClients () =
        try
            let clientSocket = listener.AcceptTcpClient()
            Console.WriteLine("Client connected: " + clientSocket.Client.RemoteEndPoint.ToString())

            async {
                handleClient clientSocket
                clientSocket.Close()
                Console.WriteLine("Client disconnected: " + clientSocket.Client.RemoteEndPoint.ToString())
                acceptClients ()
            } |> Async.Start

            acceptClients ()
        with
        | :? SocketException -> () // Handle listener stopping
        | _ -> acceptClients ()

    acceptClients ()

serverMain 12345 // Replace with the desired port number
