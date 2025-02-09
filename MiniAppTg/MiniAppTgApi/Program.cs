using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace MiniAppTgApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.MapPost("/api/issue-card", async ([FromBody] IssueCardRequest request) =>
            {
                using (var httpClient = new HttpClient())
                {
                    var payload = new
                    {
                        UserId = request.UserId,
                        CardType = "Virtual"
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

                    var response = await httpClient.PostAsync("https://api.example.com/issue-card", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Results.Ok(new { success = true, message = "Карта успешно выпущена!", data = result });
                    }
                    else
                    {
                        return Results.BadRequest(new { success = false, message = "Ошибка при выпуске карты." });
                    }
                }
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        public class IssueCardRequest
        {
            public string UserId { get; set; }
        }
    }
}
