using System;
using System.Collections.Generic;
using System.Text;

namespace CsDO.Membership
{

    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false)]
    public class User_Name : Attribute
    {
        public User_Name() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_Email : Attribute
    {
        public User_Email() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_PasswordQuestion : Attribute
    {
        public User_PasswordQuestion() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_Comment : Attribute
    {
        public User_Comment() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_IsApproved : Attribute
    {
        public User_IsApproved() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_IsLockedOut : Attribute
    {
        public User_IsLockedOut() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_CreationDate : Attribute
    {
        public User_CreationDate() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_LastLoginDate : Attribute
    {
        public User_LastLoginDate() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_LastActivityDate : Attribute
    {
        public User_LastActivityDate() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_LastPasswordChangedDate : Attribute
    {
        public User_LastPasswordChangedDate() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_LastLockedOutDate : Attribute
    {
        public User_LastLockedOutDate() { }
    }
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_Password : Attribute
    {
        public User_Password() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_PasswordAnswer : Attribute
    {
        public User_PasswordAnswer() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_IsOnLine : Attribute
    {
        public User_IsOnLine() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_FailedPasswordAttemptCount : Attribute
    {
        public User_FailedPasswordAttemptCount() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_FailedPasswordAttemptWindowStart : Attribute
    {
        public User_FailedPasswordAttemptWindowStart() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_FailedPasswordAnswerAttemptCount : Attribute
    {
        public User_FailedPasswordAnswerAttemptCount() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class User_FailedPasswordAnswerAttemptWindowStart : Attribute
    {
        public User_FailedPasswordAnswerAttemptWindowStart() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Role_Name : Attribute
    {
        public Role_Name() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UsersInRoles_User : Attribute
    {
        public UsersInRoles_User() { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UsersInRoles_Role : Attribute
    {
        public UsersInRoles_Role() { }
    }
}
