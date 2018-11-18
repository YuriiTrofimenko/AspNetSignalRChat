using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Http;
using WebSocketDemo.Models;

namespace WebSocketDemo.Controllers
{
    public class ChatController : ApiController
    {
        /*private static readonly ConcurrentQueue<StreamWriter> mStreammessageConcurrentQueue =
            new ConcurrentQueue<StreamWriter>();*/
        //Коллекция подписчиков
        private static ConcurrentBag<StreamWriter> mClients;
        private static Random mRandom;
        private static Timer mTimer;
        private static Message mCurrentMessage;

        static ChatController()
        {
            mClients = new ConcurrentBag<StreamWriter>();
            mRandom = new Random();
            //newTimer();

            //mTimer.Start();
        }
        [NonAction]
        public static void newTimer() {

            mTimer = new Timer();
            mTimer.Interval = 1000;
            //Отрабатывать ф-цию обратного вызова только первый раз
            mTimer.AutoReset = false;
            mTimer.Elapsed += timer_Elapsed;
            mTimer.Start();
        }

        //Действие добавления нового подписчика
        [HttpGet]
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            HttpResponseMessage response = request.CreateResponse();
            response.Content = new PushStreamContent(
                (stream, content, contex) => OnStreamAvailable(stream, content, contex)
                , "text/event-stream"
            );
            return response;
        }
        
        //Добавление нового сообщения от одного подписчика
        [HttpPost]
        public void Post(Message m)
        {
            m.dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            mCurrentMessage = m;
            newTimer();
        }

        [NonAction]
        public void OnStreamAvailable(Stream stream, HttpContent content, TransportContext context)
        {
            StreamWriter client = new StreamWriter(stream);
            mClients.Add(client);
        }

        //Передача нового сообщения всем подписчикам
        private async static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Перебираем потоки вывода всех подписчиков
            foreach (var client in mClients)
            {
                try
                {
                    //!!! Специальное форматирование строки данных для ее передачи клиенту
                    //через WebSocket
                    var data = string.Format("data: {0}\n\n", JsonConvert.SerializeObject(mCurrentMessage));
                    await client.WriteAsync(data);
                    await client.FlushAsync();
                }
                catch (Exception)
                {
                    StreamWriter ignore;
                    mClients.TryTake(out ignore);
                }
            }
            //Освобождаем ресурс Таймер
            mTimer.Dispose();
        }
    }
}