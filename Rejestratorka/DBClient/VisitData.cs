using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{    
    public class VisitData
    {
        private string _patientName;
        private string _patientSurname;
        private string _patientPesel;
        private string _patientDateOfBirth;
        private string _date;
        private string _doctor;

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

        public string Date
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
    }
}
