cd ../
cd GIT
mkdir estudo-caso
code .

git init -b main
git add .
git commit -m "First commit"
#add a repo in github
dotnet new gitignore

#Api
dotnet add package Mediatr --version 12.2.0

# Application       - MediatR
# Application       - FluentValidation.DependecyInjection
# Infrastructure    - Microsoft.Extensions.DependencyInjection
# WebApi            - MediatR.Extensions.DependecyInjection
```
public interface ICommand<out TResponse> : IRequest<TResponse>
{

}
public interface IQuery<out TResponse> : IRequest<TResponse>
{

}
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
{

}
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
{
    
}

# Método Get
public record TransferenciaQuery(int TransferenciaId, string ContaDebitante, string ContaCreditante): IQuery<TransferenciaQueryResponse>

public class TransferenciaQueryHandler : IQueryHandler
{

}
public class TransferenciaQueryResponse : IResponse
{

}

# Método Post
public class TransferenciaCommand(decimal Valor, string ContaDebitante, string ContaCreditante) : ICommand<TransferenciaCommandResponse>
{

}
public class TransferenciaCommandHandler : ICommandHandler
{

}
public class TransferenciaCommandResponse : IResponse
{

}
public sealed class TransferenciaCommandValidator : AbstractValidator<TransferenciaCommand>
{
    public TransferenciaCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

# Repository

public interface ITransferenciaRepository: ITransferenciaReadRepository, ITransferenciaWriteRepository
{

}
public interface ITransferenciaReadRepository
{
    Task<ICollection<Person>> GetAll();
    
    Task<Person> GetPersonById(int personId);
}
public interface ITransferenciaWriteRepository
{
    Task<Person> AddPerson(Person toCreate);

    Task<Person> UpdatePerson(int personId, string name, string email);

    Task DeletePerson(int personId);
}

# Mensageria
public interface IEvent
{
    Guid IdEvent { get; }
}
public interface IEventProducer
{
    Task ConsumeAsync(IEvent @event, CancellationToken cancellationToken);
}
public interface IEventPublisher
{
    Task SendAsync(IEvent @event, CancellationToken cancellationToken);
}
```
