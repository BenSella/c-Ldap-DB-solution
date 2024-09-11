using System.DirectoryServices.Protocols;
using System.Threading.Tasks;
using ActiveDirectioryCommunication.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace ActiveDirectioryCommunication.Utils
{
    /// <summary>
    /// This class is responsible for checking if a user exists in Active Directory (AD) via LDAP.
    /// It uses ILdapService to interact with AD and retrieve the 'userAccountControl' attribute to 
    /// determine if a user's account is enabled or disabled.
    /// </summary>
    public class UserExistsInActiveDirectory
    {
        private readonly ILdapService _ldapService;  // LDAP service interface for querying AD
        private readonly string _baseDn;             // Base DN (Distinguished Name) for the LDAP search
        private readonly ILogger<UserExistsInActiveDirectory> _logger;  // Logger for logging information and errors
        private readonly string format = "(&(objectClass=user)(sAMAccountName={0}))"; // LDAP filter format for user search

        /// <summary>
        /// Constructor that accepts ILdapService, baseDn, and logger dependencies.
        /// </summary>
        /// <param name="ldapService">Service to interact with LDAP (Active Directory)</param>
        /// <param name="baseDn">The base Distinguished Name (DN) to perform LDAP queries under</param>
        /// <param name="logger">Logger instance for logging LDAP operations</param>
        public UserExistsInActiveDirectory(
            ILdapService ldapService,
            string baseDn,
            ILogger<UserExistsInActiveDirectory> logger)
        {
            _ldapService = ldapService;
            _baseDn = baseDn;
            _logger = logger;
        }

        /// <summary>
        /// Checks if the specified user exists in Active Directory by querying for the 'userAccountControl' attribute.
        /// Determines if the user account is enabled or disabled based on this attribute.
        /// </summary>
        /// <param name="userName">The username (sAMAccountName) to search for in Active Directory</param>
        /// <returns>Returns true if the user exists and the account is enabled, otherwise false</returns>
        public async Task<bool> ExtractData(string userName)
        {
            try
            {
                // Construct LDAP search filter using the provided username
                string filter = string.Format(format, userName);

                // Send LDAP request using ILdapService, asking for 'userAccountControl' attribute
                SearchResponse response = await _ldapService.SendLdapRequest(_baseDn, filter, "userAccountControl");

                // Check if the LDAP response contains entries
                if (response != null && response.Entries.Count > 0)
                {
                    var entry = response.Entries[0];

                    // Check if 'userAccountControl' attribute exists in the response
                    if (entry.Attributes.Contains("userAccountControl"))
                    {
                        // Convert 'userAccountControl' value to integer and check if the account is disabled
                        string userAccountControlString = entry.Attributes["userAccountControl"][0].ToString();
                        int userAccountControl = int.Parse(userAccountControlString);
                        bool isAccountDisabled = IsAccountDisabled(userAccountControl);

                        // Log whether the user account is enabled or disabled
                        _logger.LogInformation($"User account '{userName}' is {(isAccountDisabled ? "disabled" : "enabled")}.");
                        return !isAccountDisabled;  // Return true if the account is enabled
                    }
                    else
                    {
                        _logger.LogWarning($"'userAccountControl' attribute not found for user '{userName}'.");
                        return false;  // Return false if 'userAccountControl' is missing
                    }
                }
                else
                {
                    _logger.LogWarning($"User '{userName}' not found.");
                    return false;  // Return false if the user is not found
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the LDAP query
                _logger.LogError($"An error occurred while extracting data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the 'userAccountControl' attribute indicates that the account is disabled.
        /// </summary>
        /// <param name="userAccountControl">The integer value of the 'userAccountControl' attribute</param>
        /// <returns>Returns true if the account is disabled, otherwise false</returns>
        private static bool IsAccountDisabled(int userAccountControl)
        {
            const int UF_ACCOUNTDISABLE = 0x0002;  // Bitwise flag for account disabled
            return (userAccountControl & UF_ACCOUNTDISABLE) == UF_ACCOUNTDISABLE;
        }
    }
}
