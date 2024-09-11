using ActiveDirectioryCommunication.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

public class UserExistsInActiveDirectoryTests
{
    private readonly UserExistsInActiveDirectory _userExistsInActiveDirectory;
    private readonly List<FakeLdapUser> _fakeUserData;
public UserExistsInActiveDirectoryTests()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionRoot = Directory.GetParent(currentDirectory).Parent.Parent.Parent.FullName;
        string _jsonFilePath = Path.Combine(solutionRoot, "ActiveDirectioryCommunication", "fake-ldap-data.json");

        // Deserialize the JSON file into a list of FakeLdapUser objects
        var fakeLdapData = JsonConvert.DeserializeObject<FakeLdapData>(File.ReadAllText(_jsonFilePath));
        _fakeUserData = fakeLdapData?.Users ?? new List<FakeLdapUser>();
       
        // Create the Fake LDAP Service
        var fakeLdapService = new FakeLdapService(_fakeUserData);

        // Mock the logger
        var mockLogger = new Mock<ILogger<UserExistsInActiveDirectory>>();

        // Instantiate UserExistsInActiveDirectory with the fake service
        _userExistsInActiveDirectory = new UserExistsInActiveDirectory(fakeLdapService, "DC=example,DC=com", mockLogger.Object);
    }

    [Fact]
    public async Task UserExists_ShouldReturnTrue_WhenUserIsEnabled()
    {
        // Act
        var result = await _userExistsInActiveDirectory.ExtractData("jdoe");

        // Assert
        Assert.True(result);
    }
    

    [Fact]
    public async Task UserExists_ShouldReturnFalse_WhenUserIsDisabled()
    {
        // Act
        var result = await _userExistsInActiveDirectory.ExtractData("asmith");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserExists_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Act
        var result = await _userExistsInActiveDirectory.ExtractData("nonexistentuser");

        // Assert
        Assert.False(result);
    }
}
