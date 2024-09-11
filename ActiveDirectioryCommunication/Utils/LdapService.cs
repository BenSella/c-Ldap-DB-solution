using ActiveDirectioryCommunication.Utils.Interfaces;
using System.DirectoryServices.Protocols;
using System.Net;

namespace ActiveDirectioryCommunication.Utils
{
    /// <summary>
    /// LdapService provides methods to interact with an LDAP server, such as Active Directory.
    /// It establishes connections and sends search requests to retrieve directory information.
    /// </summary>
    public class LdapService : ILdapService
    {
        private readonly string _ldapPath;        // The LDAP server address
        private readonly string _ldapUsername;    // The username used to authenticate to the LDAP server
        private readonly string _ldapPassword;    // The password used to authenticate to the LDAP server
        private readonly string _domainName;      // The domain name of the LDAP server
        private readonly ILogger<LdapService> _logger;  // Logger for logging operations and errors

        /// <summary>
        /// Constructor to initialize the LdapService with the LDAP server path, credentials, domain, and logger.
        /// </summary>
        /// <param name="ldapPath">The LDAP server address</param>
        /// <param name="ldapUsername">The username used for LDAP authentication</param>
        /// <param name="ldapPassword">The password used for LDAP authentication</param>
        /// <param name="domainName">The domain name of the LDAP server</param>
        /// <param name="logger">The logger instance for logging actions and errors</param>
        public LdapService(
            string ldapPath,
            string ldapUsername,
            string ldapPassword,
            string domainName,
            ILogger<LdapService> logger)
        {
            _ldapPath = ldapPath;
            _ldapUsername = ldapUsername;
            _ldapPassword = ldapPassword;
            _domainName = domainName;
            _logger = logger;
        }

        /// <summary>
        /// Establishes and returns an LDAP connection asynchronously.
        /// It sets up the connection using the specified path, credentials, and domain.
        /// </summary>
        /// <returns>A Task that resolves to an LdapConnection object</returns>
        public async Task<LdapConnection> GetLdapConnection()
        {
            try
            {
                // Create an LdapConnection with the specified path and credentials
                var ldapConnection = new LdapConnection(new LdapDirectoryIdentifier(_ldapPath, 389),
                    new NetworkCredential()
                    {
                        UserName = string.Format("{0}\\{1}", _domainName, _ldapUsername),
                        Password = _ldapPassword
                    }, AuthType.Basic);

                // Set LDAP session options
                ldapConnection.SessionOptions.ProtocolVersion = 3;
                ldapConnection.AutoBind = true;
                ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                ldapConnection.Bind();  // Bind to the LDAP server (synchronously)

                _logger.LogInformation("Successfully connected to LDAP server.");

                return ldapConnection;  // Return the established LDAP connection
            }
            catch (LdapException ldapEx)
            {
                _logger.LogError($"LDAP error occurred: {ldapEx.Message}");  // Log any LDAP-specific errors
                throw;  // Re-throw the exception to propagate it up the call stack
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while connecting to LDAP: {ex.Message}");  // Log any general exceptions
                throw;  // Re-throw the exception
            }
        }

        /// <summary>
        /// Sends an LDAP search request and returns the search response asynchronously.
        /// It queries the LDAP server for specific attributes under the given base DN.
        /// </summary>
        /// <param name="baseDn">The base DN (Distinguished Name) under which to perform the search</param>
        /// <param name="filter">The LDAP search filter string used to narrow the search results</param>
        /// <param name="attributes">An array of attribute names to retrieve in the search result</param>
        /// <returns>A Task that resolves to a SearchResponse object containing the LDAP search results</returns>
        public async Task<SearchResponse> SendLdapRequest(string baseDn, string filter, params string[] attributes)
        {
            using (var ldapConnection = await GetLdapConnection())  // Establish connection asynchronously
            {
                // Construct an LDAP search request with the specified base DN, filter, and attributes
                var request = new SearchRequest(
                    baseDn,
                    filter,
                    SearchScope.Subtree,  // Perform the search in all subtrees
                    attributes
                );

                _logger.LogInformation($"Sending LDAP request with filter: {filter}");

                // Send the request and return the response (synchronously, as LDAP requests are not inherently async)
                return (SearchResponse)ldapConnection.SendRequest(request);
            }
        }
    }
}
