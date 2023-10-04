// For more information see https://aka.ms/fsharp-console-apps
open System;
open System.IO;
open System.Net;
open System.Net.Sockets;
open System.Text;
open System.Threading.Tasks
open System.Threading

module ServerSideProgram=
    let mutable is_server_run = true  // self-definition exception
    type Error_Incorrect_Operation_Command() =
        inherit Exception()
    type Error_Inputs_Less_Than_Two() =
        inherit Exception()
    type Error_Inputs_More_Than_Four() =
        inherit Exception()


    let exceptionHandler (array:string array) : string = // if there is a system command, such as "bye" and "terminate", or an exception, it would return the error code. Otherwise, it will return pass
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

    let operate (array : string array) : string= // Since all possible errors has been handled, we could calculate the client's command.
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
            elif (array.[0] = "bye")
            then
                result <- -5
            elif (array.[0] = "terminate")
            then
                result <- -5
            code <- string(result)
        code

    let rec clientCommunication (client: TcpClient, clientNum: int, server: TcpListener) =

        let stream = client.GetStream()
        let bufferArray : byte[] = Array.zeroCreate 256
        let mutable continueProcessing = true

        async{
            while continueProcessing && is_server_run do
                try
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
                        
                        if serverResponseData = "-6" then
                            is_server_run <- false
                        else
                            let msg = System.Text.Encoding.ASCII.GetBytes(serverResponseData)
                            stream.Write(msg, 0, msg.Length)
                            Console.WriteLine("Responding to Client {0} with result: {1}", clientNum, serverResponseData)
                        // Console.WriteLine("Response Sent to Client {0}", clientNum)
                with
                | ex -> 
                    Console.WriteLine("Client {0} disconnected brutely. :(",  clientNum)
                    continueProcessing <- false
        } |> Async.Start

        while continueProcessing && is_server_run do // keep tracking 1) the server is still running and 2) server still connect to the client
            async{ // but the sleep doesn't work
                do! Async.Sleep 500
            } |> ignore
            
        if is_server_run = false then // if the server is closed, then send error code -5 to the current client this async function connected
            let msg = System.Text.Encoding.ASCII.GetBytes("-5")
            stream.Write(msg, 0, msg.Length)
            Console.WriteLine("Responding to Client {0} with result: {1}", clientNum, "-5")
            stream.Close() // close all connections and ports
            client.Close()
            client.Dispose() 

  


    let initiateServer() =
        let cts = new CancellationTokenSource() // used to abort async fucntion
        let port = 13000;
        let ipAddress = IPAddress.Parse("127.0.0.1")
        let server = new TcpListener(ipAddress, port)
        server.Start()
        Console.WriteLine("Server is running and listening on port {0}.", port)
        let mutable clientNum = 0
        let runServer = async{
            try
                while is_server_run do
                    Console.WriteLine("Waiting for a connection... ")
                    let client = server.AcceptTcpClient()
                    clientNum <- clientNum + 1
                    Console.WriteLine("Client {0} Connected",clientNum)
                    async {
                        do! Async.SwitchToThreadPool()
                        clientCommunication(client, clientNum, server)
                    } |> Async.Start
            with
                | Failure(msg) -> printfn "%s" msg
                | ex -> printfn "An error occurred: %s" ex.Message
        }
        Async.Start(runServer, cts.Token) // add cts.Token to async function, bind them together so we could use cts.Cancel() to abort the async function


        while is_server_run do // Keep checking if the server is still running
            async{ // but the sleep doesn't work
                do! Async.Sleep 500
            } |> ignore

        server.Stop() // close server
        cts.Cancel()
            

        

ServerSideProgram.initiateServer()