namespace Dr.NutrizioNino.Models.Dto;

public record CreateDishDto(string Name, IList<DishIngredientDto> Ingredients);
