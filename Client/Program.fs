// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net
open System.Net.Sockets
open System.Threading
exception BreakException

module ClientSideProgram=
    open System.Threading
    open System.Text
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let serverAddress = "127.0.0.1"
    let port = 13000
    let cancellationTokenSource = new CancellationTokenSource()

    let checkConnection(currentClient : TcpClient) =
        while true do
            try
                let client = new TcpClient()
                client.Connect(serverAddress, port)
                let stream = client.GetStream()
                let sendData = Encoding.ASCII.GetBytes("Heartbeat")
                stream.Write(sendData, 0, sendData.Length)
                client.Close()
            with
            | :? SocketException -> 
                cancellationTokenSource.Cancel()
                printf "SERVER IS DISCONNECTED"
                currentClient.Close()
                Environment.Exit(0)
            Thread.Sleep(2000)
    let connect () = 
        try
            let port = 13000;
            let tcpClient = new TcpClient("127.0.0.1", port)
            while true do
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
                        tcpClient.Close()
                        raise BreakException
        with
        | Failure(msg: string) -> printfn "SOMETHING FAILED";
        | BreakException -> Console.WriteLine("Client Disconnected from Server")

ClientSideProgram.connect()