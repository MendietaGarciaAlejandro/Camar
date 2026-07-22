namespace Camar.Domain.Reservations
{
    public readonly record struct Period
    {
        public readonly DateTimeOffset Start { get; }
        public readonly DateTimeOffset End { get; }

        public Period(DateTimeOffset start, DateTimeOffset end)
        {
            if (end <= start)
            {
                throw new ArgumentException("El fin debe ser posterior al inicio.", nameof(end));
            }
            Start = start;
            End = end;
        }

        public bool Overlaps(Period other)
        {
            return Start < other.End && End > other.Start;
        }

    }
}
