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
        private ContactModel _backupSelectedContact;
        private ICollectionView _contactListView;
        private string _sortSelection;
        private SQLiteConnection _dbConn;
        private bool _lnSort;
        private string _letterFilter;

        public ContactViewModel()
        {
            _selectedContact = new ContactModel();
            _backupSelectedContact = new ContactModel();
            _contactDb = new ContactDatabase();
            _lnSort = true;
            //_contactList.Add(new ContactModel("Tu", "Nguyen", "abc@xyz.com", true));
            // _contactList.Add(new ContactModel("Chi", "Tran", "abc@xyz.com", true));
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            RefreshContactList();
            
            SelectedContact = ContactList.First();
            _backupSelectedContact = SelectedContact;
            LetterFilter = "All letters";
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
            while (sqlReader.Read())
            {
                bool temp;
                if (sqlReader.GetValue(4).Equals("0"))
                    temp = false;
                else
                    temp = true;
                ContactModel contact = new ContactModel(Convert.ToInt32(sqlReader.GetValue(0)), (string)sqlReader.GetValue(1), (string)sqlReader.GetValue(2), (string)sqlReader.GetValue(3), temp);
                contactList.Add(contact);
            }

            _dbConn.Close();
            return contactList;
        }

        public ObservableCollection<ContactModel> ContactList
        {
            get { return _contactList; }
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
            get { return CollectionViewSource.GetDefaultView(ContactList); }
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
                
                if (value != null && _selectedContact!=null)
                {
                    _selectedContact = value;
                    CopyContact(_selectedContact,_backupSelectedContact);
                    ContactName = _selectedContact.ToString();
                    ContactFName = _selectedContact.FName;
                    ContactLName = _selectedContact.LName;
                    ContactEmail = _selectedContact.Email;
                    ContactDidSend = _selectedContact.DidSend;
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

        public int ContactId
        {
            get { return SelectedContact.Id; }
            set
            {

            }
        }

        public string ContactLName
        {
            get { return SelectedContact.LName; }
            set
            {
                SelectedContact.LName = value;
                OnPropertyChanged("ContactLName");
            }
        }

        public string ContactFName
        {
            get { return SelectedContact.FName; }
            set
            {
                SelectedContact.FName = value;
                OnPropertyChanged("ContactFName");
            }
        }

        public string ContactEmail
        {
            get { return SelectedContact.Email; }
            set
            {
                SelectedContact.Email = value;
                OnPropertyChanged("ContactEmail");
            }
        }

        public bool ContactDidSend
        {
            get { return SelectedContact.DidSend; }
            set
            {
                SelectedContact.DidSend = value;
                OnPropertyChanged("ContactDidSend");
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
                else if (value.Contains("Last Name"))
                {
                    SetContactSort(true);
                }

                ContactList = GetContactList();
                ContactListView = CollectionViewSource.GetDefaultView(ContactList);
                //SelectedContact = ContactList.First();
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
            if (_letterFilter.Contains("All"))
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

        private void CopyContact(ContactModel from, ContactModel to)
        {
            to.Id = from.Id;
            to.LNSort = from.LNSort;
            to.FName = from.FName;
            to.LName = from.LName;
            to.Email = from.Email;
            to.DidSend = from.DidSend;
        }

        private void CancelEditContactExe()
        {
            CopyContact(_backupSelectedContact, _selectedContact);
            RaiseAllContactChange();

        }

        private void SaveEditContactExe()
        {
            _contactDb.SaveEditContact(SelectedContact.Id, SelectedContact);
            CopyContact(SelectedContact, _backupSelectedContact);
            RefreshContactList();
            CopyContact(_backupSelectedContact, SelectedContact);
            RaiseAllContactChange();

        }

        private bool CanSaveEditContact()
        {
            if (ContactFName.Equals("") || ContactLName.Equals("") || ContactEmail.Equals(""))
                return false;
            else
            {
                try
                {
                    //http://stackoverflow.com/a/1374644

                    var addr = new System.Net.Mail.MailAddress(ContactEmail);
                    return addr.Address == ContactEmail;
                }
                catch
                {
                    return false;
                }

            }

        }

        private void RefreshContactList()
        {
            ContactList = GetContactList();
            ContactListView = CollectionViewSource.GetDefaultView(ContactList);
            
        }

        private void RaiseAllContactChange()
        {
            OnPropertyChanged("ContactName");
            OnPropertyChanged("ContactFName");
            OnPropertyChanged("ContactLName");
            OnPropertyChanged("ContactEmail");
            OnPropertyChanged("ContactDidSend");
        }


        public ICommand CancelEditContact
        {
            get
            {
                return new RelayCommand(CancelEditContactExe);
            }      
        }

        public ICommand SaveEditContact
        {
            get
            {
                return new RelayCommand(SaveEditContactExe, CanSaveEditContact);
            }
        }

#endregion
    }
}
