export interface ApiResponseDto<T> {
  success: boolean
  data: Array<T>
}
