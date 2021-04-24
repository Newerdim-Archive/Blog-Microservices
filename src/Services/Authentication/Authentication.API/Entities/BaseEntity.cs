using System;

namespace Authentication.API.Entities
{
    public record BaseEntity
    {
        public int Id { get; init; }
        public DateTimeOffset LastChange { get; init; }
        public DateTimeOffset Created { get; init; }
    }
}
