using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization; // Necesario para JsonPropertyName

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private const string ApiUrl = "http://192.168.20.26:5000/api/empleado/";
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Ignorar mayúsculas/minúsculas
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Esperar nombres en camelCase
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("Cliente de Gestión de Empleados");

        while (true)
        {
            Console.WriteLine("\nMENÚ PRINCIPAL:");
            Console.WriteLine("1. Listar todos los empleados");
            Console.WriteLine("2. Ver detalles de un empleado");
            Console.WriteLine("3. Crear nuevo empleado");
            Console.WriteLine("4. Actualizar empleado existente");
            Console.WriteLine("5. Eliminar empleado");
            Console.WriteLine("6. Salir");
            Console.Write("Seleccione una opción: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await ListarEmpleados();
                    break;
                case "2":
                    await ObtenerEmpleado();
                    break;
                case "3":
                    await CrearEmpleado();
                    break;
                case "4":
                    await ActualizarEmpleado();
                    break;
                case "5":
                    await EliminarEmpleado();
                    break;
                case "6":
                    Console.WriteLine("Saliendo del sistema...");
                    return;
                default:
                    Console.WriteLine("Opción inválida, por favor intente de nuevo.");
                    break;
            }
        }
    }

    private static async Task ListarEmpleados()
    {
        Console.WriteLine("\nObteniendo lista de empleados...");

        try
        {
            var response = await client.GetAsync(ApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Respuesta cruda del servidor: " + content); // Para depuración

                var empleados = JsonSerializer.Deserialize<Empleado[]>(content, jsonOptions);

                if (empleados == null || empleados.Length == 0)
                {
                    Console.WriteLine("No se encontraron empleados");
                    return;
                }

                Console.WriteLine("\nLISTA DE EMPLEADOS:");
                Console.WriteLine("====================================================");
                foreach (var emp in empleados)
                {
                    Console.WriteLine($"ID: {emp.Id}");
                    Console.WriteLine($"Nombre: {emp.Nombre}");
                    Console.WriteLine($"Puesto: {emp.Puesto}");
                    Console.WriteLine($"Salario: {emp.Salario:C}");
                    Console.WriteLine("----------------------------------------------------");
                }
                Console.WriteLine($"Total: {empleados.Length} empleados");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Detalles del error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task ObtenerEmpleado()
    {
        Console.Write("\nIngrese el ID del empleado: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido");
            return;
        }

        Console.WriteLine($"Buscando empleado ID: {id}...");

        try
        {
            var response = await client.GetAsync($"{ApiUrl}{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var empleado = JsonSerializer.Deserialize<Empleado>(content, jsonOptions);

                Console.WriteLine("\nDETALLES DEL EMPLEADO:");
                Console.WriteLine("====================================================");
                Console.WriteLine($"ID: {empleado.Id}");
                Console.WriteLine($"Nombre: {empleado.Nombre}");
                Console.WriteLine($"Puesto: {empleado.Puesto}");
                Console.WriteLine($"Salario: {empleado.Salario:C}");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Empleado no encontrado");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Detalles del error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task CrearEmpleado()
    {
        Console.WriteLine("\nCREAR NUEVO EMPLEADO");

        var empleado = new Empleado();

        Console.Write("Nombre: ");
        empleado.Nombre = Console.ReadLine();

        Console.Write("Puesto: ");
        empleado.Puesto = Console.ReadLine();

        Console.Write("Salario: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal salario))
        {
            Console.WriteLine("Salario inválido");
            return;
        }
        empleado.Salario = salario;

        Console.WriteLine("Creando empleado...");

        try
        {
            var json = JsonSerializer.Serialize(empleado, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdEmpleado = JsonSerializer.Deserialize<Empleado>(responseContent, jsonOptions);
                Console.WriteLine($"\nEmpleado creado con ID: {createdEmpleado.Id}");
                Console.WriteLine(JsonSerializer.Serialize(createdEmpleado, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Detalles: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task ActualizarEmpleado()
    {
        Console.WriteLine("\nACTUALIZAR EMPLEADO");

        Console.Write("ID del empleado a actualizar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido");
            return;
        }

        Console.Write("Nuevo Nombre: ");
        var nombre = Console.ReadLine();

        Console.Write("Nuevo Puesto: ");
        var puesto = Console.ReadLine();

        Console.Write("Nuevo Salario: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal salario))
        {
            Console.WriteLine("Salario inválido");
            return;
        }

        var empleado = new Empleado
        {
            Id = id,
            Nombre = nombre,
            Puesto = puesto,
            Salario = salario
        };

        Console.WriteLine($"Actualizando empleado ID: {id}...");

        try
        {
            var json = JsonSerializer.Serialize(empleado, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{ApiUrl}{id}", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Empleado ID: {id} actualizado correctamente");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Empleado no encontrado");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Detalles: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task EliminarEmpleado()
    {
        Console.WriteLine("\nELIMINAR EMPLEADO");

        Console.Write("ID del empleado a eliminar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido");
            return;
        }

        Console.WriteLine($"Eliminando empleado ID: {id}...");

        try
        {
            var response = await client.DeleteAsync($"{ApiUrl}{id}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Empleado ID: {id} eliminado correctamente");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Empleado no encontrado");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public class Empleado
    {
        [JsonPropertyName("id")] // Coincidir con la propiedad en JSON
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("puesto")]
        public string? Puesto { get; set; }

        [JsonPropertyName("salario")]
        public decimal Salario { get; set; }
    }
}