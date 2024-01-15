using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace KT_IMAPClient;

public class IMAP
{
    private string host;
    private int port;

    protected int commandNumber = 0;
    protected string? output = string.Empty;
    protected string? outputLine = string.Empty;

    private Socket? socket;

    private SslStream? sslStream;
    private StreamReader? readStream;
    private Stream? imapStream;

    private byte[] buffer = new byte[4096];

    public IMAP(string host, int port)
    {
        this.host = host;
        this.port = port;
    }

    public bool Connect(UserInfo userInfo)
    {
        try
        {
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress hostIP = Dns.GetHostAddresses(host)[0];
            IPEndPoint ipEndPoint = new(hostIP, port);

            Console.WriteLine("Client connecting...");

            socket.Connect(ipEndPoint);

            sslStream = new(new NetworkStream(socket));
            sslStream.AuthenticateAsClient(host);

            imapStream = sslStream;
            readStream = new StreamReader(imapStream);

            string? output = readStream.ReadLine();

            Console.WriteLine("Server: " + output);

            if (output != null)
            {
                if (output.StartsWith("* OK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);

            Disconnect();

            return false;
        }
    }

    public void Disconnect()
    {
        commandNumber = 0;

        if (sslStream != null)
        {
            sslStream.Close();
        }

        if (imapStream != null)
        {
            imapStream.Close();
        }

        if (readStream != null)
        {
            readStream.Close();
        }

        if (socket != null)
        {
            socket.Close();
        }
    }

    protected bool SendIMAP(string commandRaw)
    {
        commandNumber++;

        string commandSend = "CMD" + commandNumber.ToString() + " " + commandRaw;

        Console.Write("Client: " + commandSend);

        buffer = Encoding.UTF8.GetBytes(commandSend);

        try
        {
            if (imapStream != null)
            {
                imapStream.Write(buffer);
            }
            else
            {
                return false;
            }

            string? output = ReceiveIMAP();

            if (output != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);

            Disconnect();

            return false;
        }
    }

    protected string? ReceiveIMAP()
    {
        bool EndOfMessage = false;

        try
        {
            while (!EndOfMessage)
            {
                if (readStream != null)
                {
                    outputLine = readStream.ReadLine();
                }

                output += outputLine + "\r\n";

                if (outputLine != null)
                {
                    if (outputLine.StartsWith("CMD" + commandNumber + " " + "OK"))
                    {
                        Console.WriteLine("Server: " + outputLine);

                        EndOfMessage = true;
                    }
                    else if (outputLine.StartsWith("CMD" + commandNumber + " " + "BAD") || outputLine.StartsWith("CMD" + commandNumber + " " + "NO"))
                    {
                        Console.WriteLine("Server: " + outputLine);

                        EndOfMessage = true;

                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Server: " + outputLine);
                    }
                }
            }

            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);

            Disconnect();

            return string.Empty;
        }
    }
}
