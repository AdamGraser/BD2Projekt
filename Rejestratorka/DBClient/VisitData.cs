using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{   
    /// <summary>
    /// Klasa przechowująca dane wizyty.
    /// </summary>
    public class VisitData
    {
        private string _patientName;
        private string _patientSurname;
        private string _patientPesel;
        private string _patientDateOfBirth;
        private DateTime _date;
        private string _doctor;
        private string _visitStatus;



        /// <summary>
        /// Właściwość zwracająca imię pacjenta.
        /// </summary>
        public string PatientName
        {
            get
            {
                return _patientName;
            }
            set
            {
                _patientName = value;
            }
        }



        /// <summary>
        /// Właściwość zwracająca nazwisko pacjenta.
        /// </summary>
        public string PatientSurname
        {
            get
            {
                return _patientSurname;
            }
            set
            {
                _patientSurname = value;
            }
        }



        /// <summary>
        /// Właściwość zwracająca PESEL pacjenta.
        /// </summary>
        public string PatientPesel
        {
            get
            {
                return _patientPesel;
            }
            set
            {
                _patientPesel = value;
            }
        }



        /// <summary>
        /// Właściwość zwracająca datę urodzenia pacjenta.
        /// </summary>
        public string PatientDateOfBirth
        {
            get
            {
                return _patientDateOfBirth;
            }
            set
            {
                _patientDateOfBirth = value;
            }
        }



        /// <summary>
        /// Właściwość zwracająca datę wizyty.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
            }
        }



        /// <summary>
        /// Właściwość zwracająca imię i nazwisko lekarza, u którego ma odbyć się wizyta.
        /// </summary>
        public string Doctor
        {
            get
            {
                return _doctor;
            }
            set
            {
                _doctor = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Status
        {
            get
            {
                return _visitStatus;
            }
            set
            {
                _visitStatus = value;
            }
        }
    }
}
