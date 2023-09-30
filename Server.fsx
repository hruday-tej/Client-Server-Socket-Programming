open System
open System.Net
open System.Net.Sockets
open System.IO

type Error_Incorrect_Operation_Command() =
    inherit Exception()
type Error_Inputs_Less_Than_Two() =
    inherit Exception()
type Error_Inputs_More_Than_Four() =
    inherit Exception()


let exceptionHandler (array:string array) : string = 
    try
        if array.[0] <> "add" && array.[0] <> "subtract" && array.[0] <> "multiply" then raise(Error_Incorrect_Operation_Command())
        elif array.Length < 3 then raise(Error_Inputs_Less_Than_Two())
        elif array.Length > 5 then raise(Error_Inputs_More_Than_Four())
        for i=1 to array.Length-1 do int(array.[i]) |> ignore
        "0"


    with
        | :? Error_Incorrect_Operation_Command -> "-1"
        | :? Error_Inputs_Less_Than_Two -> "-2"
        | :? Error_Inputs_More_Than_Four -> "-3"
        | :? FormatException -> "-4"
        | ex -> "-5"

let calculate (array:string array) : int =
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

    result


let handleClient (clientSocket: TcpClient) =
    let stream = clientSocket.GetStream()
    let reader = new StreamReader(stream)
    let writer = new StreamWriter(stream)
    let mutable connected = true

    writer.AutoFlush <- true

    // Send a welcome message to the client
    writer.WriteLine("Hello!")
    writer.Flush()

    // writer.WriteLine(reader.ReadLine())

    let receiveClientMessage () =
        let mutable err_code = "0" // 0 = pass
        let mutable message_to_client = ""
        let client_msg = reader.ReadLine()
        Console.WriteLine("Client "+clientSocket.Client.RemoteEndPoint.ToString()+" says:")
        Console.WriteLine(client_msg)
        let wordArray = client_msg.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

        // check the argument
        if(wordArray.Length = 1 && wordArray.[0]= "bye")then
                err_code <- "-5"
        elif(wordArray.Length = 1 && wordArray.[0]= "terminate") then
                err_code <- "-6"
        else
            err_code <- exceptionHandler(wordArray)
            // if err_code = "0" then
            //     message_to_client <- string(calculate(wordArray))

        // decide what to response
        if err_code = "0" then
            message_to_client <- string(calculate(wordArray))
        elif (err_code = "-1") then
            message_to_client <- "error code -1: incorrect operation command."
        elif (err_code = "-2") then
            message_to_client <- "error code -2: number of inputs is less than two."
        elif (err_code = "-3") then
            message_to_client <- "error code -3: number of inputs is more than four."
        elif (err_code = "-4") then
            message_to_client <- "error code -4: one or more of the inputs contain(s) non-number(s)."
        elif (err_code = "-5") then
            message_to_client <- "error code -5: exit"
        elif (err_code = "-6") then
            message_to_client <- "error code -6: terminate all process and server."

        writer.WriteLine(message_to_client)
        writer.Flush()
        
    while connected do
        try
            receiveClientMessage()
        with
        | ex -> 
            Console.WriteLine(clientSocket.Client.RemoteEndPoint.ToString() + " disconnected brutely.")
            connected <- false



let serverMain port =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = new TcpListener(ipAddress, port)

    listener.Start()
    Console.WriteLine("Server is running and listening on port " + port.ToString())

    let acceptClients () =
        let mutable listening= true
        while(listening) do
            try
                let clientSocket = listener.AcceptTcpClient()
                Console.WriteLine("Client connected: " + clientSocket.Client.RemoteEndPoint.ToString())

                async {
                    handleClient clientSocket
                    
                } |> Async.Start

                
            with
            | :? SocketException -> () // Handle listener stopping
        

    acceptClients ()



serverMain 12345 // Replace with the desired port number