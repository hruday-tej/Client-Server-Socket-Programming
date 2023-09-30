// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net
open System.Net.Sockets

module ClientSideProgram=
    let ipAddress = IPAddress.Parse("127.0.0.1")

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
        with
        | Failure(msg: string) -> printfn "SOMETHING FAILED";

ClientSideProgram.connect()
