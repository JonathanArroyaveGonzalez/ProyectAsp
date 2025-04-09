using DB_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

namespace DB_MVC.Controllers.Tienda
{
    public class ProductosController : Controller
    {
        private readonly IConfiguration _configuration;
        public ProductosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Insertar");
        }

        public IActionResult Insertar()
        {
            return View("Views/Tienda/Insertar.cshtml");
        }

        [HttpGet]
        public JsonResult ObtenerTodos()
        {
            List<BicicletaModel> bicicletas = new List<BicicletaModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Placa, Marca FROM Bicicletas";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BicicletaModel bicicleta = new BicicletaModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Placa = reader["Placa"].ToString(),
                                    Marca = reader["Marca"].ToString()
                                };
                                bicicletas.Add(bicicleta);
                            }
                        }
                        return Json(new { success = true, data = bicicletas });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, mensaje = "Error al obtener bicicletas: " + ex.Message });
                    }
                }
            }
        }

        [HttpGet]
        public JsonResult ObtenerPorId(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Placa, Marca FROM Bicicletas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                BicicletaModel bicicleta = new BicicletaModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Placa = reader["Placa"].ToString(),
                                    Marca = reader["Marca"].ToString()
                                };
                                return Json(new { success = true, data = bicicleta });
                            }
                            else
                            {
                                return Json(new { success = false, mensaje = "Bicicleta no encontrada" });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, mensaje = "Error al obtener la bicicleta: " + ex.Message });
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Guardar([FromBody] BicicletaModel bicicleta)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Error de validación de datos del modelo",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            if (bicicleta.Id > 0)
            {
                return Actualizar(bicicleta);
            }
            else
            {
                return Insertar(bicicleta);
            }
        }

        [HttpPost]
        public IActionResult Insertar([FromBody] BicicletaModel bicicleta)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Error de validación de datos del modelo",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Bicicletas (Placa, Marca) VALUES (@Placa, @Marca); SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Placa", bicicleta.Placa);
                    command.Parameters.AddWithValue("@Marca", bicicleta.Marca);
                    try
                    {
                        connection.Open();
                        decimal id = (decimal)command.ExecuteScalar();
                        bicicleta.Id = Convert.ToInt32(id);

                        return Json(new
                        {
                            success = true,
                            mensaje = "Bicicleta registrada correctamente",
                            data = bicicleta
                        });
                    }
                    catch (SqlException ex)
                    {
                        return Json(new { success = false, mensaje = "Error en la base de datos: " + ex.Message });
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Actualizar([FromBody] BicicletaModel bicicleta)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Error de validación de datos del modelo",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Bicicletas SET Placa = @Placa, Marca = @Marca WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", bicicleta.Id);
                    command.Parameters.AddWithValue("@Placa", bicicleta.Placa);
                    command.Parameters.AddWithValue("@Marca", bicicleta.Marca);
                    try
                    {
                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                            return Json(new
                            {
                                success = true,
                                mensaje = "Bicicleta actualizada correctamente",
                                data = bicicleta
                            });
                        else
                            return Json(new { success = false, mensaje = "No se encontró la bicicleta para actualizar" });
                    }
                    catch (SqlException ex)
                    {
                        return Json(new { success = false, mensaje = "Error en la base de datos: " + ex.Message });
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Eliminar([FromBody] int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Bicicletas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    try
                    {
                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                            return Json(new { success = true, mensaje = "Bicicleta eliminada correctamente" });
                        else
                            return Json(new { success = false, mensaje = "No se encontró la bicicleta para eliminar" });
                    }
                    catch (SqlException ex)
                    {
                        return Json(new { success = false, mensaje = "Error en la base de datos: " + ex.Message });
                    }
                }
            }
        }
    }
}