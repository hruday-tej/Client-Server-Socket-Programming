open System
open System.Net
open System.Net.Sockets
exception BreakException
let port = 13000;
let tcpClient = new TcpClient("127.0.0.1", port)

Console.WriteLine("Connected value is {0}", tcpClient.Connected);