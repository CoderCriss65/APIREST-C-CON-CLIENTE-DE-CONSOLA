namespace BACKEND.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }  // Permite null
        public string? Puesto { get; set; }  // Permite null
        public decimal Salario { get; set; } // No permite null, usa 0 como default
    }
}
