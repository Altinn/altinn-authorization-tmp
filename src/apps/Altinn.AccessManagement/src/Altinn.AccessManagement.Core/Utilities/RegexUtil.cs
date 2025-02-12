using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Utilities
{
    /// <summary>
    /// Compilation of different regex checks
    /// </summary>
    public static class RegexUtil
    {
        private static readonly Regex OrgNumber = new Regex(@"^\d{9}$");

        private static readonly Regex SSN = new Regex(@"^\d{11}$");

        private static readonly Regex Username = new Regex(@"(?!^\d+$)^[a-zA-Z0-9._@\-]{6,64}$");

        private static readonly Regex MaskinportenScopes = new Regex(@"^([a-z0-9]+:)([a-z0-9]+\/?)+[a-z0-9]+(\.[a-z0-9]+)?$");

        /// <summary>
        /// Check for Orgnumber
        /// Should be 9 numbers
        /// </summary>
        /// <param name="orgnumber">orgnumer to check</param>
        /// <returns>result of regex match</returns>
        public static bool CheckOrgNumber(string orgnumber)
        {
            return OrgNumber.IsMatch(orgnumber);
        }

        /// <summary>
        /// Checks if it is a valid username
        /// Should be between 6-64, can contain number, letters, ., _, @, -
        /// Can not only be numbers
        /// </summary>
        /// <param name="username">the username</param>
        /// <returns>the result of the check</returns>
        public static bool CheckUsername(string username)
        {
            return Username.IsMatch(username);
        }

        /// <summary>
        /// Checks if it is a valid ssn
        /// Should be 9 chars long, can only be numbers
        /// </summary>
        /// <param name="ssn">the ssn</param>
        /// <returns>the result of the check</returns>
        public static bool CheckSSN(string ssn)
        {
            return SSN.IsMatch(ssn);
        }
    }
}
