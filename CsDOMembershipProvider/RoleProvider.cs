using System;
using System.Data;
using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using CsDO.Lib;
using System.Collections;

namespace CsDO.Membership
{

    public sealed class CsDORoleProvider : RoleProvider
    {
        #region Private Fields
        private Type _rolesObjectType;
        internal PropertyInfo _Role_Name;
        internal Type _UsersInRolesObjectType;
        internal PropertyInfo _UsersInRoles_User;
        internal PropertyInfo _UsersInRoles_Role;

        private string assemblyName;
        private string userType;
        #endregion

        #region Helper Functions
        internal DataObject NewRole()
        {
            return (DataObject)Activator.CreateInstance(_rolesObjectType);
        }

        private PropertyInfo GetRoleProperty(Type customAttribute)
        {
            foreach (PropertyInfo pi in _rolesObjectType.GetProperties())
                if (pi.GetType().GetCustomAttributes(customAttribute, true).Length > 0)
                    return pi;

            return null;
        }

        internal DataObject NewUsersInRoles()
        {
            return (DataObject)Activator.CreateInstance(_UsersInRolesObjectType);
        }

        private PropertyInfo GetUsersInRolesProperty(Type customAttribute)
        {
            foreach (PropertyInfo pi in _UsersInRolesObjectType.GetProperties())
                if (pi.GetType().GetCustomAttributes(customAttribute, true).Length > 0)
                    return pi;

            return null;
        }

        internal DataObject GetRole(string rolename)
        {
            DataObject role = NewRole();
            _Role_Name.SetValue(role, rolename, null);
            role.find();
            if (role.fetch())
                return role;
            else
                return null;
        }

        internal void SetupParameters(string assemblyName, string roleObjectType, string usersInRolesObjectType)
        {
            ObjectHandle obj = Activator.CreateInstance(assemblyName, roleObjectType);
            _rolesObjectType = obj.Unwrap().GetType();
            if (!_rolesObjectType.IsSubclassOf(typeof(DataObject)) && 
                _rolesObjectType != typeof(DataObject))
                throw new ProviderException("Role Data Object should be a subtype of CsDO.Lib.DataObject .");

            _Role_Name = GetRoleProperty(typeof(Role_Name));
            if (_Role_Name == null)
                throw new ProviderException("Role must have Name field.");

            obj = Activator.CreateInstance(assemblyName, usersInRolesObjectType);
            _UsersInRolesObjectType = obj.Unwrap().GetType();
            if (!_UsersInRolesObjectType.IsSubclassOf(typeof(DataObject)) && 
                _UsersInRolesObjectType != typeof(DataObject))
                throw new ProviderException("Users In Roles Data Object should be a subtype of CsDO.Lib.DataObject .");

            _UsersInRoles_User = GetUsersInRolesProperty(typeof(UsersInRoles_User));
            if (_UsersInRoles_User == null)
                throw new ProviderException("Role must have Name field.");

            _UsersInRoles_Role = GetUsersInRolesProperty(typeof(UsersInRoles_Role));
            if (_UsersInRoles_Role == null)
                throw new ProviderException("Role must have Name field.");
        }
        #endregion
        
        #region RoleProvider Properties
        private string _ApplicationName;
        public override string ApplicationName
        {
            get { return _ApplicationName; }
            set { _ApplicationName = value; }
        } 
        #endregion

        #region RoleProvider Methods
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "CsDORoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "CsDO Role Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                _ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                _ApplicationName = config["applicationName"];
            }

