using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pottencial.Invoices.UseCases.Invoices.UseCases
{
    public class InvoiceUseCases
    {
        private readonly IServiceProvider services;

        public InvoiceUseCases(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task Invoke(object request)
        {
            var requestType = request.GetType();

            var requestUseCase = $"{requestType.Name}UseCase";

            var requestUseCaseType = Assembly.GetExecutingAssembly().GetExportedTypes().FirstOrDefault(x => x.Name == requestUseCase);

            dynamic useCase = services.GetService(requestUseCaseType);

            dynamic arg = request;

            await useCase.Invoke(arg);
        }

        public async Task<T> Invoke<T>(object request)
        {
            var requestType = request.GetType();

            var requestUseCase = $"{requestType.Name}UseCase";

            var requestUseCaseType = Assembly.GetExecutingAssembly().GetExportedTypes().FirstOrDefault(x => x.Name == requestUseCase);

            dynamic useCase = services.GetService(requestUseCaseType);

            dynamic arg = request;

            return await useCase.Invoke(arg);
        }
    }
}
