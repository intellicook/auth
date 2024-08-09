using FluentAssertions;
using IntelliCook.Auth.Host.Extensions;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace IntelliCook.Auth.Host.UnitTests.Extensions;

public class AuthServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _serviceCollection = new();
    private readonly Mock<IConfiguration> _configurationMock = new();

    #region AddAuthOptions

    [Fact]
    public void AddAuthOptions_ValidConfiguration_ShouldAddOptions()
    {
        // Arrange
        var configurationProvider = new MemoryConfigurationProvider(new MemoryConfigurationSource())
        {
            { "Api:Title", "Test Title" },
            { "Api:Description", "Test Description" },
            { "Api:MajorVersion", "1" },
            { "Api:MinorVersion", "0" }
        };
        _configurationMock
            .Setup(x => x.GetSection(ApiOptions.SectionKey))
            .Returns(new ConfigurationSection(
                new ConfigurationRoot(new IConfigurationProvider[] { configurationProvider }),
                ApiOptions.SectionKey
            ));

        // Act
        _serviceCollection.AddAuthOptions<ApiOptions>(_configurationMock.Object);

        // Assert
        var serviceProvider = _serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddAuthOptions_MissingProperty_ShouldThrowException()
    {
        // Arrange
        var configurationProvider = new MemoryConfigurationProvider(new MemoryConfigurationSource())
        {
            { "Api:Description", "Test Description" },
            { "Api:MajorVersion", "1" },
            { "Api:MinorVersion", "0" }
        };
        _configurationMock
            .Setup(x => x.GetSection(ApiOptions.SectionKey))
            .Returns(new ConfigurationSection(
                new ConfigurationRoot(new IConfigurationProvider[] { configurationProvider }),
                ApiOptions.SectionKey
            ));

        // Act
        var act = () => _serviceCollection.AddAuthOptions<ApiOptions>(_configurationMock.Object);

        // Assert
        var ex = act.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().BeEquivalentTo("Title is required");
    }

    [Fact]
    public void AddAuthOptions_NotFound_ShouldThrowException()
    {
        // Arrange
        _configurationMock
            .Setup(x => x.GetSection(ApiOptions.SectionKey))
            .Returns(new ConfigurationSection(
                new ConfigurationRoot(Array.Empty<IConfigurationProvider>()),
                ApiOptions.SectionKey
            ));

        // Act
        var act = () => _serviceCollection.AddAuthOptions<ApiOptions>(_configurationMock.Object);

        // Assert
        var ex = act.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should()
            .BeEquivalentTo($"{nameof(ApiOptions)} options section '{ApiOptions.SectionKey}' not found");
    }

    #endregion

    #region AddAuthContext

    [Fact]
    public void AddAuthContext_InMemory_ShouldAddInMemoryDbContext()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            Name = "Test",
            UseInMemory = true
        };

        // Act
        _serviceCollection.AddAuthContext(options);

        // Assert
        var serviceProvider = _serviceCollection.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<AuthContext>();
        context.Database.ProviderName.Should().BeEquivalentTo("Microsoft.EntityFrameworkCore.InMemory");
    }

    [Fact]
    public void AddAuthContext_NotInMemory_ShouldAddSqlDbContext()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            Name = "Test",
            UseInMemory = false,
            ConnectionString = "TestConnectionString"
        };

        // Act
        _serviceCollection.AddAuthContext(options);

        // Assert
        var serviceProvider = _serviceCollection.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<AuthContext>();
        context.Database.ProviderName.Should().BeEquivalentTo("Microsoft.EntityFrameworkCore.SqlServer");
    }

    #endregion
}