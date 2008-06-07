using System;
using System.Text;
using System.Web.Security;
using System.Collections.Specialized;
using System.Security.Cryptography;
using CsDO.Lib;
using System.Web.Configuration;
using System.Configuration.Provider;
using System.Configuration;
using System.Web.Hosting;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections;

namespace CsDO.Membership
{
    public sealed class CsDOMembershipProvider : MembershipProvider
    {
        #region Private Fields
        private int newPasswordLength = 8;
        private MachineKeySection machineKey;
        private Type _userObjectType;
        internal PropertyInfo _User_Name;
        internal PropertyInfo _User_Email;
        internal PropertyInfo _User_PasswordQuestion;
        internal PropertyInfo _User_Comment;
        internal PropertyInfo _User_IsApproved;
        internal PropertyInfo _User_IsLockedOut;
        internal PropertyInfo _User_CreationDate;
        internal PropertyInfo _User_LastLoginDate;
        internal PropertyInfo _User_LastActivityDate;
        internal PropertyInfo _User_LastPasswordChangedDate;
        internal PropertyInfo _User_LastLockedOutDate;
        internal PropertyInfo _User_Password;
        internal PropertyInfo _User_PasswordAnswer;
        internal PropertyInfo _User_IsOnLine;
        internal PropertyInfo _User_FailedPasswordAttemptCount;
        internal PropertyInfo _User_FailedPasswordAttemptWindowStart;
        internal PropertyInfo _User_FailedPasswordAnswerAttemptCount;
        internal PropertyInfo _User_FailedPasswordAnswerAttemptWindowStart;

        private string assemblyName;
        private string roleType;
        private string usersInRolesType;
        #endregion

        #region Helper functions
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        internal DataObject GetUser(string username)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            if (data.fetch())
                return data;
            else
                return null;
        }

        internal DataObject NewUser()
        {
            return (DataObject) Activator.CreateInstance(_userObjectType);
        }

        private PropertyInfo GetProperty(Type customAttribute)
        {
            foreach (PropertyInfo pi in _userObjectType.GetProperties())
                if (pi.GetType().GetCustomAttributes(customAttribute, true).Length > 0)
                    return pi;

            return null;
        }

