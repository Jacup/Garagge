namespace Application.Features.Users.Sessions;

public record SessionsDto(IEnumerable<SessionDto> Items)
{
    public IEnumerable<SessionDto> Items { get; set; } = Items;
}