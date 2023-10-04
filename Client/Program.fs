// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net
open System.Net.Sockets
open System.Threading.Tasks
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
    let connect () = 
            
            let tcpClient = new TcpClient(serverAddress, port)
            let stream = tcpClient.GetStream()
            Console.Write("Enter a command (e.g., 'add 5 8'): ")
            async{ // the first async : receive message from the server 
            
                while is_run do 
                    let bufferArray : byte[] = Array.zeroCreate 256
                    let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                    let responseData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                    Console.WriteLine("SERVER's RESPONSE {0}", responseData)

                    if responseData = "-5" then
                        is_run <- false
                
            } |> Async.Start
            
            
            let cts = new CancellationTokenSource() // use to abort the async function
            let ReadServer = async{ // the second async : send message to the server
                while is_run do
                    let message = Console.ReadLine()
                    let data : byte[] = System.Text.Encoding.ASCII.GetBytes(message)
                    stream.Write(data, 0, data.Length)
            } 
            Async.Start(ReadServer, cts.Token)

            while is_run do // keep monitoring if the client still running
                async{ // don't know how to write, fail to wait
                    do! Async.Sleep 500
                } |> ignore

            try // close the port and service to prevent dangling thread
                stream.Close() 
                tcpClient.Close()
                tcpClient.Dispose()
                cts.Cancel()
                raise BreakException
            with
                | Failure(msg: string) -> printfn "SOMETHING FAILED";
                | BreakException -> Console.WriteLine("Client Disconnected from Server")
                


ClientSideProgram.connect()