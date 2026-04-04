using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapsCategoriesEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/categories")
            .WithTags("Categories")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("", async (CategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetCategoriesAsync(ct);
            return result.Count > 0
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "No categories found."
                });
        })
            .WithName("GetCategories")
            .WithSummary("Get all categories")
            .WithDescription("Returns all available categories.")
            .Produces<IList<CategoryDto>>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("{id}", async (CategoryService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetCategoryAsync(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Category not found."
                });
        })
            .WithName("GetCategoryById")
            .WithSummary("Get category by id")
            .WithDescription("Returns a category for the provided identifier.")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (CategoryService service, CreateCategoryDto dto, CancellationToken ct) =>
        {
            if (await service.IsCategoryNameTakenAsync(dto.Name, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già una categoria con il nome \"{dto.Name}\"."
                });
            }

            var result = await service.CreateCategoryAsync(dto, ct);
            return Results.Ok(result);
        })
            .WithName("CreateCategory")
            .WithSummary("Create a new category")
            .WithDescription("Creates a new category and returns the created entity.")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);

        group.MapPut("{id}", async (CategoryService service, Guid id, Category category, CancellationToken ct) =>
        {
            if (await service.IsCategoryNameTakenAsync(category.Name, excludeId: category.Id, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già una categoria con il nome \"{category.Name}\"."
                });
            }

            var updated = await service.UpdateCategoryAsync(category, ct);
            return updated
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Category not found for update."
                });
        })
            .AddEndpointFilter(async (context, next) =>
            {
                var routeId = context.GetArgument<Guid>(1);
                var category = context.GetArgument<Category>(2);
                if (category.Id == Guid.Empty)
                {
                    category.Id = routeId;
                }

                if (category.Id != routeId)
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Category id in route and body must match."
                    });
                }

                return await next(context);
            })
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category")
            .WithDescription("Updates an existing category by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        group.MapGet("{id}/is-in-use", async (CategoryService service, Guid id, CancellationToken ct) =>
            Results.Ok(await service.IsCategoryInUseAsync(id, ct)))
            .WithName("IsCategoryInUse")
            .WithSummary("Check if category is in use")
            .WithDescription("Returns true if the category is referenced by one or more foods.")
            .Produces<bool>(StatusCodes.Status200OK);

        group.MapDelete("{id}", async (CategoryService service, Guid id, CancellationToken ct) =>
        {
            if (await service.IsCategoryInUseAsync(id, ct))
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "La categoria è in uso e non può essere eliminata."
                });

            var deleted = await service.DeleteCategoryAsync(id, ct);
            return deleted
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Category not found for delete."
                });
        })
            .WithName("DeleteCategory")
            .WithSummary("Delete a category")
            .WithDescription("Deletes an existing category by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        return endpoints;
    }
}
