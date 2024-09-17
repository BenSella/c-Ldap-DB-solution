project_description 
Project: Active Directory Communication via LDAP in C#

This project demonstrates how to interact with Active Directory (or other LDAP servers) using the LDAP 
protocol in C#. It involves establishing connections, sending LDAP search requests, and extracting useful 
information such as user data or checking if an account is disabled or enabled.

The project is modular and divided into various classes and interfaces to make it scalable and reusable. 
It includes services for establishing LDAP connections, sending queries, and extracting data. The focus is on 
asynchronous programming to ensure that connections and queries do not block the main thread.

---

## Architecture Overview:

### ActiveDirectioryCommunication
ElasticSearchSolution/ 
├── Controller/ 
│ └── SimpleElasticTestController.cs # Contains API endpoints for indexing, retrieving, and searching documents in Elasticsearch 
├── HealthTest/ 
│ └── ElasticHealthCheck.cs # Health check for Elasticsearch connection 
├── Utils/ 
│ ├── Interfaces/ 
│ │ └── IElasticsearchClient.cs # Interface defining methods for Elasticsearch operations 
│ └── ElasticsearchClient.cs # Implementation of the Elasticsearch client, interacting with Elasticsearch APIs 
├── Program.cs # Main entry point for the application, including service registration and health checks 
├── ElasticSearchSolution.csproj # Project file for building the solution 
└── README.md---

## Core Components:

1. **UserDataExtractionActiveDirectory.cs**:
   - Extracts user information from LDAP, including attributes like `givenName`, `sn`, `displayName`, and `title`.
   - It uses the ILdapService interface to communicate with the LDAP server asynchronously.

2. **UserExistsInActiveDirectory.cs**:
   - This class checks if a user exists in Active Directory by querying for the `userAccountControl` attribute.
   - It returns true if the user account is enabled and false if it’s disabled or not found.

3. **LdapService.cs**:
   - Provides methods to establish LDAP connections and send search queries.
   - Uses asynchronous operations to ensure non-blocking LDAP queries and connection setups.

4. **ILdapService.cs**:
   - Interface defining methods for establishing LDAP connections and sending LDAP search requests.

