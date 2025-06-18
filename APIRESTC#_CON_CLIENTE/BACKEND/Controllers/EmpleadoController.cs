// Controllers/EmpleadoController.cs
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using BACKEND.Models;
using System.Data;
using MySql.Data;

namespace BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadoController : ControllerBase
    {
        private readonly ILogger<EmpleadoController> _logger;
        private readonly string _connectionString;

        public EmpleadoController(ILogger<EmpleadoController> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("MySQLConnection");
        }

        // GET: api/empleado
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("GET /api/empleado recibido");

            var empleados = new List<Empleado>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM empleados", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Manejar valores NULL
                        string nombre = !reader.IsDBNull("nombre")
                            ? reader.GetString("nombre")
                            : null;

                        string puesto = !reader.IsDBNull("puesto")
                            ? reader.GetString("puesto")
                            : null;

                        // Para decimal, usa un valor por defecto (0) si es NULL
                        decimal salario = !reader.IsDBNull("salario")
                            ? reader.GetDecimal("salario")
                            : 0;

                        empleados.Add(new Empleado
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = nombre,
                            Puesto = puesto,
                            Salario = salario
                        });
                    }
                }
            }
            return Ok(empleados);
        }

        // GET api/empleado/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation($"GET /api/empleado/{id} recibido");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM empleados WHERE id = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var empleado = new Empleado
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),
                                Puesto = reader.GetString("puesto"),
                                Salario = reader.GetDecimal("salario")
                            };
                            return Ok(empleado);
                        }
                        return NotFound();
                    }
                }
            }
        }

        // POST api/empleado
        [HttpPost]
        public IActionResult Post([FromBody] Empleado empleado)
        {
            _logger.LogInformation($"POST /api/empleado recibido: {empleado.Nombre}");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(
                    "INSERT INTO empleados (nombre, puesto, salario) VALUES (@nombre, @puesto, @salario); SELECT LAST_INSERT_ID();",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", empleado.Nombre);
                    cmd.Parameters.AddWithValue("@puesto", empleado.Puesto);
                    cmd.Parameters.AddWithValue("@salario", empleado.Salario);

                    var newId = cmd.ExecuteScalar();

                    // Crear nuevo objeto con el ID generado
                    var nuevoEmpleado = new Empleado
                    {
                        Id = Convert.ToInt32(newId),
                        Nombre = empleado.Nombre,
                        Puesto = empleado.Puesto,
                        Salario = empleado.Salario
                    };

                    return CreatedAtAction(nameof(Get), new { id = newId }, nuevoEmpleado);
                }
            }
        }






        // PUT api/empleado/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Empleado empleado)
        {
            _logger.LogInformation($"PUT /api/empleado/{id} recibido: {empleado.Nombre}");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(
                    "UPDATE empleados SET nombre = @nombre, puesto = @puesto, salario = @salario WHERE id = @id",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre", empleado.Nombre);
                    cmd.Parameters.AddWithValue("@puesto", empleado.Puesto);
                    cmd.Parameters.AddWithValue("@salario", empleado.Salario);

                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0) return NotFound();
                    return NoContent();
                }
            }
        }

        // DELETE api/empleado/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"DELETE /api/empleado/{id} recibido");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand("DELETE FROM empleados WHERE id = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0) return NotFound();
                    return NoContent();
                }
            }
        }
    }
}
