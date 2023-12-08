using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentValidation;
using MassTransit;
using MediatR;
using Moq;
using System.Net;

namespace Monetus.UnitTests;

public class TransferenciaEventPublisherTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly Mock<IPublisher<TransferenciaEvent>> _publicador;
    private readonly TransferenciaEvent _event;
    private readonly TransferenciaEventPublisher _publisher;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Guid _id = Guid.NewGuid();
    public TransferenciaEventPublisherTests()
    {
        _event = _fixture.Create<TransferenciaEvent>();
        _publisher = _fixture.Build<TransferenciaEventPublisher>()
                        .OmitAutoProperties()
                        .Create();

        //_publisher = _fixture.Freeze<Mock<IPublisher<TransferenciaEvent>>>();
        //_publicador
        //    .Setup(x => x.SendAsync(_event, _cancellationToken))
        //    .ReturnsAsync(_id);
        //_publisher = _fixture.Build<TransferenciaEventPublisher>()
        //                //.OmitAutoProperties()
        //                .Create();
    }

    [Fact]
    public async Task Test1()
    {
        var id = await _publisher.SendAsync(_event, _cancellationToken);
        //_publisher.Verify(x => x.SendAsync(_event, _cancellationToken), Times.Once);
        Assert.NotNull(id);
    }
}
public class TransferenciaEventConsumerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    //private readonly Mock<IConsumer<TransferenciaEvent>> _consumerInterface;
    private readonly ConsumeContext<TransferenciaEvent> _event;
    private readonly TransferenciaEventConsumer _consumer;
    public TransferenciaEventConsumerTests()
    {
        _event = _fixture.Create<ConsumeContext<TransferenciaEvent>>();
        //_consumerInterface = _fixture.Freeze<Mock<IConsumer<TransferenciaEvent>>>();
        //_consumerInterface
        //    .Setup(x => x.Consume(_event))
        //    .Returns(Task.CompletedTask);
        _consumer = _fixture.Build<TransferenciaEventConsumer>()
                        .OmitAutoProperties()
                        .Create();
    }

    [Fact]
    public async Task Test1()
    {
        await _consumer.Consume(_event);
        //_consumerInterface.Verify(x => x.Consume(_event), Times.Once);
    }
}

public interface IEvent
{
    Guid IdEvent { get; }
}
public interface IPublisher<TEvent>
{
    Task<Guid> SendAsync(TEvent @event, CancellationToken cancellationToken);
}