            assemblyName = config["assemblyName"];
            userType = config["userObjectType"];
            SetupParameters(config["assemblyName"], config["roleObjectType"], config["usersInRolesObjectType"]);
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.IndexOf(',') > 0)
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }

            CsDOMembershipProvider mp = new CsDOMembershipProvider();
            mp.SetupParameters(assemblyName, userType);

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    DataObject data = NewUsersInRoles();
                    _UsersInRoles_Role.SetValue(data, GetRole(rolename), null);
                    _UsersInRoles_User.SetValue(data, mp.GetUser(username), null);
                    data.insert();
                }
            }
        }

        public override void CreateRole(string rolename)
        {
            if (rolename.IndexOf(',') > 0)
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            DataObject data = NewRole();
            _Role_Name.SetValue(data, rolename, null);
            data.insert();
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            DataObject data = NewRole();
            _UsersInRoles_Role.SetValue(data, rolename, null);
            data.find();
            data.fetch();
            DataObject usersInRoles = NewUsersInRoles();
            _UsersInRoles_Role.SetValue(usersInRoles, data, null);
            IList users = usersInRoles.ToArray(true);

            foreach (DataObject user in users)
                if (!user.delete())
                    return false;

            return data.delete();
        }

        public override string[] GetAllRoles()
        {
            string tmpRoleNames = "";

            DataObject data = NewRole();
            IList roles = data.ToArray(true);

            foreach (DataObject role in roles)
                tmpRoleNames += _Role_Name.GetValue(role, null).ToString();

            if (tmpRoleNames.Length > 0)
            {
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);
                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }

        public override string[] GetRolesForUser(string username)
        {
            string tmpRoleNames = "";

            DataObject data = NewRole();
            _UsersInRoles_User.SetValue(data, username, null);
            IList roles = data.ToArray(true);


            foreach(DataObject role in roles)
            {
                tmpRoleNames += _Role_Name.GetValue(
                    _UsersInRoles_Role.GetValue(role, null), null).ToString() + ",";
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);
                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }

        public override string[] GetUsersInRole(string rolename)
        {
            string tmpUserNames = "";

            DataObject data = NewRole();
            _UsersInRoles_Role.SetValue(data, rolename, null);
            IList users = data.ToArray(true);

            foreach(DataObject user in users)
            {
                CsDOMembershipProvider mp = new CsDOMembershipProvider();
                mp.SetupParameters(assemblyName, userType);

                tmpUserNames += mp._User_Name.GetValue(
                    _UsersInRoles_User.GetValue(user, null), null).ToString() + ",";
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;

            DataObject data = NewRole();
            _UsersInRoles_Role.SetValue(data, rolename, null);
            _UsersInRoles_User.SetValue(data, username, null);
            IList users = data.ToArray(true);

            if (users.Count > 0)
            {
                userIsInRole = true;
            }

            return userIsInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }

            CsDOMembershipProvider mp = new CsDOMembershipProvider();
            mp.SetupParameters(assemblyName, userType);

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    DataObject role = NewRole();
                    _Role_Name.SetValue(role, rolename, null);
                    role.find();
                    role.fetch();
                    DataObject user = mp.NewUser();
                    mp._User_Name.SetValue(user, username, null);
                    role.find();
                    role.fetch();
                    DataObject usersInRoles = NewUsersInRoles();
                    _UsersInRoles_Role.SetValue(usersInRoles, role, null);
                    _UsersInRoles_User.SetValue(usersInRoles, user, null);
                    IList users = usersInRoles.ToArray(true);

                    foreach (DataObject item in users)
                        item.delete();
                }
            }
        }

        public override bool RoleExists(string rolename)
        {
            bool exists = false;

            DataObject roles = NewRole();
            _Role_Name.SetValue(roles, rolename, null);
            IList users = roles.ToArray(true);

            if (users.Count > 0)
            {
                exists = true;
            }

            return exists;
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            string tmpUserNames = "";

            CsDOMembershipProvider mp = new CsDOMembershipProvider();
            mp.SetupParameters(assemblyName, userType);

            DataObject role = NewRole();
            _Role_Name.SetValue(role, rolename, null);
            role.find();
            role.fetch();
            DataObject user = mp.NewUser();
            mp._User_Name.SetValue(user, usernameToMatch, null);
            role.find();
            role.fetch();
            DataObject usersInRoles = NewUsersInRoles();
            _UsersInRoles_Role.SetValue(usersInRoles, role, null);
            _UsersInRoles_User.SetValue(usersInRoles, user, null);
            IList users = usersInRoles.ToArray(true);

            foreach(DataObject item in users)
            {
                tmpUserNames += mp._User_Name.GetValue(item, null).ToString() + ",";
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }
        #endregion
    }
}