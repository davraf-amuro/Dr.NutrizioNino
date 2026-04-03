namespace Dr.NutrizioNino.Models.Dto;

public record CategoryDto(Guid Id, string Name);

public record CreateCategoryDto(string Name);
