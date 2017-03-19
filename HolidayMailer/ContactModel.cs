using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HolidayMailer
{
    class ContactModel : ObservableObject
    {
        private static bool _lnSort = true;
        private int _id;
        private string _lname;
        private string _fname;
        private string _email;
        private bool _didSend;

        public ContactModel(int id, string lname, string fname, string email, bool didSend)
        {
            _id = id;
            _lname = lname;
            _fname = fname;
            _email = email;
            _didSend = didSend;
            //LNSort = false;
        }
        
        public bool LNSort
        {
            get { return _lnSort; }
            set { _lnSort = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string LName
        {
            get { return _lname; }
            set { _lname = value; }
        }

        public string FName
        {
            get { return _fname; }
            set { _fname = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public bool DidSend
        {
            get { return _didSend; }
            set { _didSend = value; }
        }

        public override string ToString()
        {
            if (_lnSort)
                return _lname + ", " + _fname;
            else
                return _fname + " " + _lname;
        }

    }
}
