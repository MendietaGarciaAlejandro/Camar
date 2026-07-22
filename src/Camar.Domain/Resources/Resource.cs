using Camar.Domain.Common;

namespace Camar.Domain.Resources
{
    public class Resource(string name, ResourceType type, int capacity)
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();
        public string Name { get; private set; } = Guard.NotBlank(name);
        public ResourceType Type { get; private set; } = type;
        public int Capacity { get; private set; } = Guard.Positive(capacity);
        public bool IsActive { get; private set; } = true;

        public void Deactivate()
        {
            if (!IsActive) throw new InvalidOperationException("El recurso ya está inactivo.");
            IsActive = false;
        }
    }
}
