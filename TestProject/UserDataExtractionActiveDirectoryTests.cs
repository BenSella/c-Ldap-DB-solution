using ActiveDirectioryCommunication.Utils.Interfaces;
using ActiveDirectioryCommunication.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.DirectoryServices.Protocols;
using System.Collections.Generic;
using Xunit;
using System.IO;
using System.Linq;
using ActiveDirectioryCommunication.Objects;

namespace ActiveDirectioryCommunication.Tests
{
    public class UserDataExtractionActiveDirectoryTests
    {
        private readonly Mock<ILdapService> _ldapServiceMock;
        private readonly Mock<ILogger<UserDataExtractionActiveDirectory>> _loggerMock;
        private readonly UserDataExtractionActiveDirectory _userDataExtractor;
        private readonly List<FakeLdapUser> _fakeUserData;

        public UserDataExtractionActiveDirectoryTests()
        {
            _ldapServiceMock = new Mock<ILdapService>();
            _loggerMock = new Mock<ILogger<UserDataExtractionActiveDirectory>>();
            _userDataExtractor = new UserDataExtractionActiveDirectory(
                _ldapServiceMock.Object,
                "DC=example,DC=com",
                _loggerMock.Object);

            var currentDirectory = Directory.GetCurrentDirectory();
            var solutionRoot = Directory.GetParent(currentDirectory).Parent.Parent.Parent.FullName;
            string _jsonFilePath = Path.Combine(solutionRoot, "ActiveDirectioryCommunication", "fake-ldap-data.json");

            // Deserialize the JSON file into a list of FakeLdapUser objects
            var fakeLdapData = JsonConvert.DeserializeObject<FakeLdapData>(File.ReadAllText(_jsonFilePath));
            _fakeUserData = fakeLdapData?.Users ?? new List<FakeLdapUser>();
        }

        [Fact]
        public async Task ExtractData_UserFound_ReturnsUserData()
        {
            // Arrange
            var user = _fakeUserData.FirstOrDefault(u => u.sAMAccountName == "jdoe");
            if (user != null)
            {
                // Mocking the attributes for the user
                var mockAttributes = new DirectoryAttributeCollection
                {
                    new DirectoryAttribute("givenName", user.givenName),
                    new DirectoryAttribute("sn", user.sn),
                    new DirectoryAttribute("displayName", user.displayName),
                    new DirectoryAttribute("title", user.title)
                };

                // Mocking the SearchResultEntry
                var mockSearchResultEntry = new Mock<SearchResultEntry>();
               // mockSearchResultEntry.Setup(e => e.Attributes);
               // mockSearchResultEntry.Setup(e => e.DistinguishedName).Returns("CN=John Doe,OU=Users,DC=example,DC=com");

                // Create a SearchResultEntryCollection and add the mock SearchResultEntry
                var mockSearchResultEntryCollection = new Mock<SearchResultEntryCollection>();
                //mockSearchResultEntryCollection
                //    .Setup(c => c[0])
                //    .Returns(mockSearchResultEntry.Object);
                //mockSearchResultEntryCollection
                //    .Setup(c => c.Count)
                //    .Returns(1);

                // Mock the SearchResponse to return the mock entry collection
                var mockSearchResponse = new Mock<SearchResponse>();
                mockSearchResponse.Setup(r => r.Entries).Returns(mockSearchResultEntryCollection.Object);

                // Mock the ILdapService to return the mock SearchResponse
                _ldapServiceMock
                    .Setup(x => x.SendLdapRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                    .Returns(mockSearchResponse.Object);
            }

            // Act
            var result = await _userDataExtractor.ExtractData("jdoe");

            // Assert
            Assert.Equal("John", result.UserName);
            Assert.Equal("Doe", result.UserFamily);
            Assert.Equal("John Doe", result.UserFullName);
            Assert.Equal("Software Engineer", result.UserTitle);
        }
    }
}
