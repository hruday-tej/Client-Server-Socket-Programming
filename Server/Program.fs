﻿// For more information see https://aka.ms/fsharp-console-apps
open System;
open System.IO;
open System.Net;
open System.Net.Sockets;
open System.Text;

module ServerSideProgram=

    let initiateServer() =
        try
            let port = 13000;
            let ipAddress = IPAddress.Parse("127.0.0.1")
            let server = TcpListener(ipAddress, port)
            server.Start()
            let data = null
            while true do
                Console.Write("Waiting for a connection... ")
                let client = server.AcceptTcpClient()
                Console.WriteLine("Connected")
                let stream = client.GetStream()
                let bufferArray : Byte array = Array.zeroCreate 256
                let bytes = stream.Read(bufferArray, 0, bufferArray.Length)
                let clientResponseData = System.Text.Encoding.ASCII.GetString(bufferArray, 0, bytes)
                Console.WriteLine("Received From Client: {0}", clientResponseData)
                // while true do
                //     printfn "nn"
                let msg = System.Text.Encoding.ASCII.GetBytes("Server is Saying Hii!")
                stream.Write(msg, 0, msg.Length)
                Console.WriteLine("Sent {0} to the Client", msg)
            
            with
                | Failure(msg) -> printfn "%s" msg;

ServerSideProgram.initiateServer()