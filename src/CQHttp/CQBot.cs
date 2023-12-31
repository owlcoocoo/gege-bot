﻿using CQHttp.DTOs;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQHttp
{
    public class CQBot
    {
        /// <summary>
        /// 异常处理
        /// </summary>
        public event Action<Exception, CQSession> Exception = null;
        /// <summary>
        /// 接收私聊和群组的消息
        /// </summary>
        public event Action<CQEventMessageEx> ReceivedMessage = null;
        /// <summary>
        /// 接收私聊的消息
        /// </summary>
        public event Action<CQEventMessagePrivate> ReceivedPrivateMessage = null;
        /// <summary>
        /// 接收群组的消息
        /// </summary>
        public event Action<CQEventMessageGroup> ReceivedGroupMessage = null;
        /// <summary>
        /// 接收请求，加好友请求，加群请求／邀请。
        /// </summary>
        public event Action<CQEventRequest> ReceivedRequest = null;
        /// <summary>
        /// 处理群禁言，返回 true 不处理消息
        /// </summary>
        public Func<CQEventMessageEx, bool> HandleGroupBan { get; set; } = null;

        private readonly ClientWebSocket webSocket = new ClientWebSocket();

        private readonly ConcurrentDictionary<string, CQAPIContext> contexts = new ConcurrentDictionary<string, CQAPIContext>();
        public CQSession Session { get; private set; } = new CQSession();


        public CQBot(string uri)
        {
            webSocket.ConnectAsync(new Uri(uri), CancellationToken.None).Wait();
        }

        public Task SendAsync(CQRequest req)
        {
            if (req == null) return Task.CompletedTask;

            string json = Json.ToJsonString(req);
            var data = Encoding.UTF8.GetBytes(json);
            return webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async void Send(CQAPIContext req)
        {
            contexts.TryAdd(req.Id, req);
            await SendAsync(req.Request);
        }

        private void HandleMetaEvent(string meta_event_type, string json)
        {
            switch (meta_event_type)
            {
                case CQMetaRventType.Lifecycle:
                    //CQEventLifecycle lifecycle = Json.FromJsonString<CQEventLifecycle>(json);
                    break;
                case CQMetaRventType.Heartbeat:
                    //CQEventHeartbeat heartbeat = Json.FromJsonString<CQEventHeartbeat>(json);
                    break;
                default:
                    break;
            }
        }

        private bool isHandlingMessageQueue = false;
        private void HandleMessageQueue()
        {
            try
            {
                var obj = Session.MessageQueue.Dequeue();
                Session.CurrentMessage = obj;

                bool isBanned = false;
                if (HandleGroupBan != null)
                {
                    isBanned = HandleGroupBan(obj);
                }

                if (!isBanned)
                {
                    ReceivedMessage.Invoke(obj);

                    if (ReceivedPrivateMessage != null && obj.message_type == CQMessageType.Private)
                    {
                        string json = Json.ToJsonString(obj);
                        var message = Json.FromJsonString<CQEventMessagePrivate>(json);
                        ReceivedPrivateMessage.Invoke(message);
                    }
                    else if (ReceivedGroupMessage != null && obj.message_type == CQMessageType.Group)
                    {
                        string json = Json.ToJsonString(obj);
                        var message = Json.FromJsonString<CQEventMessageGroup>(json);
                        ReceivedGroupMessage.Invoke(message);
                    }
                }

                Session.CurrentMessage = null;
            }
            catch (Exception ex)
            {
                Exception?.Invoke(ex, Session);
            }

            if (Session.MessageQueue.Count > 0) HandleMessageQueue();
            else
            {
                isHandlingMessageQueue = false;
                
                GC.Collect();
            }
        }

        private void HandleMessage(string json)
        {
            CQEventMessageEx obj = Json.FromJsonString<CQEventMessageEx>(json);
            Session.MessageQueue.Enqueue(obj);

            if (!isHandlingMessageQueue)
            {
                isHandlingMessageQueue = true;
                Task.Run(HandleMessageQueue);
            }
        }

        private void HandleRequest(string json)
        {
            CQEventRequest request = Json.FromJsonString<CQEventRequest>(json);
            ReceivedRequest?.Invoke(request);
            GC.Collect();
        }

        private void HandleResponse(string json)
        {
            CQResponse response = Json.FromJsonString<CQResponse>(json);
            if (string.IsNullOrEmpty(response.echo)) return;
            contexts.TryRemove(response.echo, out CQAPIContext context);
            if (context != null)
            {
                response.FireCallBack(context);
            }
            GC.Collect();
        }

        private void NewTask(Action action)
        {
            Task.Run(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Exception?.Invoke(e, Session);
                }
            });
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                WebSocketReceiveResult result = null;
                StringBuilder sb = new StringBuilder();
                var buffer = new byte[1024];

                do
                {
                    try
                    {
                        result = webSocket.ReceiveAsync(buffer, CancellationToken.None).Result;
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var bytes = new byte[result.Count];
                            Array.Copy(buffer, bytes, bytes.Length);
                            sb.Append(Encoding.UTF8.GetString(bytes));

                            if (result.EndOfMessage)
                            {
                                string json = sb.ToString();

                                CQEventMeta cqEventBase = Json.FromJsonString<CQEventMeta>(json);
                                switch (cqEventBase.post_type)
                                {
                                    case CQPostType.Message:
                                        HandleMessage(json);
                                        break;
                                    case CQPostType.MessageSent:
                                        break;
                                    case CQPostType.Request:
                                        NewTask(() => HandleRequest(json));
                                        break;
                                    case CQPostType.Notice:
                                        break;
                                    case CQPostType.MetaEvent:
                                        HandleMetaEvent(cqEventBase.meta_event_type, json);
                                        break;
                                    default:
                                        NewTask(() => HandleResponse(json));
                                        break;
                                }

                                sb.Clear();

                                GC.Collect();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sb.Clear();
                        Exception?.Invoke(e, Session);
                    }
                }
                while (result == null || !result.CloseStatus.HasValue);

                Close();
            }, TaskCreationOptions.LongRunning);
        }

        private async void Close()
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            webSocket.Dispose();
        }
    }
}
