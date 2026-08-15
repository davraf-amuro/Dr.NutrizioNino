namespace Dr.NutrizioNino.Models.Dto;

public record CreateRecipeDto(string Name, IList<RecipeIngredientDto> Ingredients);
