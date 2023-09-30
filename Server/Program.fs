// For more information see https://aka.ms/fsharp-console-apps
open System;
open System.IO;
open System.Net;
open System.Net.Sockets;
open System.Text;

module ServerSideProgram=
    let operate (input: string, clientNum: int) =
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
        | [| "multiply"; x; y; z |] ->
            let result = int x * int y * int z
            Console.WriteLine("Responding to client {0} with result: {1}", clientNum, result)
            sprintf "%d" result
        | _ ->
            "Invalid command"

    let rec handleClient (client: TcpClient, clientNum: int) =
        try
            let stream = client.GetStream()
            let bufferArray : byte[] = Array.zeroCreate 256
            let mutable continueProcessing = true

            let handleRequest () =
                let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                if bytes = 0 then
                    // The client has disconnected, so stop processing
                    Console.WriteLine("Client {0} has disconnected.", clientNum)
                    continueProcessing <- false
                else
                    let clientRequestData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                    Console.WriteLine("Received From Client {0}: {1}", clientNum, clientRequestData)

                    let serverResponseData = operate (clientRequestData, clientNum)
                    let msg = System.Text.Encoding.ASCII.GetBytes(serverResponseData)
                    stream.Write(msg, 0, msg.Length)
                    Console.WriteLine("Response Sent to Client {0}", clientNum)

            while continueProcessing do
                handleRequest()

        with
            | ex ->
                Console.WriteLine("An error occurred with Client {0}: {1}", clientNum, ex.Message)
                // Handle the error and continue processing other clients if needed


    let initiateServer() =
        try
            let port = 13000;
            let ipAddress = IPAddress.Parse("127.0.0.1")
            let server = new TcpListener(ipAddress, port)
            server.Start()
            Console.WriteLine("Server is running and listening on port {0}.", port)
            let mutable clientNum = 0
            while true do
                Console.Write("Waiting for a connection... ")
                let client = server.AcceptTcpClient()
                clientNum <- clientNum + 1
                Console.WriteLine("Connected")
                async {
                    do! Async.SwitchToThreadPool()
                    handleClient(client, clientNum)
                } |> Async.Start
        with
            | Failure(msg) -> printfn "%s" msg
            | ex -> printfn "An error occurred: %s" ex.Message

ServerSideProgram.initiateServer()