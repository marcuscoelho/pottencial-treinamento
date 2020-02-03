namespace Pottencial.Invoices.Borders.Invoices.Entities
{
    public class Item
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }
    }
}
