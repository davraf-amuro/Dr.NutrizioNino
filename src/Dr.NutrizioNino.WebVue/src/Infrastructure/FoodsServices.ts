import type { ApiResponseDto } from '@/Interfaces/ApiResponseDto'
import type { FoodDashboardDto } from '../Interfaces/foods/FoodDashboardDto'
import axios from 'axios'

export class FoodsService {
  private async fetchData(): Promise<ApiResponseDto<FoodDashboardDto>> {
    try {
      const response = await axios.get('https://localhost:44360/foods/dashboard')
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async GetDashboard(): Promise<ApiResponseDto<FoodDashboardDto>> {
    const data = await this.fetchData()
    return data
  }

  public async FactoryNewFood(): Promise<FoodDashboardDto> {
    

}
