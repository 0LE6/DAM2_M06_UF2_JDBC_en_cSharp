using System.Data;
using MySql.Data.MySqlClient;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            string connectionStr = "Server=localhost;Database=hospital;User=root;Password=";
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                con.Open();

                // PREPARED STATEMENT desde aquí:

                // 1st -> Preparando la consulta con ? como parámetros
                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE alumnos SET nota = 'NP' " +
                    "WHERE exp > @param1 AND exp < @param2", con))
                {
                    // 2nd -> Parametrización
                    cmd.Parameters.AddWithValue("@param1", 1);
                    cmd.Parameters.AddWithValue("@param2", 5);

                    // 3rd -> Ejecución
                    int n = cmd.ExecuteNonQuery();
                    if (n != 0) Console.WriteLine("BUENA ACTUALIZACIÓN");
                    else Console.WriteLine("MALA ACTUALIZACIÓN");
                }

                // ---- EJERCICIO 1 ----

                // 1st -> Preparando la consulta SELECT con ?
                string sSQL = "SELECT * FROM alumnos WHERE exp = @exp";

                // 2nd -> Creando el SqlCommand
                using (MySqlCommand cmd = new MySqlCommand(sSQL, con))
                {
                    // Vamos a seleccionar alumnos con exp de 1 a 3
                    for (int i = 1; i <= 3; i++)
                    {
                        cmd.Parameters.AddWithValue("@exp", i);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("Resultados para exp -> " + i + ":");
                            ShowResults(reader);
                        }
                    }
                }

                // ---- EJERCICIO 2 ----

                // Preparando la instrucción INSERT con parametrización
                sSQL = "INSERT INTO doctor (doctor_codi, doctor_hospital_codi," +
                    "doctor_nom, doctor_especialitat) VALUES (@param1, @param2, @param3, @param4)";
                int totalAffectedRows = 0; int affectedRow = 0;

                using (MySqlCommand insertCmd = new MySqlCommand(sSQL, con))
                {
                    // INSERT con addBatch() y executeBatch()
                    insertCmd.Parameters.AddWithValue("@param1", 5);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Dr. CSharp");
                    insertCmd.Parameters.AddWithValue("@param4", "Desde C#");
                    affectedRow = insertCmd.ExecuteNonQuery();
                    totalAffectedRows += affectedRow;

                    insertCmd.Parameters.AddWithValue("@param1", 6);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Dr. NET");
                    insertCmd.Parameters.AddWithValue("@param4", "Desde C#");
                    affectedRow = insertCmd.ExecuteNonQuery();
                    totalAffectedRows += affectedRow;

                    insertCmd.Parameters.AddWithValue("@param1", 7);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Dr. C#");
                    insertCmd.Parameters.AddWithValue("@param4", "Desde C#");
                    affectedRow = insertCmd.ExecuteNonQuery();
                    totalAffectedRows += affectedRow;
                }
                Console.WriteLine($"Número de INSERTs con Batch -> {totalAffectedRows}");

                // Preparando la instrucción UPDATE con parametrización al estilo C#, con @
                sSQL = "UPDATE doctor SET doctor_hospital_codi = @param1 " +
                    "WHERE doctor_codi = @param2";
                totalAffectedRows = 0; affectedRow = 0;
                using (MySqlCommand updateCmd = new MySqlCommand(sSQL, con))
                {
                    for (int i = 5; i <= 7; i++)
                    {
                        // Cambiando el Hospital de nuestros 3 nuevos doctores CSharperos
                        updateCmd.Parameters.AddWithValue("@param1", 22);
                        updateCmd.Parameters.AddWithValue("@param2", i);
                        affectedRow = updateCmd.ExecuteNonQuery();
                        totalAffectedRows += affectedRow;
                    }
                    Console.WriteLine($"Número de actualizaciones con Batch -> {totalAffectedRows}");
                }

                // ---- EJERCICIO 3 ----

                // CallableStatement con un solo valor al estilo C#
                // en otras palabras, en C#, no se hace distinción entre si es Callable, Prepared etc
                // MySqlCommand te permite trabajar todas juntas

                // 1st - Preparando la llamada a nuestro PROCEDURE almacenado
                string storedProcedure = "GetDoctor";
                int codigoDoctor = 6;
                using (MySqlCommand cmd = new MySqlCommand(storedProcedure, con))
                {
                    // Definimos que tipo de comando vamos a ejecutar
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@doctor_codi", codigoDoctor);

                    // Manejamos el parámetro de salida
                    MySqlParameter outParam = new MySqlParameter("@doctor_nom", SqlDbType.VarChar); // le decimos que es VARCHAR
                    outParam.Direction = ParameterDirection.Output; // y su dirección es de salida
                    cmd.Parameters.Add(outParam); // y lo añadimos la comando

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Imprimir el resultado del parámetro de salida
                    Console.WriteLine($"Nombre del doctor con ID {codigoDoctor}: " + outParam.Value); // lo printeamos
                }

                // ---- EJERCICIO 4 ----

                // Preparando la llamada al PROCEDURE almacenado con cursor
                storedProcedure = "GetDoctorsByHospital";
                using (MySqlCommand cmd = new MySqlCommand(storedProcedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@doctor_hospital_codi", 22);

                    // Ejecutar el procedimiento almacenado con variable booleana
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        ShowResultForCallableStatementWithCursor(reader);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("EXCEPCIÓN: CONEXIÓN NO ESTABLECIDA");
            Console.WriteLine(e.Message);
        }
    }

    // Método para el EJERCICIO 1
    static void ShowResults(MySqlDataReader results)
    {
        while (results.Read())
        {
            int exp = results.GetInt32(0); // Puedes usar el índice o el nombre de la columna
            string nombre = results.GetString(1);
            string nota = results.GetString(2);
            Console.WriteLine("Exp: " + exp + " | Nombre: " + nombre + " | Nota: " + nota);
        }
    }

    static void ShowResultForCallableStatementWithCursor(MySqlDataReader results)
    {
        while (results.Read())
        {
            int doctorCode = results.GetInt32(0);
            int hospitalCode = results.GetInt32(1);
            string doctorName = results.GetString(2);
            string doctorSpecialization = results.GetString(3);
            Console.WriteLine("Código: " + doctorCode +
                " | Nombre: " + doctorName +
                " | HospitalCode: " + hospitalCode +
                " | Especialización: " + doctorSpecialization);
        }
    }
}
