using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/* Made by Chloe Parbst for the RAWDATA 2019 Assignment 3
 * Roskilde University
 * */

namespace Assignment3TestSuite
{


    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }

        public string Date { get; set; }
        public string Body { get; set; }

    }

    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("cid")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

    }

    class MyTcpListener
    {

        public static List<Category> categories;

        private static Response CheckRequest(string data)
        {
            var request = new Request();
            var response = new Response();

            List<string> reason = new List<string>();
            string code = null;

            if (data.Contains("method"))
            {
                Console.WriteLine("METHOD FOUND");
                request.Method = data.FromJson<Request>().Method.ToLower();
            }
            else
            {
                code = "4";
                reason.Add("missing method");
            }

            
            request.Path = data.FromJson<Request>().Path;

            if (data.Contains("date"))
            {
                request.Date = data.FromJson<Request>().Date;
            }
            else
            {
                code = "4";
                reason.Add("missing date");
            }
            request.Body = data.FromJson<Request>().Body;

            Console.WriteLine("Method: " + request.Method);
            Console.WriteLine("Path: " + request.Path);
            Console.WriteLine("Date: " + request.Date);
            Console.WriteLine("Body: " + request.Body);

     

            if (request.Method == null|| request.Date == null)
            {
              

                var result = code;
                for (int i = 0; i < reason.Count; i++)
                {
                    result += " " + reason[i] + ",";

                }
                response.Status = result;
            }
            else
            {
                response = GetMethod(request);
            }

            return response;
        }

        private static Response GetMethod(Request request)
        {
            var response = new Response();

            //The error codes for the date. If no date is entered, error code 4. If the format is wrong error 5
            
            foreach (char c in request.Date)
            {
                //Since unix time is only numbers
                if (!char.IsDigit(c))
                {
                    response.Status = "5 illegal date";
                    return response;
                }
            }
            

            switch (request.Method)
            {
                case "create":
                    
                    return Create(request);

                case "read":
                   
                    return Read(request);

                case "update":
                    
                     return Update(request);
                    
                case "delete":

                    return Delete(request);

                case "echo":
                  
                    return Echo(request);

                default:
                    response.Status = "5 illegal method";
                    
                    return response;

            }
        }

        private static Response Create(Request request)
        {
            var response = new Response();
            List<string> reason = new List<string>();
            string code = null;
            bool error = false;
            
            if (request.Body == null)
            {
                if (code == null) code = "4";
                reason.Add("missing body");
                error = true;
                
            }
            if (request.Path == null)
            {
                if (code == null) code = "4";
                reason.Add("missing resource");
                error = true;
            }
            
            if(!error)
            {
                var path = GetPath(request.Path);
                if (path[1] != null && path[1].Contains("api"))
                {
                    if (path[2] != null && path[2].Contains("categories"))
                    {
                        if (path[3] != null)
                        {
                            if (code == null) code = "4";
                            reason.Add("Bad Request");
                        }

                        
                        else
                        {

                            var name = request.Body.FromJson<Category>().Name;

                            Category newItem = new Category { Id = categories.Count+1, Name = name };

                            categories.Add(newItem);
                            response.Body= JsonSerializer.Serialize<Category>(newItem);

                            if (code == null) code = "2";
                            reason.Add("Created");
                        }

                    }
                    else
                    {
                        if (code == null) code = "4";
                        reason.Add("Bad Request");
                    }
                }
                else
                {
                    if (code == null) code = "4";
                    reason.Add("Bad Request");
                }
            }

            var result = code;
            for (int i = 0; i < reason.Count; i++)
            {
                result += " " + reason[i] + ",";

            }
            if (result.EndsWith(","))
            {
                result = result.Remove(result.Length-1);
            }
            response.Status = result;

            return response;
        }


        private static Response Delete(Request request)
        {
            var response = new Response();
            List<String> reason = new List<string>();
            string code = null;
            bool error = false;

            if (request.Path == null)
            {
                if (code == null) code = "4";
                reason.Add("missing resource");
                error = true;
            }
            
            if(!error)
            {
                var path = GetPath(request.Path);

                if (path[1] != null && path[1].Contains("api"))
                {
                    if (path[2] != null && path[2].Contains("categories"))
                    {

                        if (path[3] != null)
                        {
                            int num = 0;
                            if (Int32.TryParse(path[3], out num))
                            {
                                
                                Console.WriteLine("There's " + categories.Count + "items in the categories");
                                foreach(Category cat in categories)
                                {
                                    Console.WriteLine("Checking for individual id. Path number is: " + num + ", checking " + cat.Id);

                                    if (num == cat.Id)
                                    {
                                        Console.WriteLine("FOUND IT! "+cat.Name);

                                        categories.Remove(cat);
                                        response.Status = "1 Ok";
                                        return response;

                                    }
                                    else
                                    {
                                        Console.WriteLine("Nope, not " + cat.Id + ", " + cat.Name);
                                       
                                    }
                                }
                                response.Status = "5 Not Found";
                                return response;
                            }

                        }
                        else
                        {
                            if (code == null) code = "4";
                            reason.Add("Bad Request");
                        }
                    }
                    else
                    {
                        if (code == null) code = "4";
                        reason.Add("Bad Request");
                    }
                }
                else
                {
                    if (code == null) code = "4";
                    reason.Add("Bad Request");
                }

            }

            var result = code;
            for (int i = 0; i < reason.Count; i++)
            {
                result += " " + reason[i] + ",";

            }
            if (result.EndsWith(","))
            {
                result = result.Remove(result.Length - 1);
            }
            response.Status = result;


            return response;
        }


        private static Response Echo(Request request)
        {
            var response = new Response();
            if (request.Body == null)
            {
                response.Status = "4: missing body";
                response.Body = null;
                return response;
            }


            response.Status = "1";
            response.Body = request.Body;

            return response;
        }

        private static string[] GetPath(string path)
        {

            var paths = new string[4];
            var temp = path.Split("/");
            for (int i = 0; i < paths.Length; i++)
            {
                if(i < temp.Length)
                {
                    paths[i] = temp[i];
                }
                else
                {
                    paths[i] = null;
                }
            }
            return paths;

        }


        private static Response Read(Request request)
        {
            var response = new Response();
            bool error = false;

            if (request.Path == null)
            {
                response.Status = "4 missing resource";
                error = true;
            }
            
            if(!error)
            {
                var path = GetPath(request.Path);
                
                if(path[1] != null && path[1].Contains("api"))
                {
                    if(path[2] != null && path[2].Contains("categories"))
                    {
                        if(path[3] != null)
                        {

                            
                            int num = 0;
                            if (Int32.TryParse(path[3], out num)) 
                            {
                                foreach(Category cat in categories)
                                {
                                    Console.WriteLine("Checking for individual id. Path number is: "+num+", checking "+cat.Id);

                                    if(num == cat.Id)
                                    {
                                        Console.WriteLine("Found an element with the requested id!");

                                        response.Body = JsonSerializer.Serialize<Category>(cat);
                                        response.Status = "1 Ok";
                                        return response;

                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                response.Status = "5 not found";
                                return response;
                            }
                            else
                            {
                                response.Status = "4 Bad Request";
                            }
                                
                            

                        }
                        else
                        {
                            //read all the items from category
                            response.Body += "[";
                            for (int i = 0; i < categories.Count; i++)
                                {
                                    var cat = categories[i];

                                    response.Body += JsonSerializer.Serialize<Category>(cat);

                                //Makes sure we don't add a space, after the final element
                                    if(i<categories.Count-1) response.Body += ",";

                                }

                            response.Body += "]"; 


                            Console.WriteLine(response.Body);
                            response.Status = "1 Ok";
                        }
                    }
                    else
                    {
                        response.Status = "4 Bad Request";
                    }
                }
                else
                {
                    response.Status = "4 Bad Request";
                }

            }

            return response;
        }


        private static Response Update(Request request)
        {
            var response = new Response();
            List<String> reason = new List<string>();
            string code = null;
            bool error = false;

            if (request.Body == null)
            {
                if (code == null) code = "4";
                reason.Add("missing body");
                error = true;
            }
            else if (!(request.Body.StartsWith("{") && request.Body.EndsWith("}")))
            {
                if (code == null) code = "5";
                reason.Add("illegal body");
                error = true;
            }

            if (request.Path == null)
            {
                if (code == null) code = "4";
                reason.Add("missing resource");
                error = true;
            }
            
            if(!error)
            {
                var path = GetPath(request.Path);

                if (path[1] != null && path[1].Contains("api"))
                {
                    if (path[2] != null && path[2].Contains("categories"))
                    {
                        if (path[3] != null)
                        {
                            int num = 0;
                            if (Int32.TryParse(path[3], out num))
                            {

                                Console.WriteLine("There's " + categories.Count + "items in the categories");
                                foreach (Category cat in categories)
                                {
                                    Console.WriteLine("Checking for individual id. Path number is: " + num + ", checking " + cat.Id);

                                    if (num == cat.Id)
                                    {
                                        Console.WriteLine("FOUND IT! will update" + cat.Name);

                                        //UPDATE HERE
                                        cat.Id = request.Body.FromJson<Category>().Id;
                                        cat.Name = request.Body.FromJson<Category>().Name;
                                        Console.WriteLine("UPDATE INFO TO: "+cat.Id+", "+cat.Name);

                                        response.Status = "3 Updated";
                                        return response;

                                    }
                                    else
                                    {
                                        Console.WriteLine("Nope, not " + cat.Id + ", " + cat.Name);
                                        
                                    }
                                }
                                response.Status = "5 Not Found";
                                return response;
                            }
                        }
                        else
                        {
                            if (code == null) code = "4";
                            reason.Add("Bad Request");
                        }
                    }
                    else
                    {
                        if (code == null) code = "4";
                        reason.Add("Bad Request");
                    }
                }
                else
                {
                    if (code == null) code = "4";
                    reason.Add("Bad Request");
                }

            }

            var result = code;
            for (int i = 0; i < reason.Count; i++)
            {
                result += " " + reason[i] + ",";

            }
            if (result.EndsWith(","))
            {
                result = result.Remove(result.Length - 1);
            }
            response.Status = result;


            return response;
        }


        private static void ProcessClientRequests(object arg)
        {
            TcpClient client = (TcpClient)arg;
            // Buffer for reading data
            Byte[] bytes = new Byte[2048];
            String data = null;

            NetworkStream stream = client.GetStream();

            try
            {
                int i;
                // Loop to receive all the data sent by the client.

                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {

                    data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);


                    var response = CheckRequest(data);

                    data = response.ToJson();

                    Console.WriteLine("Server Response Status: " + response.Status);
                    Console.WriteLine("Server Response Body: " + response.Body);


                    byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);


                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data);
                    break;
                }
            }
            catch(IOException e)
            {
                Console.WriteLine("Excpetion "+e.GetType().Name);
            }
            
            // Shutdown and end connection
            Console.WriteLine("CLIENT LEFT");
            client.Close();
        }

        private static void SetupCategories()
        {
            categories = new List<Category>();

            categories.Add(new Category { Id = 1, Name = "Beverages" });
            categories.Add(new Category { Id = 2, Name = "Condiments" });
            categories.Add(new Category { Id = 3, Name = "Confections" });
        }

        public static void Main()
        {
            TcpListener server = null;
            try
            {
                

                // Set the TcpListener on port 13000.
                Int32 port = 5000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();
                SetupCategories();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    //Threading
                    Thread t = new Thread(ProcessClientRequests);
                    t.Start(client);

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
            

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }

    public static class Util
    {
        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static T FromJson<T>(this string element)
        {
            return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
