namespace Dr.NutrizioNino.Api.Dto
{
    public class ApiResponseMultipleDto<T>
    {
        public ApiResponseMultipleDto()
        {
            this.Data = [];
            this.Errors = [];
        }
        public bool Success { get; set; }
        public IList<T>? Data { get; set; }
        public IList<string> Errors { get; set; }

    }
}
