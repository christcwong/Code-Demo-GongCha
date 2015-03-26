using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Helpers
{
    public class GongChaMemberShipRoleProvider :System.Web.Security.RoleProvider
    {
        private static Models.GongChaDbContext db = new Models.GongChaDbContext();
        public override string[] GetRolesForUser(string username)
        {
            return (from m in db.Members where m.Email==username select m.AccessLevel).ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return (from m in db.Members where m.AccessLevel == roleName select m.Email).ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetUsersInRole(roleName).Contains(username);
        }

        public override string[] GetAllRoles()
        {
            var roles = (from m in db.Members
                        select m.AccessLevel).Distinct();
            return roles.ToArray();
        }

        #region NotSupportedFunctions
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="roleName"></param>
        public override void CreateRole(string roleName)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not Supported
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion
    }
}