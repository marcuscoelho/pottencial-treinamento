namespace Pottencial.Invoices.Borders.Invoices.Requests
{
    public class GetInvoices
    {
        public GetInvoices()
        {
            Take = 10;
        }

        public string Customer { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
