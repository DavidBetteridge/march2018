using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebApplication3.Pages
{
    public class OrderSubmission
    {
        public string EmailAddress { get; set; }
        public string ID { get; set; }
        public string PreferredLanguage { get; set; }
        public string Product { get; set; }
        public string Source { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderResponse
    {
        public string orderId { get; set; }
    }
    

    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly Settings.API _settings;
        public string OrderNumber { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]
        [Required]
        public string EmailAddress { get; set; }

        [BindProperty]
        public string PreferredLanguage { get; set; }

        [BindProperty]
        public string Product { get; set; }

        [BindProperty]
        public decimal Total { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IOptions<Settings.API> appSettings)
        {
            _logger = logger;
            _settings = appSettings.Value;
        }

        public async Task OnPost()
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.EmailAddress))
            {
                ModelState.AddModelError(nameof(EmailAddress), "Your email address must be entered");
                return;
            }

            if (string.IsNullOrEmpty(this.Product))
            {
                ModelState.AddModelError(nameof(Product), "The name of the product to order must be entered");
                return;
            }

            if (this.Total <= 0)
            {
                ModelState.AddModelError(nameof(Total), "The total must be a positive number");
                return;
            }

            try
            {

                var client = new HttpClient();
                client.BaseAddress = new Uri(_settings.Endpoint);

                var order = new OrderSubmission()
                {
                    EmailAddress = this.EmailAddress,
                    PreferredLanguage = this.PreferredLanguage,
                    Product = this.Product,
                    Total = this.Total,
                    Source = "WebSite",
                    ID = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
                    Status = "New Order"
                };

                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await client.PostAsync("/v1/order", content);
                response.EnsureSuccessStatusCode();

                var responseJSON = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<OrderResponse>(responseJSON);
                this.OrderNumber = responseObject.orderId;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Sorry your order could not be placed at the moment. Please try again later.";
                _logger.LogError(ex.ToString());
            }

        }

        public void OnGet()
        {
        }
    }
}
