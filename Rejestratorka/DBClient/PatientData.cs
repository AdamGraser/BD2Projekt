using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class PatientData
    {
        private string _patientName;
        private string _patientSurname;
        private string _patientPesel;
        private string _patientDateOfBirth;
        private string _patientGender;
        private string _patientStreet;
        private string _patientNumberOfHouse;
        private string _patientNumberOfFlat;
        private string _patientPostCode;
        private string _patientCity;



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
        /// Właściwość zwracająca płeć pacjenta.
        /// </summary>
        public string PatientGender
        {
            get
            {
                return _patientGender;
            }
            set
            {
                _patientGender = value;
            }
        }


        /// <summary>
        /// Właściwość zwracająca nazwę ulicy na której mieszka pacjent.
        /// </summary>
        public string PatientStreet
        {
            get
            {
                return _patientStreet;
            }
            set
            {
                _patientStreet = value;
            }
        }


        /// <summary>
        /// Właściwość zwracająca numer budynku w którym mieszka pacjent.
        /// </summary>
        public string PatientNumberOfHouse
        {
            get
            {
                return _patientNumberOfHouse;
            }
            set
            {
                _patientNumberOfHouse = value;
            }
        }


        /// <summary>
        /// Właściwość zwracająca numer mieszkania pacjenta.
        /// </summary>
        public string PatientNumberOfFlat
        {
            get
            {
                return _patientNumberOfFlat;
            }
            set
            {
                _patientNumberOfFlat = value;
            }
        }


        /// <summary>
        /// Właściwość zwracająca kod pocztowy adresu pacjenta.
        /// </summary>
        public string PatientPostCode
        {
            get
            {
                return _patientPostCode;
            }
            set
            {
                _patientPostCode = value;
            }
        }


        /// <summary>
        /// Właściwość zwracająca nazwę miejscowości w której mieszka pacjent.
        /// </summary>
        public string PatientCity
        {
            get
            {
                return _patientCity;
            }
            set
            {
                _patientCity = value;
            }
        }
    }
}
