using EscolaMilitar.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using EscolaMilitar.Data; // Ensure correct namespace for your context

// All the pages must be listed here, even those with controllers.
namespace EscolaMilitar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EscolaMilitarContext _context;
        private readonly string _defaultConnectionString = "server=localhost;port=3306;database=militar;user=root;password=0000";

        public HomeController(ILogger<HomeController> logger, EscolaMilitarContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        private bool IsValidEmail(string email)
        {
            var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(Registos registo)
        {
            if (!IsValidEmail(registo.Email))
            {
                ModelState.AddModelError("Email", "Por favor, introduza um email válido.");
                return View("SignIn", registo); // Return immediately if email is invalid
            }

            if (!ModelState.IsValid)
            {
                return View("SignIn", registo); // Return immediately if the model is invalid
            }

            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "SELECT Id, Email, Senha FROM registos WHERE Email = @Email AND Senha = @Senha";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", registo.Email);
                    command.Parameters.AddWithValue("@Senha", registo.Senha);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve and store the user data in a session variable or cookies
                            int userId = reader.GetInt32(0); // Assuming Id is an integer
                            string userEmail = reader.GetString(1);
                            string userPassword = reader.GetString(2);

                            // Perform login logic here, e.g., redirect to a success page
                            return RedirectToAction("Info", new { tempId = userId });
                        }
                        else
                        {
                            ModelState.AddModelError("Denied", "Senha ou email inválidos.");
                        }
                    }
                }
            }
            return View("SignIn", registo);
        }

        private List<Militares> GetMilitaresData()
        {
            var militaresLista = new List<Militares>();
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM militares";
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var militar = new Militares
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Mec = Convert.ToInt32(reader["Mec"]),
                                    Nome = reader["Nome"].ToString(),
                                    Apelido = reader["Apelido"].ToString(),
                                    Idade = Convert.ToInt32(reader["Idade"])
                                };
                                militaresLista.Add(militar);
                            }
                        }
                    }
                }
            }
            return militaresLista.OrderBy(i => i.Nome).ToList();
        }

        public IActionResult Info(int? tempId)
        {
            var militaresLista = GetMilitaresData();
            ViewBag.SessionId = tempId; // Pass the sessionId (newuserId) to the view using ViewBag

            if (tempId.HasValue && tempId.Value == 1)
            {
                return View(militaresLista);
            }
            else
            {
                return RedirectToAction("Denied");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(long id, int? tempId)
        {
            // Check if tempId is provided and is valid
            if (!tempId.HasValue || tempId.Value != 1) 
            {
                return RedirectToAction("Denied"); 
            }

            var militar = _context.Militares.Find(id); // Assuming you're using Entity Framework
            if (militar == null)
            {
                return NotFound();
            }
            ViewBag.SessionId = tempId; 
            return View(militar);
        }

        public IActionResult Delete(long id, int? tempId)
        {
            if (!tempId.HasValue || tempId.Value != 1)
            {
                return RedirectToAction("Denied");
            }
            ViewBag.SessionId = tempId; 
            var militar = _context.Militares.Find(id); // Assuming you're using Entity Framework
            if (militar == null)
            {
                return NotFound();
            }
            return View(militar);
        }

        private bool IsAlphabetLetters(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(c => char.IsLetter(c));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Militares militar)
        {
            if (ModelState.IsValid)
            {
                if (militar.Mec <= 0)
                {
                    ModelState.AddModelError("Mec", "O número Mec deve ser maior que zero.");
                }

                if (!IsAlphabetLetters(militar.Nome))
                {
                    ModelState.AddModelError("Nome", "O Nome deve conter apenas letras do alfabeto.");
                }

                if (!IsAlphabetLetters(militar.Apelido))
                {
                    ModelState.AddModelError("Apelido", "O Apelido deve conter apenas letras do alfabeto.");
                }

                if (militar.Idade <= 17)
                {
                    ModelState.AddModelError("Idade", "A Idade deve ser maior que 17 anos.");
                }

                using (var connection = new MySqlConnection(_defaultConnectionString))
                {
                    connection.Open();
                    var sql = "INSERT INTO militares (Mec, Nome, Apelido, Idade) VALUES (@Mec, @Nome, @Apelido, @Idade); SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Mec", militar.Mec);
                        command.Parameters.AddWithValue("@Nome", militar.Nome);
                        command.Parameters.AddWithValue("@Apelido", militar.Apelido);
                        command.Parameters.AddWithValue("@Idade", militar.Idade);
                        militar.Id = Convert.ToInt32(command.ExecuteScalar());
                        /*var lastInsertedId = Convert.ToInt32(command.ExecuteScalar());
                        militar.Id = lastInsertedId;*/
                    }
                }
                return RedirectToAction("Lists");
            }
            return View(militar); // Será porque coloquei um IActionResult lá em cima?
        }

        // Edit Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Militares militar)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_defaultConnectionString))
                {
                    connection.Open();
                    var sql = "UPDATE militares SET Mec = @Mec, Nome = @Nome, Apelido = @Apelido, Idade = @Idade WHERE id = @id";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Mec", militar.Mec);
                        command.Parameters.AddWithValue("@Nome", militar.Nome);
                        command.Parameters.AddWithValue("@Apelido", militar.Apelido);
                        command.Parameters.AddWithValue("@Idade", militar.Idade);
                        command.Parameters.AddWithValue("@id", militar.Id);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Info", new { tempId = 1 }); // Redirect to a list or details page after successful update
            }
            return View(militar);
        }

        public IActionResult Details(long id, int? tempId)
        {
            if (!tempId.HasValue || tempId.Value != 1)
            {
                return RedirectToAction("Denied");
            }
            ViewBag.SessionId = tempId;

            var militar = new Militares();
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM militares WHERE id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                militar.Id = Convert.ToInt32(reader["id"]);
                                militar.Mec = Convert.ToInt32(reader["Mec"]);
                                militar.Nome = reader["Nome"].ToString();
                                militar.Apelido = reader["Apelido"].ToString();
                                militar.Idade = Convert.ToInt32(reader["Idade"]);
                            }
                        }
                    }
                }
            }
            return View(militar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Militares militar)
        {
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "DELETE FROM militares WHERE id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", militar.Id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Info", new { tempId = 1 });
        }

        private List<Registos> GetRegistosData()
        {
            var militaresRegisto = new List<Registos>();
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM registos";
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var registo = new Registos
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Email = reader["Email"].ToString(),
                                    Senha = reader["Senha"].ToString(),
                                };
                                militaresRegisto.Add(registo);
                            }
                        }
                    }
                }
            }
            return militaresRegisto.OrderBy(i => i.Email).ToList();
        }

        public IActionResult Info2(int? tempId)
        {
            if (!tempId.HasValue || tempId.Value != 1)
            {
                return RedirectToAction("Denied");
            }
            ViewBag.SessionId = tempId; // Pass the sessionId (newuserId) to the view using ViewBag
            var militaresRegisto = GetRegistosData();
            return View(militaresRegisto);
        }

        public IActionResult Create2()
        {
            return View();
        }

        public IActionResult Delete2(long id, int? tempId)
        {
            if (!tempId.HasValue || tempId.Value != 1)
            {
                return RedirectToAction("Denied");
            }
            ViewBag.SessionId = tempId; 
            var registo = _context.Registos.Find(id); // Assuming you're using Entity Framework
            if (registo == null)
            {
                return NotFound();
            }
            return View(registo);
        }

        /*       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(Registos model)
        {
            if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Por favor, introduza um email válido.");
                return View("SignIn", model); // Return immediately if email is invalid
            }

            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_defaultConnectionString))
                {
                    connection.Open();
                    var sql = "INSERT INTO registos (Email, Senha) VALUES (@Email, @Senha)";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@Senha", model.Senha);
                        command.ExecuteNonQuery();
                    }
                }
            }
            return RedirectToAction("Info2");
        }
        */

        public IActionResult Details2(long id, int? tempId)
        {
            if (!tempId.HasValue || tempId.Value != 1)
            {
                return RedirectToAction("Denied");
            }
            ViewBag.SessionId = tempId;

            var registo = new Registos();
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM registos WHERE id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                registo.Id = Convert.ToInt32(reader["id"]);
                                registo.Email = reader["Email"].ToString();
                                registo.Senha = reader["Senha"].ToString();
                            }
                        }
                    }
                }
            }
            return View(registo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(Registos registo)
        {
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();
                var sql = "DELETE FROM registos WHERE id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", registo.Id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Info2", new { tempId = 1 });
        }

        [HttpGet]
        public IActionResult DownloadData()
        {
            using (var connection = new MySqlConnection(_defaultConnectionString))
            {
                connection.Open();

                var sql = "SELECT * FROM militares";
                var command = new MySqlCommand(sql, connection);

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            // Create a memory stream to store the downloaded data
                            MemoryStream outputStream = new MemoryStream();

                            // Write the data to the memory stream
                            while (reader.Read())
                            {
                                Militares militar = new Militares();
                                militar.Id = Convert.ToInt32(reader["id"]);
                                militar.Mec = Convert.ToInt32(reader["Mec"]);
                                militar.Nome = reader["Nome"].ToString();
                                militar.Apelido = reader["Apelido"].ToString();
                                militar.Idade = Convert.ToInt32(reader["Idade"]);

                                // Append the data in a desired format (e.g., CSV, JSON, etc.)
                                outputStream.Write(System.Text.Encoding.UTF8.GetBytes($"{militar.Id},{militar.Mec},{militar.Nome},{militar.Apelido},{militar.Idade}\r\n"));
                            }

                            // Convert the memory stream to an array of bytes
                            byte[] fileData = outputStream.ToArray();

                            // Return the file as a download
                            return File(fileData, "application/octet-stream", "militares.csv");
                        }
                        else
                        {
                            // No rows found, redirect to Denied action
                            return RedirectToAction("Denied");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that occurred during the data retrieval
                    // Log the exception (ex) if necessary
                    return RedirectToAction("Denied");
                }
            }
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult News()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Lists()
        {
            var militaresLista = GetMilitaresData();
            return View(militaresLista);
        }

        [AllowAnonymous]
        public IActionResult Contacts()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Denied()
        {
            return View();
        }
    }
}