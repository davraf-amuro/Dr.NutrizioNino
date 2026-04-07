namespace Dr.NutrizioNino.Models.Dto;

public record SimulationSectionDto(Guid Id, string Name, int DisplayOrder, bool IsActive);
public record CreateSimulationSectionDto(string Name);
public record UpdateSimulationSectionDto(string Name);
public record SimulationSectionReorderItem(Guid Id, int DisplayOrder);
