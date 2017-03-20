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
using System.Windows;
using System.Net;
using System.Net.Mail;
using System.Windows.Documents;
using System.IO;

namespace HolidayMailer
{
    class MainViewModel : ObservableObject
    {
        #region Contact members

        private ContactDatabase _contactDb;
        private ObservableCollection<ContactModel> _contactList = new ObservableCollection<ContactModel>();
        private ContactModel _selectedContact;
        private ContactModel _backupSelectedContact;
        private ICollectionView _contactListView;
        private string _sortSelection;
        private SQLiteConnection _dbConn;
        private bool _lnSort;
        private string _letterFilter;
        private bool _toCreateContact;

        #endregion Contact members

        #region Mail members

        private string _fromAddr;
        private string _toAddr;
        private string _mailSubject;
        private string _mailBody;
        private bool _toAll;
        private bool _toPreYear;
        private SmtpClient _mailClient;
        private List<string> _allRecipientsList;
        private List<string> _preYearRecipientsList;
        private bool _hasAttachment;
        private string _attachFullPath;

        #endregion Mail members


        public MainViewModel()
        {
            #region Contact init

            _selectedContact = new ContactModel();
            _backupSelectedContact = new ContactModel();
            _contactDb = new ContactDatabase();
            _lnSort = true;
            _dbConn = new SQLiteConnection("Data Source=contactdb.sqlite;Version=3;");
            RefreshContactList();
            SelectedContact = ContactList.First();
            _backupSelectedContact = SelectedContact;
            LetterFilter = "All letters";

            #endregion Contact init


            #region Mail init

            _fromAddr = "prihaht@gmail.com";
            _toAddr = "";
            _mailBody = "";
            _attachFullPath = "";
            _allRecipientsList = new List<string>();
            _preYearRecipientsList = new List<string>();
            GetAllAddrs();
            GetPreYearAddrs();
            OnPropertyChanged("AttachEnable");
            OnPropertyChanged("DeleteAttachEnable");

            _mailClient = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_fromAddr, ""),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            #endregion Mail init

        }

        #region ContactVM

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

                    if (value != null && _selectedContact != null)
                    {
                        _selectedContact = value;
                        
                        if (!_toCreateContact)
                            CopyContact(_selectedContact, _backupSelectedContact);

                        ContactName = _selectedContact.ToString();
                        ContactFName = _selectedContact.FName;
                        ContactLName = _selectedContact.LName;
                        ContactEmail = _selectedContact.Email;
                        ContactDidSend = _selectedContact.DidSend;

                        //ToAddr = ContactEmail;
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

        private void SaveContactExe()
        {
            if(!_toCreateContact)
            {
                _contactDb.SaveEditContact(SelectedContact.Id, SelectedContact);
            }
            else
            {
                _contactDb.CreateContact(SelectedContact);
            }
            
            CopyContact(SelectedContact, _backupSelectedContact);
            RefreshContactList();
            CopyContact(_backupSelectedContact, SelectedContact);
            RaiseAllContactChange();

        }

        private bool CanSaveContact()
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

        private void SetToCreateContactExe()
        {
            _toCreateContact = true;
            CopyContact(SelectedContact, _backupSelectedContact);
            SelectedContact = new ContactModel();
        }

        private void SetToEditContactExe()
        {
            _toCreateContact = false;
        }

        private void DeleteContactExe()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to delete " + ContactName + " contact?", "Delete " + ContactName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(result == MessageBoxResult.Yes)
            {
                _contactDb.DeleteContact(SelectedContact.Id);
                RefreshContactList();
                SelectedContact = ContactList.First();
                
            }
            else
            {
                return;
            }
        }

        public ICommand SetToCreateContact
        {
            get { return new RelayCommand(SetToCreateContactExe); }
        }

        public ICommand SetToEditContact
        {
            get { return new RelayCommand(SetToEditContactExe); }
        }


        public ICommand CancelEditContact
        {
            get { return new RelayCommand(CancelEditContactExe); }      
        }

        public ICommand SaveContact
        {
            get { return new RelayCommand(SaveContactExe, CanSaveContact); }
        }

        public ICommand DeleteContact
        {
            get { return new RelayCommand(DeleteContactExe); }
        }

        #endregion

        #endregion Contact

        #region MailVM


        public string ToAddr
        {
            get { return _toAddr; }
            set
            {
                _toAddr = value;
                OnPropertyChanged("ToAddr");
            }
        }

        public bool ToAll
        {
            get { return _toAll; }
            set
            {
                _toAll = value;

                if (value == true)
                    ToPreYear = false;

                OnPropertyChanged("ToAll");
            }
        }

        public bool ToPreYear
        {
            get { return _toPreYear; }
            set
            {
                _toPreYear = value;

                if (value == true)
                    ToAll = false;

                OnPropertyChanged("ToPreYear");
            }
        }

