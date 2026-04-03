using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Extensions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class CategoryService(DrRepository drRepository)
{
    public async Task<IList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default) =>
        await drRepository.GetCategoriesAsync(CategoryExtensions.ToCategoryDto, ct).ConfigureAwait(false);

    public async Task<CategoryDto?> GetCategoryAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetCategoryAsync(id, CategoryExtensions.ToCategoryDto, ct).ConfigureAwait(false);

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = dto.Name };
        var created = await drRepository.CreateCategoryAsync(category, ct).ConfigureAwait(false);
        return CategoryExtensions.ToCategoryDto.Compile()(created);
    }

    public async Task<bool> UpdateCategoryAsync(Category category, CancellationToken ct = default) =>
        await drRepository.UpdateCategoryAsync(category, ct).ConfigureAwait(false);

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteCategoryAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsCategoryInUseAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.IsCategoryInUseAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsCategoryNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drRepository.IsCategoryNameTakenAsync(name, excludeId, ct).ConfigureAwait(false);
}