        private void UpdateFailureCount(string username, string failureType)
        {
                DataObject data = NewUser();
                _User_Name.SetValue(data, username, null);
                data.find();
                if (data.fetch())
                {
                    DateTime windowStart = new DateTime();
                    int failureCount = 0;

                    if (failureType == "password")
                    {
                        failureCount = (int)_User_FailedPasswordAttemptCount.GetValue(data, null);
                        windowStart = (DateTime)_User_FailedPasswordAttemptWindowStart.GetValue(data, null);
                    }

                    if (failureType == "passwordAnswer")
                    {
                        failureCount = (int)_User_FailedPasswordAnswerAttemptCount.GetValue(data, null);
                        windowStart = (DateTime)_User_FailedPasswordAnswerAttemptWindowStart.GetValue(data, null);
                    }

                    DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                    if (failureCount == 0 || DateTime.Now > windowEnd)
                    {
                        // First password failure or outside of PasswordAttemptWindow. 
                        // Start a new password failure count from 1 and a new window starting now.

                        if (failureType == "password")
                        {
                            _User_FailedPasswordAttemptCount.SetValue(data, 1, null);
                            _User_FailedPasswordAttemptWindowStart.SetValue(data, DateTime.Now, null);
                        }

                        if (failureType == "passwordAnswer")
                        {
                            _User_FailedPasswordAnswerAttemptCount.SetValue(data, 1, null);
                            _User_FailedPasswordAnswerAttemptWindowStart.SetValue(data, DateTime.Now, null);
                        }

                        data.update();
                    }
                    else
                    {
                        if (failureCount++ >= MaxInvalidPasswordAttempts)
                        {
                            // Password attempts have exceeded the failure threshold. Lock out
                            // the user.

                            _User_IsLockedOut.SetValue(data, true, null);
                            _User_LastLockedOutDate.SetValue(data, DateTime.Now, null);

                            data.update();
                        }
                        else
                        {
                            // Password attempts have not exceeded the failure threshold. Update
                            // the failure counts. Leave the window the same.

                            if (failureType == "password")
                                _User_FailedPasswordAttemptCount.SetValue(data, failureCount, null);

                            if (failureType == "passwordAnswer")
                                _User_FailedPasswordAnswerAttemptCount.SetValue(data, failureCount, null);

                            data.update();
                        }
                    }
                }
                else
                {
                    throw new ProviderException("Unable to update failure count and window start.");
                }
        }

        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(machineKey.ValidationKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private MembershipUser GetUserFromReader(DataObject data)
        {
            object providerUserKey = data.getPrimaryKey();
            string username = (string) _User_Name.GetValue(data, null);
            string email = (string)_User_Email.GetValue(data, null);
            string passwordQuestion = (string)_User_PasswordQuestion.GetValue(data, null);
            string comment = (string)_User_Comment.GetValue(data, null);
            bool isApproved = (bool)_User_IsApproved.GetValue(data, null);
            bool isLockedOut = (bool)_User_IsLockedOut.GetValue(data, null);
            DateTime creationDate = (DateTime)_User_CreationDate.GetValue(data, null);
            DateTime lastLoginDate = (DateTime)_User_LastLoginDate.GetValue(data, null);
            DateTime lastActivityDate = (DateTime)_User_LastActivityDate.GetValue(data, null);
            DateTime lastPasswordChangedDate = (DateTime)_User_LastPasswordChangedDate.GetValue(data, null);
            DateTime lastLockedOutDate = (DateTime)_User_LastLockedOutDate.GetValue(data, null);

            MembershipUser u = new MembershipUser(this.Name,
                                                  username,
                                                  providerUserKey,
                                                  email,
                                                  passwordQuestion,
                                                  comment,
                                                  isApproved,
                                                  isLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);

            return u;
        }

        internal void SetupParameters(string assemblyName, string userObjectType)
        {
            ObjectHandle obj = Activator.CreateInstance(assemblyName, userObjectType);
            _userObjectType = obj.Unwrap().GetType();
            if (!_userObjectType.IsSubclassOf(typeof(DataObject)) && _userObjectType != typeof(DataObject))
                throw new ProviderException("User Data Object should be a subtype of CsDO.Lib.DataObject .");

            _User_Comment = GetProperty(typeof(User_Comment));
            if (_User_Comment == null)
                throw new ProviderException("Must have Comment field.");
            _User_CreationDate = GetProperty(typeof(User_CreationDate));
            if (_User_CreationDate == null)
                throw new ProviderException("Must have Creation Date field.");
            _User_Email = GetProperty(typeof(User_Email));
            if (_User_Email == null)
                throw new ProviderException("Must have User Email field.");
            _User_IsApproved = GetProperty(typeof(User_IsApproved));
            if (_User_IsApproved == null)
                throw new ProviderException("Must have Approved field.");
            _User_IsLockedOut = GetProperty(typeof(User_IsLockedOut));
            if (_User_IsLockedOut == null)
                throw new ProviderException("Must have Locked Out field.");
            _User_LastActivityDate = GetProperty(typeof(User_LastActivityDate));
            if (_User_LastActivityDate == null)
                throw new ProviderException("Must have Last Activity Date field.");
            _User_LastLockedOutDate = GetProperty(typeof(User_LastLockedOutDate));
            if (_User_LastLockedOutDate == null)
                throw new ProviderException("Must have Last Locked Out Date field.");
            _User_LastLoginDate = GetProperty(typeof(User_LastLoginDate));
            if (_User_LastLoginDate == null)
                throw new ProviderException("Must have last Login Date field.");
            _User_LastPasswordChangedDate = GetProperty(typeof(User_LastPasswordChangedDate));
            if (_User_LastPasswordChangedDate == null)
                throw new ProviderException("Must have last Password Changed Date field.");
            _User_Name = GetProperty(typeof(User_Name));
            if (_User_Name == null)
                throw new ProviderException("Must have User Name field.");
            _User_LastLoginDate = GetProperty(typeof(User_LastLoginDate));
            if (_User_LastLoginDate == null)
                throw new ProviderException("Must have last Login Date field.");
            _User_LastActivityDate = GetProperty(typeof(User_LastActivityDate));
            if (_User_LastActivityDate == null)
                throw new ProviderException("Must have Last Activity Date field.");
            _User_LastPasswordChangedDate = GetProperty(typeof(User_LastPasswordChangedDate));
            if (_User_LastPasswordChangedDate == null)
                throw new ProviderException("Must have Last Password Changed Date field.");
            _User_LastLockedOutDate = GetProperty(typeof(User_LastLockedOutDate));
            if (_User_LastLockedOutDate == null)
                throw new ProviderException("Must have Last Locked Out Date field.");
            _User_Password = GetProperty(typeof(User_Password));
            if (_User_Password == null)
                throw new ProviderException("Must have User Password field.");
            _User_PasswordQuestion = GetProperty(typeof(User_PasswordQuestion));
            if (_User_PasswordQuestion == null)
                throw new ProviderException("Must have Password Question field.");
            _User_PasswordAnswer = GetProperty(typeof(User_PasswordAnswer));
            if (_User_PasswordAnswer == null)
                throw new ProviderException("Must have Password Answer field.");
            _User_IsOnLine = GetProperty(typeof(User_IsOnLine));
            if (_User_IsOnLine == null)
                throw new ProviderException("Must have Online field.");
            _User_FailedPasswordAttemptCount = GetProperty(typeof(User_FailedPasswordAttemptCount));
            if (_User_FailedPasswordAttemptCount == null)
                throw new ProviderException("Must have Failed Password Attempt Count field.");
            _User_FailedPasswordAttemptWindowStart = GetProperty(typeof(User_FailedPasswordAttemptWindowStart));
            if (_User_FailedPasswordAttemptWindowStart == null)
                throw new ProviderException("Must have Failed Password Attempt Window Start field.");
            _User_FailedPasswordAnswerAttemptCount = GetProperty(typeof(User_FailedPasswordAnswerAttemptCount));
            if (_User_FailedPasswordAnswerAttemptCount == null)
                throw new ProviderException("Must have Failed Password Answer Attempt Count field.");
            _User_FailedPasswordAnswerAttemptWindowStart = GetProperty(typeof(User_FailedPasswordAnswerAttemptWindowStart));
            if (_User_FailedPasswordAnswerAttemptWindowStart == null)
                throw new ProviderException("Must have Failed Password Answer Attempt Window Start field.");
        }
        #endregion

        #region MembershipProvider Properties
        private string _ApplicationName;
        private bool _EnablePasswordReset;
        private bool _EnablePasswordRetrieval;
        private bool _RequiresQuestionAndAnswer;
        private bool _RequiresUniqueEmail;
        private int _MaxInvalidPasswordAttempts;
        private int _PasswordAttemptWindow;
        private MembershipPasswordFormat _PasswordFormat;
        private int _MinRequiredNonAlphanumericCharacters;
        private int _MinRequiredPasswordLength;
        private string _PasswordStrengthRegularExpression;

        public override string ApplicationName
        {
            get { return _ApplicationName; }
            set { _ApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _EnablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _RequiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _MaxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _PasswordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }
        #endregion

        #region MembershipProvider methods
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "CsDOMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "CsDO Membership Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            _ApplicationName = GetConfigValue(config["applicationName"],
                System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _MaxInvalidPasswordAttempts = Convert.ToInt32(
                GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _PasswordAttemptWindow = Convert.ToInt32(
                GetConfigValue(config["passwordAttemptWindow"], "10"));
            _MinRequiredNonAlphanumericCharacters = Convert.ToInt32(
                GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _MinRequiredPasswordLength = Convert.ToInt32(
                GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _PasswordStrengthRegularExpression = Convert.ToString(
                GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            _EnablePasswordReset = Convert.ToBoolean(
                GetConfigValue(config["enablePasswordReset"], "true"));
            _EnablePasswordRetrieval = Convert.ToBoolean(
                GetConfigValue(config["enablePasswordRetrieval"], "true"));
            _RequiresQuestionAndAnswer = Convert.ToBoolean(
                GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _RequiresUniqueEmail = Convert.ToBoolean(
                GetConfigValue(config["requiresUniqueEmail"], "true"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    _PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    _PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    _PasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            assemblyName = config["assemblyName"];
            roleType = config["roleObjectType"];
            usersInRolesType = config["usersInRolesObjectType"];
            SetupParameters(config["assemblyName"], config["userObjectType"]);

            Configuration cfg =
              WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");
        }

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
                return false;

            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();
            if (data.fetch())
            {
                _User_Password.SetValue(data, EncodePassword(newPwd), null);
                _User_LastPasswordChangedDate.SetValue(data, DateTime.Now, null);

                return data.update();
            }
            else
                return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username,
                      string password,
                      string newPwdQuestion,
                      string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();
            if (data.fetch())
            {
                _User_PasswordQuestion.SetValue(data, newPwdQuestion, null);
                _User_PasswordAnswer.SetValue(data, EncodePassword(newPwdAnswer), null);

                return data.update();
            }
            else
                return false;
        }

        public override MembershipUser CreateUser(string username,
                 string password,
                 string email,
                 string passwordQuestion,
                 string passwordAnswer,
                 bool isApproved,
                 object providerUserKey,
                 out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                DataObject data = NewUser();
                data.setPrimaryKey(providerUserKey);

                _User_Name.SetValue(data, username, null);
                _User_Password.SetValue(data, EncodePassword(password), null);
                _User_Email.SetValue(data, email, null);
                _User_PasswordQuestion.SetValue(data, passwordQuestion, null);
                _User_PasswordAnswer.SetValue(data, EncodePassword(passwordAnswer), null);
                _User_IsApproved.SetValue(data, isApproved, null);
                _User_Comment.SetValue(data, "", null);
                _User_CreationDate.SetValue(data, createDate, null);
                _User_LastPasswordChangedDate.SetValue(data, createDate, null);
                _User_LastActivityDate.SetValue(data, createDate, null);
                _User_IsLockedOut.SetValue(data, false, null);
                _User_LastLockedOutDate.SetValue(data, createDate, null);
                _User_FailedPasswordAttemptCount.SetValue(data, 0, null);
                _User_FailedPasswordAttemptWindowStart.SetValue(data, createDate, null);
                _User_FailedPasswordAnswerAttemptCount.SetValue(data, 0, null);
                _User_FailedPasswordAnswerAttemptWindowStart.SetValue(data, 0, null);
   
                if (data.insert())
                {
                    status = MembershipCreateStatus.Success;
                }
                else
                {
                    status = MembershipCreateStatus.UserRejected;
                }

                return GetUser(username, false);
            }
            else
            {

                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();
            if (data.fetch())
            {
                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
                }

                return data.delete();
            }
            else
                return false;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, 
                    int pageSize, 
                    out int totalRecords)
        {
            DataObject data = NewUser();

            MembershipUserCollection users = new MembershipUserCollection();

            IList list = data.ToArray(true);
            totalRecords = list.Count;

            if (totalRecords <= 0) { return users; }

            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            for (int i = startIndex; i < endIndex; i++)
            {
                MembershipUser u = GetUserFromReader((DataObject) list[i]);
                users.Add(u);
            }

            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            DataObject data = NewUser();
            IList list = data.ToArray(true);

            int numOnline = 0;

            foreach (DataObject item in list)
            {
                if ((DateTime)_User_LastActivityDate.GetValue(item, null) > compareTime)
                    numOnline++;
            }

            return numOnline;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            string password = "";
            string passwordAnswer = "";

            if (data.fetch())
            {
                if ((bool) _User_IsLockedOut.GetValue(data, null))
                    throw new MembershipPasswordException("The supplied user is locked out.");

                password = (string) _User_Password.GetValue(data, null);
                passwordAnswer = (string) _User_PasswordAnswer.GetValue(data, null);
            }
            else
            {
                throw new MembershipPasswordException("The supplied user name is not found.");
            }

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }


            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            if (data.fetch())
            {
                MembershipUser u = null;

                u = GetUserFromReader(data);

                if (userIsOnline)
                {
                    _User_LastActivityDate.SetValue(data, DateTime.Now, null);
                    data.update();
                }
  
                return u;
            }
            else
                throw new ProviderException("User not found on database");
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            DataObject data = NewUser();
            if (data.Get(providerUserKey))
            {
                MembershipUser u = null;

                u = GetUserFromReader(data);

                if (userIsOnline)
                {
                    _User_LastActivityDate.SetValue(data, DateTime.Now, null);
                    data.update();
                }

                return u;
            }
            else
                throw new ProviderException("User not found on database");
        }

        public override bool UnlockUser(string username)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            if (data.fetch())
            {
                _User_IsLockedOut.SetValue(data, false, null);
                _User_LastLockedOutDate.SetValue(data, DateTime.Now, null);

                return data.update();
            }
            else
                return false;
        }

        public override string GetUserNameByEmail(string email)
        {
            DataObject data = NewUser();
            _User_Email.SetValue(data, email, null);
            data.find();

            string username = "";

            if (data.fetch())
            {
                username = (string)_User_Name.GetValue(data, null);
            }
            
            if (username == null)
                username = "";

            return username;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword =
              System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);


            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");

            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            if (data.fetch())
            {
                string passwordAnswer = "";

                if ((bool) _User_IsLockedOut.GetValue(data, null))
                    throw new MembershipPasswordException("The supplied user is locked out.");

                passwordAnswer = (string)_User_Password.GetValue(data, null);

                if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                _User_Password.SetValue(data, EncodePassword(newPassword), null);
                _User_LastPasswordChangedDate.SetValue(data, DateTime.Now, null);

                if(data.update())
                {
                    return newPassword;
                }
            }
            else
            {
                throw new MembershipPasswordException("The supplied user name is not found.");
            }

            throw new MembershipPasswordException("The supplied password could not be applied.");
        }

        public override void UpdateUser(MembershipUser user)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, user.UserName, null);
            data.find();

            if (data.fetch())
            {
                _User_Email.SetValue(data, user.Email, null);
                _User_Comment.SetValue(data, user.Comment, null);
                _User_IsApproved.SetValue(data, user.IsApproved, null);

                data.update();
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            DataObject data = NewUser();
            _User_Name.SetValue(data, username, null);
            data.find();

            if (data.fetch())
            {
                bool isApproved = false;
                string pwd = "";

                pwd = (string) _User_Password.GetValue(data, null);
                isApproved = (bool) _User_IsApproved.GetValue(data, null);

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;

                        _User_LastLoginDate.SetValue(data, DateTime.Now, null);
                        data.update();
                    }
                }
                else
                {
                    UpdateFailureCount(username, "password");
                }

                return isValid;
            }
            else
                return false;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, 
                                            int pageIndex, 
                                            int pageSize, 
                                            out int totalRecords)
        {
            DataObject data = NewUser();
            _User_Name.SetValue(data, usernameToMatch, null);
            data.find();

            IList list = data.ToArray();
            totalRecords = list.Count;

            MembershipUserCollection users = new MembershipUserCollection();

            if (totalRecords > 0)
            {
                if (totalRecords <= 0) { return users; }

                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                for (int i = startIndex; i < endIndex; i++)
                {
                    MembershipUser u = GetUserFromReader((DataObject) list[i]);
                    users.Add(u);
                }
            }

            return users;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, 
                                            int pageIndex, 
                                            int pageSize, 
                                            out int totalRecords)
        {
            DataObject data = NewUser();
            _User_Email.SetValue(data, emailToMatch, null);
            data.find();

            MembershipUserCollection users = new MembershipUserCollection();

            IList list = data.ToArray();
            totalRecords = list.Count;

            if (totalRecords > 0)
            {
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                for (int i = startIndex; i < endIndex; i++)
                {
                    MembershipUser u = GetUserFromReader((DataObject)list[i]);
                    users.Add(u);
                }
            }

            return users;
        }
        #endregion
    }
}