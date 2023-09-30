// For more information see https://aka.ms/fsharp-console-apps
open System;
open System.IO;
open System.Net;
open System.Net.Sockets;
open System.Text;

module ServerSideProgram=

    type Error_Incorrect_Operation_Command() =
        inherit Exception()
    type Error_Inputs_Less_Than_Two() =
        inherit Exception()
    type Error_Inputs_More_Than_Four() =
        inherit Exception()


    let exceptionHandler (array:string array) : string = 
        let mutable code = "pass"
        try
            if(array.Length = 1 && array.[0]= "bye") then code <- "-5"
            elif(array.Length = 1 && array.[0]= "terminate") then code <- "-6"
            elif array.[0] <> "add" && array.[0] <> "subtract" && array.[0] <> "multiply" then raise(Error_Incorrect_Operation_Command())
            elif array.Length < 3 then raise(Error_Inputs_Less_Than_Two())
            elif array.Length > 5 then raise(Error_Inputs_More_Than_Four())
            for i=1 to array.Length-1 do int(array.[i]) |> ignore
            code


        with
            | :? Error_Incorrect_Operation_Command -> "-1"
            | :? Error_Inputs_Less_Than_Two -> "-2"
            | :? Error_Inputs_More_Than_Four -> "-3"
            | :? FormatException -> "-4"
            | ex -> "-5"

    let operate (array : string array) : string=
        let mutable code = exceptionHandler(array)
        if code = "pass" then
            let mutable result = 0
            if array.[0] = "add" 
            then
                for i = 1 to array.Length-1 do
                    result <- int(array.[i]) + result
            elif (array.[0] = "subtract")
            then
                result <- int(array.[1])
                for i = 2 to array.Length-1 do
                    result <- result - int(array.[i])
            elif (array.[0] = "multiply")
            then
                result <- int(array.[1])
                for i = 2 to array.Length-1 do
                    result <- result * int(array.[i])
            code <- string(result)
        code

    let rec clientCommunication (client: TcpClient, clientNum: int) =
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
                    let wordArray = clientRequestData.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
                    let serverResponseData = operate (wordArray)
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
                    clientCommunication(client, clientNum)
                } |> Async.Start
        with
            | Failure(msg) -> printfn "%s" msg
            | ex -> printfn "An error occurred: %s" ex.Message

ServerSideProgram.initiateServer()