        public string MailSubject
        {
            get { return _mailSubject; }
            set
            {
                _mailSubject = value;
                OnPropertyChanged("MailSubject");
            }
        }

        public string MailBody
        {
            get { return _mailBody; }
            set
            {
               _mailBody = value;
               OnPropertyChanged("MailBody");
            }
        }

        public Visibility AttachEnable
        {
            get
            {
                if (_hasAttachment)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility DeleteAttachEnable
        {
            get
            {
                if (!_hasAttachment)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }

        public string AttachFileName
        {
            get
            {
                if (_attachFullPath.Equals(""))
                    return "";
                else
                    return _attachFullPath.Substring(_attachFullPath.LastIndexOf("\\")+1);
            }
            set
            {
                _attachFullPath = value;
                OnPropertyChanged("AttachFileName");
            }
        }

        private void ClearSendMailExe()
        {
            ToAddr = "";
            MailSubject = "";
            MailBody = "";
            AttachFileName = "";
            _hasAttachment = false;
            OnPropertyChanged("AttachEnable");
            OnPropertyChanged("DeleteAttachEnable");
        }

        public ICommand ClearSendMail
        {
            get { return new RelayCommand(ClearSendMailExe); }
        }

        private void SetMailTo1Exe()
        {
            ToAll = false;
            ToPreYear = false;
            ToAddr = "";
        }

        public ICommand SetMailTo1
        {
            get { return new RelayCommand(SetMailTo1Exe); }
        }

        private void SetMailToAllExe()
        {
            ToAll = true;
            GetAllAddrs();
            GetPreYearAddrs();
        }

        public ICommand SetMailToAll
        {
            get { return new RelayCommand(SetMailToAllExe); }
        }

        private void SetMailToPreYearExe()
        {
            ToPreYear = true;
            GetAllAddrs();
            GetPreYearAddrs();
        }

        public ICommand SetMailToPreYear
        {
            get { return new RelayCommand(SetMailToPreYearExe); }
        }

        private void SetMailToContactExe()
        {
            ToAddr = SelectedContact.Email;
        }

        public ICommand SetMailToContact
        {
            get { return new RelayCommand(SetMailToContactExe); }
        }

        private void SendMailExe()
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(_fromAddr);

            if(ToAll)
            {
                foreach (string addr in _allRecipientsList)
                    message.To.Add(addr);

                if(!ToAddr.Equals(""))
                    message.To.Add(ToAddr);
            }
            else
            {
                if (ToPreYear)
                {
                    foreach (string addr in _preYearRecipientsList)
                        message.To.Add(addr);

                    if (!ToAddr.Equals(""))
                        message.To.Add(ToAddr);
                }
                else
                {
                    if (!ToAddr.Equals(""))
                        message.To.Add(ToAddr);
                }
            }

            message.Subject = MailSubject;
            message.Body = MailBody;
            message.Attachments.Add(new Attachment(_attachFullPath));

            _mailClient.Send(message);

            ClearSendMailExe();

        }

        private bool CanSendMail()
        {
            if (!ToAll && !ToPreYear)
                if (ToAddr.Equals(""))
                    return false;
                else
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(ToAddr);
                        return addr.Address == ToAddr;
                    }
                    catch
                    {
                        return false;
                    }
                    
                }

            if(!ToAddr.Equals(""))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(ToAddr);
                    return addr.Address == ToAddr;
                }
                catch
                {
                    return false;
                }
            }

            return true;

        }

        public ICommand SendMail
        {
            get { return new RelayCommand(SendMailExe, CanSendMail); }
        }

        private void BrowseFileExe()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                AttachFileName = dlg.FileName;
                _hasAttachment = true;
                OnPropertyChanged("AttachEnable");
                OnPropertyChanged("DeleteAttachEnable");
            }

        }

        public ICommand BrowseFile
        {
            get { return new RelayCommand(BrowseFileExe); }
        }

        private void DeleteAttachExe()
        {
            AttachFileName = "";
            _hasAttachment = false;
            OnPropertyChanged("AttachEnable");
            OnPropertyChanged("DeleteAttachEnable");

        }

        public ICommand DeleteAttach
        {
            get { return new RelayCommand(DeleteAttachExe); }
        }

        private void GetAllAddrs()
        {
            _allRecipientsList.Clear();

            foreach(ContactModel contact in ContactList)
            {
                _allRecipientsList.Add(contact.Email);
            }
        }

        private void GetPreYearAddrs()
        {
            _preYearRecipientsList.Clear();

            foreach (ContactModel contact in ContactList)
            {
                if(contact.DidSend)
                    _preYearRecipientsList.Add(contact.Email);
            }
        }

        #endregion MailVM
    }
}
