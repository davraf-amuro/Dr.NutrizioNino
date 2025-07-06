export interface ApiResponseMultipleDto<T> {
  success: boolean
  data: Array<T>
  errors: Array<string>
}
