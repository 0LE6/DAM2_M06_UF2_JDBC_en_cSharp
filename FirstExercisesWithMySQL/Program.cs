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
                using (MySqlCommand insertCmd = new MySqlCommand(sSQL, con))
                {
                    // INSERT con addBatch() y executeBatch()
                    insertCmd.Parameters.AddWithValue("@param1", 1);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Frankenstein");
                    insertCmd.Parameters.AddWithValue("@param4", "Monstruos");
                    insertCmd.ExecuteNonQuery();

                    insertCmd.Parameters.AddWithValue("@param1", 2);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Dolittle");
                    insertCmd.Parameters.AddWithValue("@param4", "Zoologia");
                    insertCmd.ExecuteNonQuery();

                    insertCmd.Parameters.AddWithValue("@param1", 3);
                    insertCmd.Parameters.AddWithValue("@param2", 18);
                    insertCmd.Parameters.AddWithValue("@param3", "Patch Adams");
                    insertCmd.Parameters.AddWithValue("@param4", "Risoterapia");
                    insertCmd.ExecuteNonQuery();

                    Console.WriteLine("Número de INSERTs con Batch -> 3");

                }

                // Preparando la instrucción UPDATE con parametrización
                sSQL = "UPDATE doctor SET doctor_hospital_codi = @param1 " +
                    "WHERE doctor_codi = @param2";
                using (MySqlCommand updateCmd = new MySqlCommand(sSQL, con))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        // Cambiando el Hospital de nuestros 3 nuevos doctores
                        updateCmd.Parameters.AddWithValue("@param1", 22);
                        updateCmd.Parameters.AddWithValue("@param2", i);
                        updateCmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Número de actualizaciones con Batch -> 3");
                }

                // ---- EJERCICIO 3 ----

                // CallableStatement con un solo valor

                // 1st - Preparando la llamada a nuestro PROCEDURE almacenado
                string storedProcedureCall = "GetDoctor";
                using (MySqlCommand cmd = new MySqlCommand(storedProcedureCall, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@doctor_codi", 1);

                    MySqlParameter outParam = new MySqlParameter("@doctor_nom", SqlDbType.VarChar);
                    outParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outParam);

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Imprimir el resultado del parámetro de salida
                    Console.WriteLine("Nombre del doctor con ID 1: " + outParam.Value);
                }

                // ---- EJERCICIO 4 ----

                // Preparando la llamada al PROCEDURE almacenado con cursor
                storedProcedureCall = "GetDoctorsByHospital";
                using (MySqlCommand cmd = new MySqlCommand(storedProcedureCall, con))
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
