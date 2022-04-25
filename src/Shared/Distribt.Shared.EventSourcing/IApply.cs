namespace Distribt.Shared.EventSourcing;

public interface IApply<T>
{
    void Apply(T ev);
}