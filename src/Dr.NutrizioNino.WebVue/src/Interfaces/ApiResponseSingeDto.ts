export interface ApiResponseSingeDto<T> {
  success: boolean
  data: T
  errors: Array<string>
}
