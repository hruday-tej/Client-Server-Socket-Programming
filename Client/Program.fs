open System
open System.Net
open System.Net.Sockets
open System.Threading.Tasks
open System.Text
open System.Windows
open System.Threading

exception BreakException

module ClientSideProgram=
    open System.Threading
    open System.Text
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let serverAddress = "127.0.0.1"
    let port = 13000
    let mutable is_run = true

    // Function to parse an integer from a string or return None if parsing fails
    let parseIntOrDefault(s: string) =
        match Int32.TryParse(s) with
        | true, i -> Some i
        | _ -> None

    let connect () = 
            // Define a dictionary to map error codes to error messages
            let errorMessages = 
                dict [
                    -1, "incorrect operation command.";
                    -2, "number of inputs are less than two.";
                    -3, "number of inputs are more than four.";
                    -4, "one or more of the inputs contain(s) non-number(s).";
                    -5, "exit."
                ]

            let tcpClient = new TcpClient(serverAddress, port)
            let stream = tcpClient.GetStream()
            async {
                while is_run do 
                    Console.Write("Enter a command (e.g., 'add 5 8'): ")
                    let bufferArray : byte[] = Array.zeroCreate 256
                    let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                    let responseData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)

                    // Attempt to parse the responseData as an integer or set it to -1 if parsing fails
                    let errorCode =
                        match parseIntOrDefault(responseData) with
                        | Some code -> code
                        | None -> -1
                    // Check if the errorCode is in the dictionary and display the corresponding error message
                    if errorMessages.ContainsKey(errorCode) then
                        Console.WriteLine("{0}: {1}", errorCode, errorMessages.[errorCode])
                    else
                        Console.WriteLine("SERVER's RESPONSE {0}", responseData)

                    if responseData = "-5" then
                        is_run <- false
            } |> Async.Start
            let cts = new CancellationTokenSource()
            let ReadServer = async{
                while is_run do
                    let message = Console.ReadLine()
                    let data : byte[] = System.Text.Encoding.ASCII.GetBytes(message)
                    stream.Write(data, 0, data.Length)
            } 
            Async.Start(ReadServer, cts.Token)

            while is_run do
                async{
                    do! Async.Sleep 500
                } |> ignore

            try
                stream.Close()
                tcpClient.Close()
                tcpClient.Dispose()
                cts.Cancel()
                raise BreakException
            with
                | Failure(msg: string) -> printfn "SOMETHING FAILED";
                | BreakException -> Console.WriteLine("Client Disconnected from Server")

ClientSideProgram.connect()
