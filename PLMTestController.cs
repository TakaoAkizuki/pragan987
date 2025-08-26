using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PakerIoT.Controllers
{
    public class PLMTestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TestConnection([FromBody] PLMConnectionTest request)
        {
            try
            {
                // Устанавливаем адрес PLM сервера
                PLMNS.PLM.plmServer = request.ServerAddress;

                // Тестируем простой запрос к PLM
                string testUrl = $"http://{request.ServerAddress}/Service_GetObject?id=IO.1";
                
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(testUrl);
                webRequest.Timeout = 10000; // 10 секунд таймаут
                
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string responseText = reader.ReadToEnd();
                            
                            return Json(new
                            {
                                success = true,
                                message = "Подключение к PLM серверу успешно!",
                                serverAddress = request.ServerAddress,
                                responseCode = (int)response.StatusCode,
                                responseLength = responseText.Length,
                                sampleResponse = responseText.Length > 200 ? responseText.Substring(0, 200) + "..." : responseText
                            });
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Ошибка подключения к PLM серверу: {ex.Message}",
                    serverAddress = request.ServerAddress,
                    errorType = ex.GetType().Name
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Общая ошибка: {ex.Message}",
                    serverAddress = request.ServerAddress,
                    errorType = ex.GetType().Name
                });
            }
        }

        [HttpPost]
        public IActionResult TestCustomRequest([FromBody] PLMCustomRequest request)
        {
            try
            {
                // Устанавливаем адрес PLM сервера
                PLMNS.PLM.plmServer = request.ServerAddress;

                // Тестируем кастомный запрос
                string testUrl = $"http://{request.ServerAddress}/{request.Endpoint}";
                
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(testUrl);
                webRequest.Timeout = 10000;
                
                if (!string.IsNullOrEmpty(request.Method) && request.Method.ToUpper() == "POST")
                {
                    webRequest.Method = "POST";
                    if (!string.IsNullOrEmpty(request.Data))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(request.Data);
                        webRequest.ContentLength = data.Length;
                        webRequest.ContentType = "application/json";
                        
                        using (Stream dataStream = webRequest.GetRequestStream())
                        {
                            dataStream.Write(data, 0, data.Length);
                        }
                    }
                }
                
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string responseText = reader.ReadToEnd();
                            
                            return Json(new
                            {
                                success = true,
                                message = "Запрос выполнен успешно!",
                                serverAddress = request.ServerAddress,
                                endpoint = request.Endpoint,
                                method = request.Method ?? "GET",
                                responseCode = (int)response.StatusCode,
                                responseLength = responseText.Length,
                                response = responseText
                            });
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Ошибка запроса: {ex.Message}",
                    serverAddress = request.ServerAddress,
                    endpoint = request.Endpoint,
                    errorType = ex.GetType().Name
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Общая ошибка: {ex.Message}",
                    serverAddress = request.ServerAddress,
                    endpoint = request.Endpoint,
                    errorType = ex.GetType().Name
                });
            }
        }
    }

    public class PLMConnectionTest
    {
        public string ServerAddress { get; set; }
    }

    public class PLMCustomRequest
    {
        public string ServerAddress { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string Data { get; set; }
    }
}
