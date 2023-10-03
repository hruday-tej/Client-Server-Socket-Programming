// For more information see https://aka.ms/fsharp-console-apps
open System;
open System.IO;
open System.Net;
open System.Net.Sockets;
open System.Text;
open System.Threading;
open System.Collections.Generic;

module ServerSideProgram=

    let cancellationTokenSource = new CancellationTokenSource()
    let mutable connectedClients: TcpClient list = []
    // let mutable clientNum = 0
    let operate (input: string, clientNum: int ) =
        // while not (cancellationTokenSource.IsCancellationRequested)
        printfn "-------------"
        Console.WriteLine("{0}", input)
        let parts = input.Split(' ')
        match parts with
        | [| "add"; x; y |] ->
            let result = int x + int y
            Console.WriteLine("Responding to client {0} with result: {1}", clientNum, result)
            sprintf "%d" result
        | [| "subtract"; x; y |] ->
            let result = int x - int y
            Console.WriteLine("Responding to client {0} with result: {1}", clientNum, result)
            sprintf "%d" result
        | [| "multiply"; x; y |] ->
            let result = int x * int y 
            Console.WriteLine("Responding to client {0} with result: {1}", clientNum, result)
            sprintf "%d" result
        | [| "bye" |] ->
            sprintf "-5"
        | [| "terminate" |] ->
            sprintf "-5"
        | _ ->
            "Invalid command"

    let rec clientCommunication (client: TcpClient, server: TcpListener) =
        // while not (cancellationTokenSource.Token.IsCancellationRequested) do
        try
            let stream = client.GetStream()
            let bufferArray : byte[] = Array.zeroCreate 256
            let mutable continueProcessing = true

            let handleRequest () =
                let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                let clientRequestData: string = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                if not (clientRequestData.Equals("Heartbeat")) then
                    // The client has disconnected, so stop processing
                    // Console.WriteLine("Client {0} has disconnected.", clientNum)
                    // continueProcessing <- false
                    // printf ""
                // else
                    if bytes = 0 then
                    // The client has disconnected, so stop processing
                        // Console.WriteLine("Client {0} has disconnected.", clientNum)
                        continueProcessing <- false
                    // printf "-----------------"
                    // Console.WriteLine(clientRequestData)cle
                    else

                        let clientNum = connectedClients.Length+1
                        connectedClients <- [client] |> List.append connectedClients
                        Console.WriteLine("Received From Client {0}: {1}", clientNum, clientRequestData)
                        let serverResponseData = operate (clientRequestData, clientNum)
                        let msg = System.Text.Encoding.ASCII.GetBytes(serverResponseData)
                        stream.Write(msg, 0, msg.Length)
                        Console.WriteLine("{1} Response Sent to Client {0}", clientNum, serverResponseData)

                        let parts = clientRequestData.Split(' ')
                        match parts with
                        | [| "terminate" |] ->
                            cancellationTokenSource.Cancel()
                            server.Stop()
                        | _ ->
                            printf ""
            while continueProcessing do
                handleRequest()

        with
            | ex ->
                Console.WriteLine("An error occurred with Client {0}: {1}", connectedClients.Length+1, ex.Message)
                // Handle the error and continue processing other clients if needed


    let initiateServer() =
        try
            let port = 13000;
            let ipAddress = IPAddress.Parse("127.0.0.1")
            let server = new TcpListener(ipAddress, port)
            server.Start()
            Console.WriteLine("Server is running and listening on port {0}.", port)
            while true do
                if connectedClients.Length = 0 then
                    printfn "Waiting for a connection... (sleeping for 1 second)"
                else
                    Console.WriteLine("Connected to clients : {0}", connectedClients)
                async {
                    let client: TcpClient = server.AcceptTcpClient()
                    // client.
                    // Console.WriteLine("Connected")
                    do! Async.SwitchToThreadPool()
                    clientCommunication(client, server)
                } |> Async.Start
                Thread.Sleep(1000)

        with
            | Failure(msg) -> printfn "%s" msg
            | ex -> printfn "An error occurred: %s" ex.Message

ServerSideProgram.initiateServer()