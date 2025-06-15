using System.Threading;
using System.Threading.Tasks;
using Distribt.Services.Emails.Application.DataAccess;
using Distribt.Services.Emails.Application.Email;
using Distribt.Services.Emails.Application.Services;
using Distribt.Services.Emails.Controllers;
using Moq;
using Xunit;

namespace Distribt.Tests.Services.Emails.Application.UseCases;

public class SendEmailUseCaseTest
{
    [Fact]
    public async Task WhenExecute_ThenStoresAndSendsEmail()
    {
        var repository = new Mock<IEmailRepository>();
        var provider = new Mock<IEmailProvider>();
        repository.Setup(r => r.AddEmail(It.IsAny<EmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("1");

        ISendEmailUseCase useCase = new SendEmailUseCase(repository.Object, provider.Object);
        EmailDto dto = new("from", "to", "subject", "body");

        bool result = await useCase.Execute(dto);

        Assert.True(result);
        repository.Verify(r => r.AddEmail(dto, It.IsAny<CancellationToken>()), Times.Once);
        provider.Verify(p => p.Send(dto, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.MarkAsSent("1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
