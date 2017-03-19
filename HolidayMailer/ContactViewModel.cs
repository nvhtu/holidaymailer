using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

namespace HolidayMailer
{
    class ContactViewModel : ObservableObject
    {
        private ContactDatabase _contactDb;
        private ObservableCollection<ContactModel> _contactList = new ObservableCollection<ContactModel>();
        private ContactModel _selectedContact;
        private ICollectionView _contactListView;
        private string _sortSelection;
        private SQLiteConnection _dbConn;
        private bool _lnSort;
        private string _letterFilter;

        public ContactViewModel()
        {
            _contactDb = new ContactDatabase();
            _lnSort = true;
            //_contactList.Add(new ContactModel("Tu", "Nguyen", "abc@xyz.com", true));
            // _contactList.Add(new ContactModel("Chi", "Tran", "abc@xyz.com", true));
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            ContactList = GetContactList();
            ContactListView = CollectionViewSource.GetDefaultView(_contactList);
            LetterFilter = "All letters";
            SelectedContact = ContactList.First();
        }

        public ObservableCollection<ContactModel> GetContactList()
        {
            ObservableCollection<ContactModel> contactList = new ObservableCollection<ContactModel>();
            _dbConn.Open();

            string tempSort = "";
            if (_lnSort)
                tempSort = "lname";
            else
                tempSort = "fname";

            string query = "SELECT * FROM contact ORDER BY " + tempSort;
            SQLiteCommand sqlCmd = new SQLiteCommand(query, _dbConn);
            SQLiteDataReader sqlReader = sqlCmd.ExecuteReader();
            while(sqlReader.Read())
            {
                bool temp;
                if (sqlReader.GetValue(4).Equals("0"))
                    temp = false;
                else
                    temp = true;
                ContactModel contact = new ContactModel(Convert.ToInt32(sqlReader.GetValue(0)),(string)sqlReader.GetValue(1), (string)sqlReader.GetValue(2), (string)sqlReader.GetValue(3), temp);
                contactList.Add(contact);
            }

            _dbConn.Close();
            return contactList;
        }

        public ObservableCollection<ContactModel> ContactList
        {
            get {return _contactList; }
            set
            {
                if (value != _contactList)
                {
                    _contactList = value;
                    OnPropertyChanged("ContactList");
                }
            }
        }

        public ICollectionView ContactListView
        {
            get { return CollectionViewSource.GetDefaultView(_contactList); }
            set
            {
               
                    _contactListView = value;
                    OnPropertyChanged("ContactListView");
                
            }
        }

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                if(_selectedContact != null)
                {
                    ContactName = _selectedContact.ToString();
                    ContactFName = _selectedContact.FName;
                    ContactLName = _selectedContact.LName;
                    ContactEmail = _selectedContact.Email;
                }
                
            }
        }

        public string ContactName
        {
            get { return SelectedContact.ToString(); }
            set
            {
                OnPropertyChanged("ContactName");
            }
        }

        public string ContactLName
        {
            get { return SelectedContact.LName; }
            set
            {
                OnPropertyChanged("ContactLName");
            }
        }

        public string ContactFName
        {
            get { return SelectedContact.FName; }
            set
            {
                OnPropertyChanged("ContactFName");
            }
        }

        public string ContactEmail
        {
            get { return SelectedContact.Email; }
            set
            {
                OnPropertyChanged("ContactEmail");
            }
        }

        public string SortSelection
        {
            get { return _sortSelection; }
            set
            {
                _sortSelection = value;

                if (value.Contains("First Name"))
                {
                    SetContactSort(false);
                }
                else if(value.Contains("Last Name"))
                {
                    SetContactSort(true);
                }

                ContactList = GetContactList();
                ContactListView = CollectionViewSource.GetDefaultView(_contactList);
            }
        }

        private void SetContactSort(bool flag)
        {
            foreach (ContactModel contact in _contactList)
            {
                _lnSort = flag;
                contact.LNSort = flag;
            }
        }

        public string LetterFilter
        {
            get { return _letterFilter; }
            set
            {
                if (value != _letterFilter)
                {
                    if (value.Equals(""))
                    {
                        _letterFilter = "All letters";
                    }
                    else
                    {
                        if (value.Contains("All"))
                        {
                            _letterFilter = value;
                        }
                        else
                        {
                            _letterFilter = value.Substring(0, 1);
                            
                        }
                    }
                    
                    OnPropertyChanged("LetterFilter");
                    FilterContactList();
                }
            }
        }

#region Commands

        private void FilterContactList()
        {
            if(_letterFilter.Contains("All"))
            {
                ContactListView.Filter = (item) => { return true; };
            }
            else
            {
                if (_lnSort)
                {
                    ContactListView.Filter = (item) => { return (item as ContactModel).LName.ToLower().StartsWith(_letterFilter); };
                }
                else
                {
                    ContactListView.Filter = (item) => { return (item as ContactModel).FName.ToLower().StartsWith(_letterFilter); };
                }
            }
            
            
        }


        //public ICommand FilterContactList { get { return new RelayCommand(param => FilterContactList(), param => true); } }

#endregion
    }
}
