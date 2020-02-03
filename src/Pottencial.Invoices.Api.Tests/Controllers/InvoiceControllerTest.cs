using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pottencial.Invoices.Api.Tests.Fakers;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Repositories.Invoices.Context;
using Xunit;

namespace Pottencial.Invoices.Api.Tests.Controllers
{
    public class InvoiceControllerTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public InvoiceControllerTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;

            Seed();
        }

        private void Seed()
        {
            _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetService<InvoiceDbContext>();

            context.Invoices.RemoveRange(context.Invoices);

            context.Invoices.Add(InvoiceFaker.Create(1, "Joao"));
            context.Invoices.Add(InvoiceFaker.Create(2, "Joao"));
            context.Invoices.Add(InvoiceFaker.Create(3, "Joao", status: Status.Cancelled));
            context.Invoices.Add(InvoiceFaker.Create(4, "Maria"));
            context.Invoices.Add(InvoiceFaker.Create(5, "Maria", status: Status.Cancelled));
            context.Invoices.Add(InvoiceFaker.Create(6, "Maria", status: Status.Submitted));
            context.Invoices.Add(InvoiceFaker.Create(7, "Jose"));
            context.Invoices.Add(InvoiceFaker.Create(8, "Jose", status: Status.Cancelled));
            context.Invoices.Add(InvoiceFaker.Create(9, "Jose", status: Status.Submitted));
            context.Invoices.Add(InvoiceFaker.Create(10, "Pedro"));
            context.Invoices.Add(InvoiceFaker.Create(11, "Pedro", status: Status.Cancelled));
            context.Invoices.Add(InvoiceFaker.Create(12, "Pedro", status: Status.Submitted));

            context.SaveChanges();
        }

        [Fact]
        [Trait("Method", "GET")]
        public async Task Must_Return_Success_On_Get_Invoices()
        {
            var client = _factory.CreateClient();

            var customer = "Joao";

            using var response = await client.GetAsync($"invoice?customer={customer}");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            dynamic result = JArray.Parse(content);

            foreach (var invoice in result)
            {
                Assert.NotEqual((int)Status.Cancelled, (int)invoice.status);
            }
        }

        [Theory]
        [Trait("Method", "GET")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Must_Return_Success_On_Get_Invoice_By_Valid_Number(int number)
        {
            var client = _factory.CreateClient();

            using var response = await client.GetAsync($"invoice/{number}");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Method", "GET")]
        public async Task Must_Return_Not_Found_On_Get_Invoice_By_Inexistent_Number()
        {
            var client = _factory.CreateClient();

            using var response = await client.GetAsync($"invoice/{999}");

            var content = await response.Content.ReadAsStringAsync();

            Assert.Throws<HttpRequestException>(() => response.EnsureSuccessStatusCode());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "GET")]
        [InlineData("a")]
        [InlineData("%")]
        [InlineData("_")]
        public async Task Must_Return_Bad_Request_On_Get_Invoice_By_Invalid_Number(string number)
        {
            var client = _factory.CreateClient();

            using var response = await client.GetAsync($"invoice/{number}");

            var content = await response.Content.ReadAsStringAsync();

            var exception = Assert.Throws<HttpRequestException>(() => response.EnsureSuccessStatusCode());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Success_On_Post_Valid_Invoice()
        {
            var request = new
            {
                invoice = new
                {
                    number = 99,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [Trait("Method", "POST")]
        [InlineData("a")]
        [InlineData("%")]
        [InlineData("_")]
        [InlineData("0")]
        [InlineData("-1")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Invalid_Number(string number)
        {
            var request = new
            {
                invoice = new
                {
                    number,
                    date = DateTime.Now,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "POST")]
        [InlineData("2019-01-01")]
        [InlineData("2919-01-01")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Invalid_Date(string date)
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "POST")]
        [InlineData(-1.00)]
        [InlineData(0.00)]
        [InlineData(1199.99)]
        [InlineData(1200.01)]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Invalid_Amount(decimal amount)
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "POST")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Invalid_Customer(string customer)
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer,
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "POST")]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Invalid_Status(int status)
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer = "Ana",
                    status,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Empty_Items()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new object[0]
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Missing_Number()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 3,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Invalid_Number()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = "a",
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = "b",
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Missing_Description()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Invalid_Quantity()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = -1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Invalid_Unit_Price()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = -1000.00m,
                            amount = -1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Post_Invoice_With_Item_Invalid_Amount()
        {
            var request = new
            {
                invoice = new
                {
                    number = 5,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.01m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PostAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Ok_On_Submitting_Invoice()
        {
            var client = _factory.CreateClient();

            var request = new
            { 
                number = 4
            };

            using var response = await client.PostAsJsonAsync($"invoice/submit", request);

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Bad_Request_On_Submitting_Cancelled_Invoice()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                number = 5
            };

            using var response = await client.PostAsJsonAsync($"invoice/submit", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Ok_On_Submitting_Submitted_Invoice()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                number = 6
            };

            using var response = await client.PostAsJsonAsync($"invoice/submit", request);

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Method", "POST")]
        public async Task Must_Return_Not_Found_On_Submitting_Inexisting_Invoice()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                number = 99
            };

            using var response = await client.PostAsJsonAsync($"invoice/submit", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "DELETE")]
        public async Task Must_Return_Ok_On_Cancelling_Invoice()
        {
            var client = _factory.CreateClient();

            var invoice = 7;

            using var response = await client.DeleteAsync($"invoice/cancel/{invoice}");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Method", "DELETE")]
        public async Task Must_Return_Bad_Request_On_Cancelling_Cancelled_Invoice()
        {
            var client = _factory.CreateClient();

            var invoice = 8;

            using var response = await client.DeleteAsync($"invoice/cancel/{invoice}");

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "DELETE")]
        public async Task Must_Return_Ok_On_Cancelling_Submitted_Invoice()
        {
            var client = _factory.CreateClient();

            var invoice = 9;

            using var response = await client.DeleteAsync($"invoice/cancel/{invoice}");

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [Trait("Method", "DELETE")]
        public async Task Must_Return_Not_Found_On_Cancelling_Inexisting_Invoice()
        {
            var client = _factory.CreateClient();

            var invoice = 99;

            using var response = await client.DeleteAsync($"invoice/cancel/{invoice}");

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Success_On_Put_Valid_Invoice()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today.AddDays(-5),
                    amount = 1200.00m,
                    customer = "Pedro modificado",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [Trait("Method", "PUT")]
        [InlineData("a")]
        [InlineData("%")]
        [InlineData("_")]
        [InlineData("0")]
        [InlineData("-1")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Invalid_Number(string number)
        {
            var request = new
            {
                invoice = new
                {
                    number,
                    date = DateTime.Now,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "PUT")]
        [InlineData("2919-01-01")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Invalid_Date(string date)
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "PUT")]
        [InlineData(-1.00)]
        [InlineData(0.00)]
        [InlineData(1199.99)]
        [InlineData(1200.01)]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Invalid_Amount(decimal amount)
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [Trait("Method", "PUT")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Invalid_Customer(string customer)
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer,
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Cancelled_Invoice()
        {
            var request = new
            {
                invoice = new
                {
                    number = 11,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Submitted_Invoice()
        {
            var request = new
            {
                invoice = new
                {
                    number = 12,
                    date = DateTime.Today,
                    amount = 1200.00m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Empty_Items()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new object[0]
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Missing_Number()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 3,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Invalid_Number()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = "a",
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = "b",
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Missing_Description()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Invalid_Quantity()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = -1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Invalid_Unit_Price()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = -1000.00m,
                            amount = -1000.00m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Method", "PUT")]
        public async Task Must_Return_Bad_Request_On_Put_Invoice_With_Item_Invalid_Amount()
        {
            var request = new
            {
                invoice = new
                {
                    number = 10,
                    date = DateTime.Today,
                    amount = 0.0m,
                    customer = "Ana",
                    status = 1,
                    items = new[]
                    {
                        new
                        {
                            number = 1,
                            description = "pen",
                            quantity = 1.00m,
                            unitPrice = 1000.00m,
                            amount = 1000.01m
                        },
                        new
                        {
                            number = 2,
                            description = "pencil",
                            quantity = 100.00m,
                            unitPrice = 2.00m,
                            amount = 200.00m
                        }
                    }
                }
            };

            var client = _factory.CreateClient();

            using var response = await client.PutAsJsonAsync("invoice", request);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
