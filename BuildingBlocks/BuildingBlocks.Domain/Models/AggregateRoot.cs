using System.Collections.ObjectModel;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.Model
{
    public abstract class AggregateRoot : Entity 
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot() { }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => new ReadOnlyCollection<IDomainEvent>(_domainEvents);

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}