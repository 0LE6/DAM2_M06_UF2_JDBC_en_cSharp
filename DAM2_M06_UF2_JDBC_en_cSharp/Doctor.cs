using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2_M06_UF2_JDBC_en_cSharp
{
    public class Doctor
    {
        // Propiedades que encapsulan los atributos del Doctor
        public int DoctorCode { get; set; }
        public string Name { get; set; }
        public int HospitalCode { get; set; }
        public string Specialization { get; set; }

        // Constructor sin parámetros
        public Doctor() { }

        // Constructor con parámetros
        public Doctor(int doctorCode, string name, int hospitalCode, string specialization)
        {
            DoctorCode = doctorCode;
            Name = name;
            HospitalCode = hospitalCode;
            Specialization = specialization;
        }
    }
}
