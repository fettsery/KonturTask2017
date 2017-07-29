using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using NLog;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        public StatServer()
        {
            listener = new HttpListener();
            requestHandler = new RequestHandler("Data.db");
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();

            requestHandler.Dispose();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    logger.Error(error, "Error when getting context of listener");
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            string response;
            listenerContext.Response.StatusCode = HandleRequest(listenerContext.Request, out response);
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
            {
                writer.WriteLine(response);
            }
        }

        private int HandleRequest(HttpListenerRequest listenerRequest, out string response)
        {
            using (var reader = new StreamReader(listenerRequest.InputStream, listenerRequest.ContentEncoding))
            {
                try
                {
                    string endPoint;
                    DateTime date;
                    switch (listenerRequest.HttpMethod)
                    {
                        case "PUT":
                            if (RequestParser.CheckServerCommand(listenerRequest.RawUrl, out endPoint))
                                return requestHandler.AddServer(endPoint, reader.ReadToEnd(), out response);
                            if (RequestParser.CheckMatchCommand(listenerRequest.RawUrl, out endPoint, out date))
                                return requestHandler.AddMatch(endPoint, date, reader.ReadToEnd(), out response);
                            break;
                        case "GET":
                            if (RequestParser.CheckServerCommand(listenerRequest.RawUrl, out endPoint))
                                return requestHandler.GetServer(endPoint, out response);
                            if (RequestParser.CheckAllServersCommand(listenerRequest.RawUrl))
                                return requestHandler.GetAllServers(out response);
                            int count;
                            if (RequestParser.CheckGetRecentMatchesCommand(listenerRequest.RawUrl, out count))
                                return requestHandler.GetRecentMatches(count, out response);
                            if (RequestParser.CheckMatchCommand(listenerRequest.RawUrl, out endPoint, out date))
                                return requestHandler.GetMatch(endPoint, date, out response);
                            if (RequestParser.CheckGetServerStatsCommand(listenerRequest.RawUrl, out endPoint))
                                return requestHandler.GetServerStats(endPoint, out response);
                            string playerName;
                            if (RequestParser.CheckGetPlayerStatsCommand(listenerRequest.RawUrl, out playerName))
                                return requestHandler.GetPlayerStats(playerName, out response);
                            if (RequestParser.CheckGetBestPlayersCommand(listenerRequest.RawUrl, out count))
                                return requestHandler.GetBestPlayers(count, out response);
                            if (RequestParser.CheckGetPopularServersCommand(listenerRequest.RawUrl, out count))
                                return requestHandler.GetPopularServers(count, out response);
                            break;
                    }
                }
                catch (JsonReaderException e)
                {
                    logger.Warn(e, "Invalid json format");
                    response = "";
                    return (int) HttpStatusCode.BadRequest;
                }
                catch (JsonWriterException e)
                {
                    logger.Error(e, "Error during serialization to json");
                    response = "";
                    return (int) HttpStatusCode.InternalServerError;
                }
                catch (JsonSerializationException e)
                {
                    logger.Warn(e, "Wrong json data");
                    response = "";
                    return 422;
                }
                catch (LiteException e)
                {
                    logger.Error(e, "Error with working with database");
                    response = "";
                    return (int) HttpStatusCode.InternalServerError;
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Unknown error");
                    response = "";
                    return (int)HttpStatusCode.InternalServerError;
                }

                response = "";
                return (int)HttpStatusCode.BadRequest;
            }
        }

        private readonly HttpListener listener;

        private readonly RequestHandler requestHandler;
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}