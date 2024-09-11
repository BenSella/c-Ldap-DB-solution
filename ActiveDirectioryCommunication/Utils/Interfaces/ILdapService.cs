using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace ActiveDirectioryCommunication.Utils.Interfaces
{
    /// <summary>
    /// ILdapService interface defines methods for interacting with an LDAP server (such as Active Directory).
    /// It provides methods to establish a connection and send search requests asynchronously.
    /// </summary>
    public interface ILdapService
    {
        /// <summary>
        /// Establishes and returns an LDAP connection asynchronously.
        /// </summary>
        /// <returns>A Task that resolves to an LdapConnection object used to communicate with the LDAP server.</returns>
        Task<LdapConnection> GetLdapConnection(); // Return Task<LdapConnection> for async connection
        /// <summary>
        /// Sends an LDAP search request and returns the search response asynchronously.
        /// </summary>
        /// <param name="baseDn">The base Distinguished Name (DN) under which to search.</param>
        /// <param name="filter">The LDAP filter string to narrow down the search results.</param>
        /// <param name="attributes">An array of attribute names to retrieve in the search results.</param>
        /// <returns>A Task that resolves to a SearchResponse object containing the LDAP search results.</returns>
        Task<SearchResponse> SendLdapRequest(string baseDn, string filter, params string[] attributes); // Return Task<SearchResponse> for async request
    }
}
