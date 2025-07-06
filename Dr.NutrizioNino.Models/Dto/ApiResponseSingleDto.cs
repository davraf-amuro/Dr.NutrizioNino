namespace Dr.NutrizioNino.Api.Dto
{
    public class ApiResponseSingleDto<T>
    {
        public ApiResponseSingleDto()
        {
            this.Errors = [];
        }
        public bool Success { get; set; }
        public T? Data { get; set; }
        public IList<string> Errors { get; set; }

    }
}