public class TransferenciaEvent : IEvent
{
    public Guid IdEvent => Guid.NewGuid();
}
public class TransferenciaEventPublisher : IPublisher<TransferenciaEvent>
{
    public Task<Guid> SendAsync(TransferenciaEvent @event, CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
public class TransferenciaEventConsumer : IConsumer<TransferenciaEvent>
{
    public Task Consume(ConsumeContext<TransferenciaEvent> context)
    {
        return Task.CompletedTask;
    }
}


public class TransferenciaCommandHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly TransferenciaCommandHandler _handler;
    private readonly Mock<IPublisher<TransferenciaEvent>> _publisherMock;
    private readonly Mock<ITransferenciaRepository> _repositoryMock;
    private readonly TransferenciaCommand _command;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Guid _id = Guid.NewGuid();
    private readonly TransferenciaCommandResponse _response;
    private readonly Transferencia _contaCreditante;
    private readonly Transferencia _contaDebitante;
    public TransferenciaCommandHandlerTests()
    {
        _command = _fixture.Create<TransferenciaCommand>();
        _response = _fixture.Create<TransferenciaCommandResponse>();
        _contaCreditante = _fixture
            .Build<Transferencia>()
            .With(x => x.Nome, "Creditante")
            .Create();
        _contaDebitante = _fixture
            .Build<Transferencia>()
            .With(x => x.Nome, "Debitante")
            .Create();

        _publisherMock = _fixture.Freeze<Mock<IPublisher<TransferenciaEvent>>>();
        _publisherMock
            .Setup(x => x.SendAsync(_command.MapTo<TransferenciaEvent>(), _cancellationToken))
            .ReturnsAsync(_id);
        _repositoryMock = _fixture.Freeze<Mock<ITransferenciaRepository>>();
        _repositoryMock
            .Setup(x => x.GetPersonById(_command.ContaCreditante, _cancellationToken))
            .ReturnsAsync(_contaCreditante);
        _repositoryMock
            .Setup(x => x.GetPersonById(_command.ContaDebitante, _cancellationToken))
            .ReturnsAsync(_contaDebitante);

        _handler = _fixture.Create<TransferenciaCommandHandler>();
    }

    [Fact]
    public async Task Test1()
    {
        var id = await _handler.Handle(_command, _cancellationToken);
        _fixture.Freeze<Mock<ITransferenciaRepository>>()
                            .Verify(x => x.GetPersonById(It.IsAny<string>(), _cancellationToken), Times.Once);
        _fixture.Freeze<Mock<IPublisher<TransferenciaEvent>>>()
                            .Verify(x => x.SendAsync(_command.MapTo<TransferenciaEvent>(), _cancellationToken), Times.Once);
        Assert.NotNull(id);
    }
}
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public static class ObjectMapper
{
    private static readonly Lazy<IMapper> Lazy = new(() =>
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ObjectMapper).Assembly));
        return config.CreateMapper();
    });

    public static IMapper Mapper => Lazy.Value;

    public static T MapTo<T>(this object source) => Mapper.Map<T>(source);
}
public class AddressMapping : Profile
{
    public AddressMapping()
    {
        CreateMap<Transferencia, TransferenciaEvent>();
        CreateMap<TransferenciaCommand, TransferenciaEvent>();
    }
}
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
{

}
public record TransferenciaCommand(decimal Valor, string ContaDebitante, string ContaCreditante)
        : ICommand<TransferenciaCommandResponse>
{
}
public class TransferenciaCommandHandler : ICommandHandler<TransferenciaCommand, TransferenciaCommandResponse>
{
    private readonly IPublisher<TransferenciaEvent> _eventPublisher;
    private readonly ITransferenciaRepository _repository;

    public TransferenciaCommandHandler(IPublisher<TransferenciaEvent> eventPublisher, ITransferenciaRepository repository)
    {
        _eventPublisher = eventPublisher;
        _repository = repository;
    }

    public async Task<TransferenciaCommandResponse> Handle(TransferenciaCommand request, CancellationToken cancellationToken)
    {
        var contaDebitante = await _repository.GetPersonById(request.ContaDebitante, cancellationToken);
        if (contaDebitante is null)
            throw new Exception();

        var contaCreditante = await _repository.GetPersonById(request.ContaCreditante, cancellationToken);
        if (contaCreditante is null)
            throw new Exception();

        await _eventPublisher.SendAsync(request.MapTo<TransferenciaEvent>(), cancellationToken);
        return request.MapTo<TransferenciaCommandResponse>();
    }
}
public record TransferenciaCommandResponse(decimal Valor, string ContaDebitante, string ContaCreditante, DateTime DataCriacao);

public interface IEntity
{
    Guid IdEntity { get; }
}
public class Transferencia : IEntity
{
    public Guid IdEntity => Guid.NewGuid();
    public string Nome { get; set; }
}

public interface ITransferenciaRepository : ITransferenciaReadRepository, ITransferenciaWriteRepository
{

}
public interface ITransferenciaReadRepository
{
    Task<ICollection<Transferencia>> GetAll(CancellationToken cancellationToken);

    Task<Transferencia> GetPersonById(string personId, CancellationToken cancellationToken);
}
public interface ITransferenciaWriteRepository
{
    Task<Transferencia> AddPerson(Transferencia toCreate, CancellationToken cancellationToken);

    Task<Transferencia> UpdatePerson(int personId, string name, string email, CancellationToken cancellationToken);

    Task DeletePerson(int personId, CancellationToken cancellationToken);
}


public sealed class TransferenciaCommandValidator : AbstractValidator<TransferenciaCommand>
{
    public TransferenciaCommandValidator()
    {
        RuleFor(x => x.ContaDebitante).NotEmpty();

        RuleFor(x => x.ContaCreditante).NotEmpty();

        RuleFor(x => x.Valor).NotEmpty().GreaterThan(10.00M);
    }
}
