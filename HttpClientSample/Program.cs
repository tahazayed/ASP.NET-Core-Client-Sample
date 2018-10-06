using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


namespace HttpClientSample
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    internal class Program
    {
        private static readonly HttpClientHandler Handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };
        private static readonly HttpClient Client = new HttpClient(Handler, false);

        // Create the JSON formatter.
        private static readonly MediaTypeFormatter JsonFormatter = new JsonMediaTypeFormatter();

        private static void ShowProduct(Product product)
        {
            Console.WriteLine($"Name: {product.Name}\tPrice: " +
                $"{product.Price}\tCategory: {product.Category}");
        }

        private static async Task<Uri> CreateProductAsync(Product product)
        {

            HttpResponseMessage response = await Client.PostAsync(
                "api/products", product, JsonFormatter, "application/json").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        private static async Task<Product> GetProductAsync(string path)
        {
            Product product = null;
            HttpResponseMessage response = await Client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<Product>();
            }
            return product;
        }

        private static async Task<List<Product>> GetProductsAsync(string name)
        {
            List<Product> products = null;

            HttpResponseMessage response = await Client.GetAsync($"api/products/{name}");
            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadAsAsync<List<Product>>();
            }
            return products;
        }

        private static async Task<Product> UpdateProductAsync(Product product)
        {
            HttpResponseMessage response = await Client.PutAsync(
                $"api/products/{product.Id}", product, JsonFormatter, "application/json");
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            product = await response.Content.ReadAsAsync<Product>();
            return product;
        }

        private static async Task<HttpStatusCode> DeleteProductAsync(string id)
        {
            HttpResponseMessage response = await Client.DeleteAsync(
                $"api/products/{id}");
            return response.StatusCode;
        }

        private static void Main()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var sp = ServicePointManager.FindServicePoint(new Uri("http://localhost:5962/"));
            sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
            // Update port # in the following line.
            Client.BaseAddress = new Uri("http://localhost:5962/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            Client.DefaultRequestHeaders.ConnectionClose = false;

            Client.Timeout = TimeSpan.FromMinutes(30);

            try
            {

                using (var cts = new CancellationTokenSource())
                {
                    using (new Timer(callback: _ => cts.Cancel(), state: null, dueTime: Timeout.Infinite, period: Timeout.Infinite))
                    {
                        var options = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount,
                            CancellationToken = cts.Token
                        };

                        Parallel.For(0, 500, options,
                            async i =>
                            {
                                Product tempProduct = new Product
                                {
                                    Name = $"Gizmo{i}",
                                    Price = 100,
                                    Category = "Widgets"
                                };
                                try
                                {
                                    var tempUrl = await CreateProductAsync(tempProduct);

                                    Console.WriteLine($"Created at {tempUrl}, {tempProduct.Name}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Exception at {tempProduct.Name}, {ex.Message}");
                                }
                            });
                    }
                }
                //for (int i = 0; i < 5000; i++)
                //{
                //    Product tempProduct = new Product
                //    {
                //        Name = $"Gizmo{i}",
                //        Price = 100,
                //        Category = "Widgets"
                //    };
                //    var tempUrl = await CreateProductAsync(tempProduct);
                //    Console.WriteLine($"Created at {tempUrl}");
                //}

                List<Product> products = await GetProductsAsync("Giz");
                foreach (var pro in products)
                {
                    // Delete the product
                    var tempStatusCode = await DeleteProductAsync(pro.Id);
                    Console.WriteLine($"Deleted {pro.Id} (HTTP Status = {(int)tempStatusCode})");
                }

                // Create a new product
                Product product = new Product
                {
                    Name = "Gizmo",
                    Price = 100,
                    Category = "Widgets"
                };

                var url = await CreateProductAsync(product);
                Console.WriteLine($"Created at {url}");

                // Get the product
                product = await GetProductAsync(url.PathAndQuery);
                ShowProduct(product);

                // Update the product
                Console.WriteLine("Updating price...");
                product.Price = 80;
                await UpdateProductAsync(product);

                // Get the updated product
                product = await GetProductAsync(url.PathAndQuery);
                ShowProduct(product);

                // Delete the product
                var statusCode = await DeleteProductAsync(product.Id);
                Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
