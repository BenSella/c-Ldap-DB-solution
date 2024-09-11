using ActiveDirectioryCommunication.Objects;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ActiveDirectioryCommunication.Utils.Interfaces;

namespace ActiveDirectioryCommunication.Utils
{
    /// <summary>
    /// This class is responsible for extracting user data from Active Directory (AD) via LDAP.
    /// It uses ILdapService to interact with the AD server and retrieve attributes such as
    /// 'givenName', 'sn', 'displayName', and 'title' for the specified user.
    /// </summary>
    public class UserDataExtractionActiveDirectory
    {
        private readonly ILdapService _ldapService;  // LDAP service interface for querying AD
        private readonly string _baseDn;             // Base DN (Distinguished Name) for the LDAP search
        private readonly ILogger<UserDataExtractionActiveDirectory> _logger;  // Logger for information and error logging
        private readonly string format = "(&(objectClass=user)(sAMAccountName={0}))"; // LDAP filter format for user search

        /// <summary>
        /// Constructor that accepts ILdapService, baseDn, and logger dependencies.
        /// </summary>
        /// <param name="ldapService">Service to interact with LDAP (Active Directory)</param>
        /// <param name="baseDn">The base Distinguished Name (DN) to perform LDAP queries under</param>
        /// <param name="logger">Logger instance for logging LDAP operations</param>
        public UserDataExtractionActiveDirectory(
            ILdapService ldapService,  // Accept the interface instead of the concrete class
            string baseDn,
            ILogger<UserDataExtractionActiveDirectory> logger)
        {
            _ldapService = ldapService;  // Use ILdapService for both real and fake services
            _baseDn = baseDn;
            _logger = logger;
        }

        /// <summary>
        /// Extracts user data from Active Directory based on the given username.
        /// Queries the LDAP server for attributes like 'title', 'givenName', 'sn', and 'displayName'.
        /// </summary>
        /// <param name="userName">The username (sAMAccountName) to search for in Active Directory</param>
        /// <returns>Returns a UserData object with the extracted attributes or empty fields if not found</returns>
        public async Task<UserData?> ExtractData(string userName)  // Nullable return type for safety
        {
            UserData userData = new UserData();
            try
            {
                // Construct LDAP search filter using the provided username
                string filter = string.Format(format, userName);

                // Send LDAP request using ILdapService, asking for specific attributes
                SearchResponse? response = await _ldapService.SendLdapRequest(_baseDn, filter, "title", "givenName", "sn", "displayName");

                // If entries are found in the LDAP response, extract the attributes
                if (response?.Entries?.Count > 0)  // Added null checks for safety
                {
                    var entry = response.Entries[0];

                    // Extract and populate user data from LDAP response attributes
                    userData.UserName = ExtractAttribute(entry, "givenName");
                    userData.UserFamily = ExtractAttribute(entry, "sn");
                    userData.UserFullName = ExtractAttribute(entry, "displayName");
                    userData.UserTitle = ExtractAttribute(entry, "title");

                    _logger.LogInformation($"User '{userName}' found. Full Name: {userData.UserFullName}, Title: {userData.UserTitle}");

                    return userData;
                }
                else
                {
                    _logger.LogWarning($"User '{userName}' not found in Active Directory.");
                    return userData; // Return empty user data if not found
                }
            }
            catch (LdapException ldapEx)
            {
                _logger.LogError($"LDAP error occurred: {ldapEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred: {ex.Message}");
            }

            return userData;  // Return empty user data in case of exceptions
        }

        /// <summary>
        /// Extracts a specific attribute from the LDAP search result entry.
        /// If the attribute exists and has a value, it is returned; otherwise, "No Data" is returned.
        /// </summary>
        /// <param name="entry">LDAP search result entry</param>
        /// <param name="attributeName">The attribute name to extract</param>
        /// <returns>Returns the attribute value if found, otherwise "No Data"</returns>
        private static string ExtractAttribute(SearchResultEntry entry, string attributeName)
        {
            // Check if the attribute exists and contains at least one value
            if (entry.Attributes.Contains(attributeName) && entry.Attributes[attributeName]?.Count > 0)
            {
                return entry.Attributes[attributeName][0].ToString() ?? "No Data";
            }

            return "No Data"; // Default return when the attribute is not found
        }
    }
}
