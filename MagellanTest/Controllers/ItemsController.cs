
using MagellanTest.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;



namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ILogger<ItemsController> _logger;
        private NpgsqlConnection conn;   
        

        // NOTE CHANGE PASSWORD TO YOUR PASSWORD
        private string connstring = String.Format("Server={0};Port={1};" +
           "User Id={2};Password={3};",
           "localhost", 5432, "postgres", "password");

        public ItemsController(ILogger<ItemsController> logger)
        {
            _logger = logger;
            InitializeScript();

        }

        [HttpPost]
        [Route("AddItem")]
        public int AddItem([FromBody] Item item)
        {
     
            string insertRow = String.Format(@"INSERT INTO item (item_name,parent_item,cost," + 
                "req_date) VALUES  ('{0}',{1},{2},'{3}')", item.ItemName, item.ParentItem.HasValue ? item.ParentItem : "null", item.Cost, item.ReqDate);

            conn.Open();

            try
            {
                (new NpgsqlCommand(insertRow, conn)).ExecuteReader();
              

            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
            }
            conn.Close();
            int newId = getLastId();
            return newId;
        }

        [HttpGet]
        [Route("Item/{id}")]
        public async Task<Item?> getItem(int id)
        {
            string getTotalCostQuery = String.Format(@"SELECT * FROM item WHERE id = {0}",id); 

            Item? item = null;

            DataTable dt = new DataTable();
            await conn.OpenAsync();

            try
            {
                dt.Load(await (new NpgsqlCommand(getTotalCostQuery, conn)).ExecuteReaderAsync());

                DataRow row = dt.Rows[0];

                await Task.Run(() => 
                {
                    
                    item = new Item
                    {
                        Id = (int)dt.Rows[0]["id"],
                        ItemName = (string)row["item_name"],
                        ParentItem = (row["parent_item"] != DBNull.Value) ? (int)(row["parent_item"]) : null,
                        Cost = (int)row["cost"],
                        ReqDate = (DateTime)row["req_date"]

                    };

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await conn.CloseAsync();




            return item;
        }
        

        [HttpGet]
        [Route("TotalCost/{itemName}")]
        public async Task<int?> getTotalCost(string itemName)
        {
            string getTotalCostQuery = String.Format(@"Select Get_Total_Cost('{0}')", itemName);
            int? result = null;
            
            DataTable dt = new DataTable(); 

            await conn.OpenAsync();

            try
            {
             dt.Load(await (new NpgsqlCommand(getTotalCostQuery, conn)).ExecuteReaderAsync());
          
                result = (int)dt.Rows[0]["get_total_cost"];    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await conn.CloseAsync();


            
            
            return result;
        }

        private void InitializeScript()
        {
           
            string script = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() +
                "\\PostgreSqlScript\\Script.sql");

            string createDBquery = @"CREATE DATABASE part";


           NpgsqlConnection postgresConn = new NpgsqlConnection(connstring);

            

            postgresConn.Open();
            try
            {
                (new NpgsqlCommand(createDBquery, postgresConn)).ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Script Aleardy Executed!");
            }
            postgresConn.Close();

           string partDBconnstring = String.Format(connstring + "Database={0}", "part");

            conn = new NpgsqlConnection(partDBconnstring);

            conn.Open();
            try
            {
                (new NpgsqlCommand(script, conn)).ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Script Aleardy Executed!");
            }
            conn.Close();



        }

        private int getLastId()
        {
            string countRows = @"SELECT id FROM item";
            DataTable dt = new DataTable();

            conn.Open();

            try
            {
                dt.Load((new NpgsqlCommand(countRows, conn)).ExecuteReader());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            conn.Close();

            return dt.Rows.Count;
        }
    }
}

