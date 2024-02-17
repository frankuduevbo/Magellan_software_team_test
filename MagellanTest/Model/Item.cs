using Microsoft.Extensions.Hosting;

namespace MagellanTest.Model
{
    public class Item
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int?  ParentItem { get; set; }
        public int Cost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
