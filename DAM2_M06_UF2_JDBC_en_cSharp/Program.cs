using DAM2_M06_UF2_JDBC_en_cSharp;
using System.Data;
using MySql.Data.MySqlClient;

internal class Program
{
    private static void Main(string[] args)
    {
        // Aquí creamos nuestro nuevo Doctor.
        Doctor drCSharp = new Doctor(999, "CSharp", 18, "SQL from C#");
        int newHospitalCode = 22;

        if (CreateAndUpdate(drCSharp, newHospitalCode))
            Console.WriteLine("SUCCESFULLY CREATED & UPDATED");
        else Console.WriteLine("FAIL TO CREATED & UPDATED");

        Console.WriteLine("\n"); // Separamos

        Doctor drFail = new Doctor();
        drFail.DoctorCode = 70;
        drFail.Name = "Fail";
        drFail.HospitalCode = 18;
        drFail.Specialization = "Get Exceptions";

        // Con un código de hospital inexistente
        if (CreateAndUpdate(drFail, 666))
            Console.WriteLine("SUCCESFULLY CREATED & UPDATED");
        else Console.WriteLine("FAIL TO CREATED & UPDATED");
    }

    public static bool CreateAndUpdate(Doctor d, int updateHospitalCode)
    {
        string user = "root"; string pass = "";
        bool everythingIsOk = true;
        string connectionString = $"Server=localhost;Database=hospital;User={user};Password={pass};";

        // Lo primero de todo, se crea una conexion mediante la MySqlConnection
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            // Esto es el quivalente de Java --> Connection con = DriverManager.getConnection...
            // Abre la conexion
            connection.Open();

            // Creamos una transaction
            MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1st - Llamamos al procedimiento almacenado para crear un nuevo doctor
                // En este using entramos la comanda, pasandole en nombre del Procedure, la conexion y la transaccion
                using (MySqlCommand command = new MySqlCommand("CreateDoctor", connection, transaction))
                {
                    // Importante, especificamos que a lo que vamos a entrar es a una Procedure
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@doctorCode", d.DoctorCode);
                    command.Parameters.AddWithValue("@doctorName", d.Name);
                    command.Parameters.AddWithValue("@hospitalCode", d.HospitalCode);
                    command.Parameters.AddWithValue("@docSpec", d.Specialization);

                    // Ejecutamos el comando
                    command.ExecuteNonQuery();
                }

                // 2nd - Llamamos al procedimiento almacenado para actualizar el doctor
                using (MySqlCommand command = new MySqlCommand("UpdateDoctorHospital", connection, transaction))
                {
                    // Importante, especificamos que a lo que vamos a entrar es a una Procedure
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@DoctorCode", d.DoctorCode);
                    command.Parameters.AddWithValue("@newHospitalCode", updateHospitalCode);

                    // Ejecutamos el comando
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                Console.WriteLine("Transaction committed successfully!");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine("Transaction rolled back due to an Exception");
                Console.WriteLine(e.Message);
                everythingIsOk = false;
            }
        }

        return everythingIsOk;
    }
}