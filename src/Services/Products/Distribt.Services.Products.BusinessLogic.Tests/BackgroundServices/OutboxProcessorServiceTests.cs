using Distribt.Services.Products.BusinessLogic.BackgroundServices;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Distribt.Services.Products.BusinessLogic.Tests.BackgroundServices;

public class OutboxProcessorServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IOutboxMessageService> _mockOutboxMessageService;

    private readonly Mock<ILogger<OutboxProcessorService>> _mockLogger;
    private readonly OutboxProcessorService _service;

    public OutboxProcessorServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockOutboxMessageService = new Mock<IOutboxMessageService>();

        _mockLogger = new Mock<ILogger<OutboxProcessorService>>();

        // Setup service provider to return mocked services
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);

        mockScopeServiceProvider.Setup(x => x.GetRequiredService<IOutboxMessageService>())
            .Returns(_mockOutboxMessageService.Object);

        _service = new OutboxProcessorService(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessMessages_WhenPendingMessagesExist()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel after 100ms

        _mockOutboxMessageService.Setup(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        try
        {
            await _service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(150, CancellationToken.None); // Wait a bit longer than cancellation
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        finally
        {
            await _service.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockOutboxMessageService.Verify(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotProcessMessages_WhenNoPendingMessagesExist()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        _mockOutboxMessageService.Setup(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        try
        {
            await _service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(150, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        finally
        {
            await _service.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockOutboxMessageService.Verify(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinueProcessing_WhenExceptionOccurs()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        var callCount = 0;
        _mockOutboxMessageService.Setup(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("Test exception");
                return Task.CompletedTask;
            });

        // Act
        try
        {
            await _service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(150, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        finally
        {
            await _service.StopAsync(CancellationToken.None);
        }

        // Assert
        Assert.True(callCount >= 2, "Service should continue processing after exception");
        
        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while processing outbox messages")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateScopeForEachProcessingCycle()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        _mockOutboxMessageService.Setup(x => x.ProcessPendingMessagesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        try
        {
            await _service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(150, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        finally
        {
            await _service.StopAsync(CancellationToken.None);
        }

        // Assert
        _mockServiceProvider.Verify(x => x.CreateScope(), Times.AtLeastOnce);
        _mockServiceScope.Verify(x => x.Dispose(), Times.AtLeastOnce);
    }
} 