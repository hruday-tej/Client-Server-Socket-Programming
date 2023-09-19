open System
open System.Net
open System.Net.Sockets
open System.IO

try
    let ipAddress = IPAddress.Parse("127.0.0.1") // Change to the server's IP address
    let port = 12345 // Change to the server's port

    let client = new TcpClient(ipAddress.ToString(), port)
    let stream = client.GetStream()
    let reader = new StreamReader(stream)
    let writer = new StreamWriter(stream)

    writer.AutoFlush <- true

    let receiveResponse () =
        try
            let response = reader.ReadLine()
            Console.WriteLine("Server response: " + response)
        with
        | :? IOException -> () // Handle server disconnect
        | _ -> ()

    receiveResponse ()

    let rec processCommands () =
        try
            Console.Write("Type to exit: ")
            let command = Console.ReadLine().Trim() // Trim leading and trailing whitespace
            writer.WriteLine(command)
            writer.Flush()


        with
        | :? IOException -> () // Handle server disconnect
        | _ -> ()

    processCommands ()

with
| :? SocketException -> () // Handle connection error
| _ -> ()
