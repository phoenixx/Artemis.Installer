using Artemis.Installer.Services.Prerequisites;
using DotNetWindowsRegistry;
using Microsoft.Win32;
using Moq;
using Xunit;

namespace Artemis.Installer.UnitTests
{
    public class VersionParseTests
    {
        [InlineData("5.0.0")]
        [InlineData("6.0.0")]
        [InlineData("5.1.0")]
        [InlineData("6.0.2")]
        [InlineData("7.0.0")]
        [InlineData("5.2.1-preview.0.12345.6")]
        [InlineData("6.0.0-preview.5.21301.5")]
        [InlineData("7.0.0-preview.4.21225.7")]
        [Theory]
        public void When_DotNetVersionGreaterThanOrEqualTo5IsInstalled_Then_PreRequisiteIsMet(string version)
        {
            InMemoryRegistryKey registryKey = new InMemoryRegistryKey(RegistryView.Default, "sharedhost");
            registryKey.SetValue("Version", version);
            
            Mock<IRegistryKey> mockRegistryKey = new Mock<IRegistryKey>();
            mockRegistryKey.Setup(x => x.OpenSubKey(It.IsAny<string>()))
                .Returns(registryKey);
            
            Mock<IRegistry> mockRegistry = new Mock<IRegistry>();
            mockRegistry.Setup(x => x.OpenBaseKey(It.IsAny<RegistryHive>(), It.IsAny<RegistryView>()))
                .Returns(mockRegistryKey.Object);
            
            IPrerequisite sut = new DotnetPrerequisite(mockRegistry.Object);
            bool isMet = sut.IsMet();
            Assert.True(isMet);
        }

        [InlineData("2.0.0")]
        [InlineData("2.1.0")]
        [InlineData("3.0.1")]
        [InlineData("3.0.1-preview.5.21301.5")]
        [Theory]
        public void When_DotNetVersionLowerThan5IsInstalled_Then_PreRequisiteIsNotMet(string version)
        {
            InMemoryRegistryKey registryKey = new InMemoryRegistryKey(RegistryView.Default, "sharedhost");
            registryKey.SetValue("Version", version);
            
            Mock<IRegistryKey> mockRegistryKey = new Mock<IRegistryKey>();
            mockRegistryKey.Setup(x => x.OpenSubKey(It.IsAny<string>()))
                .Returns(registryKey);
            
            Mock<IRegistry> mockRegistry = new Mock<IRegistry>();
            mockRegistry.Setup(x => x.OpenBaseKey(It.IsAny<RegistryHive>(), It.IsAny<RegistryView>()))
                .Returns(mockRegistryKey.Object);
            
            IPrerequisite sut = new DotnetPrerequisite(mockRegistry.Object);
            bool isMet = sut.IsMet();
            Assert.False(isMet);
        }

        [Fact]
        public void When_DotNetRegistryKeyIsNotFound_Then_PreRequisiteIsNotMet()
        {
            Mock<IRegistryKey> mockRegistryKey = new Mock<IRegistryKey>();
            mockRegistryKey.Setup(x => x.OpenSubKey(It.IsAny<string>()))
                .Returns((IRegistryKey)null);
            
            Mock<IRegistry> mockRegistry = new Mock<IRegistry>();
            mockRegistry.Setup(x => x.OpenBaseKey(It.IsAny<RegistryHive>(), It.IsAny<RegistryView>()))
                .Returns(mockRegistryKey.Object);
            
            IPrerequisite sut = new DotnetPrerequisite(mockRegistry.Object);
            bool isMet = sut.IsMet();
            Assert.False(isMet);
        }
    }
}