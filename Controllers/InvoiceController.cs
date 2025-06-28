using InvoiceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Data;

namespace InvoiceManagementSystem.Controllers
{
    public class InvoiceController : Controller
    {
        
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public InvoiceController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("InvoiceManagementSystem")!;
        }

        public IActionResult Index()
        {            
            return View();
        }

        public IActionResult Invoice()
        {
            List<Invoice> invoices = new();

            using (SqlConnection con = new(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new("GetAllInvoices", con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    invoices.Add(new Invoice
                    {
                        Id = (int)reader["Id"],
                        InvoiceNumber = reader["InvoiceNumber"].ToString()!,
                        InvoiceDate = (DateTime)reader["InvoiceDate"],
                        DueDate = (DateTime)reader["DueDate"],
                        CustomerName = reader["CustomerName"].ToString()!,
                        Description = reader["Description"].ToString(),
                        VendorName = reader["VendorName"].ToString()!,
                        AmountPaid = (decimal)reader["AmountPaid"]
                    });
                }
            }
            return View(invoices);
        }

        public IActionResult Create()
        {
            List<Item> items = new();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("GetAllItems", con);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new Item
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                        QuantityAvailable = Convert.ToInt32(reader["QuantityAvailable"])
                    });
                }
            }

            ViewBag.Items = items;

            return View(new Invoice
            {
                InvoiceDate = DateTime.Today,
                Items = new List<InvoiceItem>()
            });
        }


        [HttpPost]
        public IActionResult Create(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return View(invoice);
            }

            invoice.InvoiceNumber = string.IsNullOrEmpty(invoice.InvoiceNumber)
                ? GenerateInvoiceNumber()
                : invoice.InvoiceNumber;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    Dictionary<int, int> stockMap = new();
                    SqlCommand stockCmd = new SqlCommand("SELECT Id, QuantityAvailable FROM Item", con, transaction);
                    using (SqlDataReader stockReader = stockCmd.ExecuteReader())
                    {
                        while (stockReader.Read())
                        {
                            int itemId = (int)stockReader["Id"];
                            int qty = (int)stockReader["QuantityAvailable"];
                            stockMap[itemId] = qty;
                        }
                    }

                    SqlCommand cmd = new SqlCommand("InsertInvoice", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                    cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                    cmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName);
                    cmd.Parameters.AddWithValue("@VendorName", invoice.VendorName);
                    cmd.Parameters.AddWithValue("@AmountPaid", invoice.AmountPaid);
                    cmd.Parameters.Add("@NewInvoiceId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    int invoiceId = (int)cmd.Parameters["@NewInvoiceId"].Value;

                    foreach (var item in invoice.Items)
                    {
                        if (!stockMap.ContainsKey(item.ItemId) || item.Quantity > stockMap[item.ItemId])
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", $"Quantity for item ID {item.ItemId} exceeds available stock.");
                            ViewBag.Items = GetItems(); 
                            return View(invoice);
                        }

                        SqlCommand itemCmd = new SqlCommand("InsertInvoiceItem", con, transaction);
                        itemCmd.CommandType = CommandType.StoredProcedure;
                        itemCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                        itemCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                        itemCmd.Parameters.AddWithValue("@Description", item.Description);
                        itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return RedirectToAction("invoice");
                }
                catch
                {
                    transaction.Rollback();
                    return View(invoice);
                }
            }
        }


        private string GenerateInvoiceNumber() => Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        private List<Item> GetItems()
        {
            List<Item> items = new();
            using (SqlConnection con = new(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new("SELECT * FROM Item", con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new Item
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString()!,
                        Description = reader["Description"].ToString()!,
                        UnitPrice = (decimal)reader["UnitPrice"],
                        QuantityAvailable = (int)reader["QuantityAvailable"]
                    });
                }
            }
            return items;
        }     

        public IActionResult Edit(int id)
        {
            Invoice invoice = new();
            invoice.Items = new List<InvoiceItem>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();                
                SqlCommand cmd = new SqlCommand("GetInvoiceById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    invoice.Id = id;
                    invoice.InvoiceNumber = reader["InvoiceNumber"].ToString();
                    invoice.InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]);
                    invoice.DueDate = Convert.ToDateTime(reader["DueDate"]);
                    invoice.CustomerName = reader["CustomerName"].ToString();
                    invoice.VendorName = reader["VendorName"].ToString();
                    invoice.AmountPaid = Convert.ToDecimal(reader["AmountPaid"]);
                }
                reader.Close();
             
                SqlCommand itemCmd = new SqlCommand("GetInvoiceItemsByInvoiceId", con);
                itemCmd.CommandType = CommandType.StoredProcedure;
                itemCmd.Parameters.AddWithValue("@InvoiceId", id);
                SqlDataReader itemReader = itemCmd.ExecuteReader();
                while (itemReader.Read())
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        ItemId = Convert.ToInt32(itemReader["ItemId"]),
                        Description = itemReader["Description"].ToString(),
                        UnitPrice = Convert.ToDecimal(itemReader["UnitPrice"]),
                        Quantity = Convert.ToInt32(itemReader["Quantity"])
                    });
                }
                itemReader.Close();
              
                List<Item> items = new();
                SqlCommand itemListCmd = new SqlCommand("GetAllItem", con);
                itemListCmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader listReader = itemListCmd.ExecuteReader();
                while (listReader.Read())
                {
                    items.Add(new Item
                    {
                        Id = Convert.ToInt32(listReader["Id"]),
                        Name = listReader["Name"].ToString(),
                        Description = listReader["Description"].ToString(),
                        UnitPrice = Convert.ToDecimal(listReader["UnitPrice"]),
                        QuantityAvailable = Convert.ToInt32(listReader["QuantityAvailable"])
                    });
                }
                listReader.Close();

                ViewBag.Items = items;
            }

            return View("Create", invoice);
        }


        public IActionResult GetInvoiceJson(int id)
        {
            var invoice = GetInvoiceById(id);
            return Json(new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.InvoiceDate,
                invoice.DueDate,
                invoice.CustomerName,
                invoice.VendorName,
                invoice.AmountPaid,
                items = invoice.Items.Select(i => new
                {
                    i.Description,
                    i.UnitPrice,
                    i.Quantity
                })
            });
        }

        public Invoice GetInvoiceById(int id)
        {
            Invoice invoice = new();
            invoice.Items = new List<InvoiceItem>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
             
                using (SqlCommand cmd = new SqlCommand("GetInvoiceById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            invoice.Id = id;
                            invoice.InvoiceNumber = reader["InvoiceNumber"].ToString();
                            invoice.InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]);
                            invoice.DueDate = Convert.ToDateTime(reader["DueDate"]);
                            invoice.CustomerName = reader["CustomerName"].ToString();
                            invoice.VendorName = reader["VendorName"].ToString();
                            invoice.AmountPaid = Convert.ToDecimal(reader["AmountPaid"]);
                        }
                    }
                }
                
                using (SqlCommand cmd = new SqlCommand("GetInvoiceItemsByInvoiceId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceId", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoice.Items.Add(new InvoiceItem
                            {
                                Description = reader["Description"].ToString(),
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }
                }
            }

            return invoice;
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
