// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net
open System.Net.Sockets
open System.Threading
exception BreakException

module ClientSideProgram =
    open System.Threading
    open System.Text
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let serverAddress = "127.0.0.1"
    let port = 13000
    let cancellationTokenSource = new CancellationTokenSource()
    let mutable continueProcessing = true

    let checkConnection(currentClient : TcpClient) =
        try
            let stream = currentClient.GetStream()
            let sendData = Encoding.ASCII.GetBytes("heartbeat")
            stream.Write(sendData, 0, sendData.Length)
            try
                // Try reading a response from the server
                let bufferArray : byte[] = Array.zeroCreate 256
                let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                let responseData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                let test="0"
                if responseData = "-0" then
                     printfn "alive"
                else
                    printfn "SOMETHING FAILED"



            with
            | Failure(msg: string) -> printfn "SOMETHING FAILED"
            | BreakException -> Console.WriteLine("Server Disconnected")
        with
        | Failure(msg: string) -> printfn "SOMETHING FAILED"
        | BreakException -> Console.WriteLine("Server Disconnected")

    let connect () =
        try
            let port = 13000
            let tcpClient = new TcpClient(serverAddress, port)

            // Start a separate thread to check server connection
            let checkConnectionThread =
                async {
                    while continueProcessing do
                        checkConnection tcpClient
                        do! Async.Sleep(2000)
                }
            Async.Start(checkConnectionThread)

            while continueProcessing do
                Console.Write("Enter a command (e.g., 'add 5 8'): ")
                let message = Console.ReadLine()
                let data : byte[] = System.Text.Encoding.ASCII.GetBytes(message)
                let stream = tcpClient.GetStream()
                stream.Write(data, 0, data.Length)
                let bufferArray : byte[] = Array.zeroCreate 256
                let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                let responseData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                Console.WriteLine("SERVER's RESPONSE {0}", responseData)
                if responseData = "-5" then
                    Console.WriteLine("Received termination command. Exiting.")
                    continueProcessing <- false
                    tcpClient.Close()

        with
        | Failure(msg: string) -> printfn "SOMETHING FAILED"
        | BreakException -> Console.WriteLine("Client Disconnected from Server")

ClientSideProgram.connect()
 